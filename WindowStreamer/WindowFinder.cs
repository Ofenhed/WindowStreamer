﻿using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using System.Web.Http;
using System.Windows.Forms;

namespace WindowStreamer
{

    public partial class WindowFinder : Form
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr SetCapture(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        static extern IntPtr WindowFromPoint(System.Drawing.Point p);

        [DllImport("user32.dll")]
        static extern IntPtr GetParent(IntPtr hwnd);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool PrintWindow(IntPtr hwnd, IntPtr hDC, uint nFlags);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;        // x position of upper-left corner
            public int Top;         // y position of upper-left corner
            public int Right;       // x position of lower-right corner
            public int Bottom;      // y position of lower-right corner
        }

        private IntPtr activeWindow;
        private RECT? posBefore;
        private Thread workerThread;
        private Action resetFunction;
        private EncoderParameters encoderParameters;
        private bool updateSize = false;
        private bool hideStreamed = false;
        private decimal targetFps = 30;

        class Streamer
        {
            private String boundary;
            private bool die;
            private int port;

            private BroadcastBlock<Lazy<byte[]>> currentImageEncoded;
            private Thread serverThread = null;
            private HttpListener host;
            private ImageCodecInfo imageCodecInfo;

            public Streamer(string boundary, int? port)
            {
                ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
                foreach (ImageCodecInfo codec in codecs)
                {
                    if (codec.FormatID == ImageFormat.Jpeg.Guid)
                    {
                        imageCodecInfo = codec;
                        break;
                    }
                }

                this.boundary = boundary;
                this.port = port.Value;
                currentImageEncoded = new BroadcastBlock<Lazy<byte[]>>(null);
                die = false;
            }


            public void Run(Action<Uri> callback)
            {
                host = new HttpListener();
                try
                {
                    var stream_num = 0;
                    while (true)
                    {
                        try
                        {
                            var url = new UriBuilder()
                            {
                                Scheme = "http",
                                Host = "*",
                                Port = port,
                                Path = $"/stream/{stream_num}/"
                            };
                            host.Prefixes.Add(url.ToString());
                            host.Start();
                            url.Host = "127.0.0.1";
                            callback(url.Uri);
                            break;
                        } catch
                        {
                            host.Prefixes.Clear();
                            ++stream_num;
                        }
                    }
                    while (!die)
                    {
                        var context = host.GetContext();
                        var request = context.Request;
                        var response = context.Response;
                        response.Headers.Add($"Content-Type:multipart/x-mixed-replace; boundary={boundary}");
                        response.StatusCode = 200;
                        new Thread(delegate () { streamCallback(response.OutputStream); }) { Priority = ThreadPriority.BelowNormal, IsBackground = true }.Start();
                    }
                }
                finally
                {
                    if (host.IsListening)
                    {
                        host.Prefixes.Clear();
                        host.Stop();
                        host.Close();
                    }
                }
            }

            public void Start(Action<Uri> callback)
            {
                if (serverThread == null)
                {
                    serverThread = new Thread(delegate () { Run(callback); }) { Priority = ThreadPriority.Lowest, IsBackground = true };
                    serverThread.Start();
                }
            }

            public void Stop()
            {
                die = true;
                if (serverThread == null)
                {
                    return;
                }
                if (serverThread.IsAlive)
                {
                    serverThread.Abort();
                }
                if (host != null)
                {
                    host.Abort();
                }
            }

            private static void writeString(Stream stream, String text) // This should be replaced with the release of .NET 5
            {
                var encoded = Encoding.ASCII.GetBytes(text);
                stream.Write(encoded, 0, encoded.Length);
            }

            private void writeHeader(Stream stream, int length)
            {
                writeString(stream, "--");
                writeString(stream, boundary);
                writeString(stream, $"\r\nContent-Type:image/jpeg\r\nContent-Length:{length}\r\n\r\n");
            }

            private void writeFooter(Stream stream)
            {
                writeString(stream, "\r\n");
            }

            public void SetCurrentImage(Bitmap image, EncoderParameters encoderParameters)
            {
                currentImageEncoded.SendAsync(new Lazy<byte[]>(() =>
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        image.Save(ms, imageCodecInfo, encoderParameters);
                        image.Dispose();
                        return ms.ToArray();
                    }
                }, true));
            }

            private byte[] getCurrentImage()
            {
                var received = currentImageEncoded.Receive();
                return received.Value;
            }

            class BroadcastObserver<T> : System.IObserver<T>
            {
                private Semaphore onReadySem;
                private Action<T> onDataAction;

                public BroadcastObserver(Semaphore ready, Action<T> onData)
                {
                    onReadySem = ready;
                    onDataAction = onData;
                }

                public void OnCompleted()
                {
                    onReadySem.Release();
                }

                public void OnError(Exception error)
                {
                    onReadySem.Release();
                }

                public void OnNext(T value)
                {
                    onDataAction(value);
                }
            }

            private void streamCallback(Stream stream)
            {
                Semaphore semaphore = new Semaphore(0, 1);
                Action<Lazy<byte[]>> handler = imageData =>
                {
                    try
                    {
                        var value = imageData.Value;
                        writeHeader(stream, value.Length);
                        stream.Write(value, 0, value.Length);
                        writeFooter(stream);
                        stream.Flush();
                    }
                    catch (HttpListenerException) { }
                    catch (ProtocolViolationException)
                    {
                        semaphore.Release();
                    }
                };
                var unsubscriber = currentImageEncoded.AsObservable().Subscribe(new BroadcastObserver<Lazy<byte[]>>(semaphore, handler));
                semaphore.WaitOne();
                unsubscriber.Dispose();
                stream.Close();
            }

        }

        private Streamer httpStreamer;

        public WindowFinder()
        {
            InitializeComponent();
            encoderParameters = new EncoderParameters(1);
            var parameter = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)imageQuality.Value);
            encoderParameters.Param[0] = parameter;
            var dragHandler = new MouseEventHandler(this.formMouseDown);
            this.MouseDown += dragHandler;
            hiddenWindowLabel.MouseDown += dragHandler;
            visibleWindowLabel.MouseDown += dragHandler;
            gotWindowPanel.MouseDown += dragHandler;
            this.MouseUp += new MouseEventHandler(this.formMouseUp);
            this.MouseCaptureChanged += new EventHandler(this.formMouseCaptureAborted);
            httpStreamer = new Streamer("SOME_BOUNDARY_WHICH_IS_NOT_PRESENT_IN_THE_FILE", 8181);
            noWindowPanel.Dock = DockStyle.Fill;
            gotWindowPanel.Dock = DockStyle.Fill;
            httpStreamer.Start((uri) =>
            {
                _ = Invoke((Action)delegate
                {
                    serverUrlLabel.Text = uri.ToString();
                });
            });
            // this.MouseCaptureChanged += new Mous

        }

        protected override void OnClosed(EventArgs e)
        {
            httpStreamer.Stop();
            if (resetFunction != null)
            {
                resetFunction();
            }
            base.OnClosed(e);
        }

        public void formMouseCaptureAborted(Object sender, EventArgs e)
        {
            Cursor.Current = Cursors.Default;
        }

        public void formMouseDown(Object sender, MouseEventArgs e)
        {
            if (noWindowPanel.Visible)
            {
                if (sender == visibleWindowLabel)
                {
                    hideStreamedCheckbox.Checked = (hideStreamed = false);
                }
                else if (sender == hiddenWindowLabel)
                {
                    hideStreamedCheckbox.Checked = (hideStreamed = true);
                }
                Cursor.Current = Cursors.Cross;
                WindowFinder.SetCapture(this.Handle);
            }
        }

        public bool doScreenshot(IntPtr hwnd, Bitmap output)
        {
            var memoryGraph = Graphics.FromImage(output);
            var ptr = memoryGraph.GetHdc();
            memoryGraph.ReleaseHdc();
            return false;
        }

        public bool streamHwnd(IntPtr hwnd)
        {
            activeWindow = hwnd;
            var hwnd2 = GetParent(activeWindow);
            if (hwnd2 != IntPtr.Zero) { activeWindow = hwnd2; }
            if (activeWindow != IntPtr.Zero)
            {
                workerThread = new Thread(delegate ()
                {
                    posBefore = null;
                    resetFunction = (delegate
                    {
                        resetFunction = null;
                        workerThread.Abort();
                        if (posBefore.HasValue)
                        {
                            var value = posBefore.Value;
                            MoveWindow(activeWindow, value.Left, value.Top, value.Right - value.Left, value.Bottom - value.Top, false);
                        }
                        posBefore = null;
                        if (!IsDisposed)
                        {
                            gotWindowPanel.Visible = false;
                            noWindowPanel.Visible = true;
                        }
                    });

                    Action doResetFunction = delegate
                    {
                        if (resetFunction != null)
                        {
                            _ = Invoke(resetFunction);
                        }
                    };

                    var timer = new System.Diagnostics.Stopwatch();
                    timer.Start();
                    while (true)
                    {
                        var rect = new RECT();
                        var got_size = GetWindowRect(activeWindow, out rect);
                        if (!got_size)
                        {
                            doResetFunction();
                            break;
                        }
                        Bitmap bmp = new Bitmap(rect.Right - rect.Left, rect.Bottom - rect.Top);
                        Graphics memoryGraph = Graphics.FromImage(bmp);

                        var screen = SystemInformation.VirtualScreen;
                        var windowPosition = new Point(screen.Width, screen.Height);
                        if (!hideStreamed)
                        {
                            if (updateSize && rect.Left >= screen.Width && rect.Top >= screen.Height && posBefore != null)
                            {
                                windowPosition.X = posBefore.Value.Left;
                                windowPosition.Y = posBefore.Value.Top;
                            }
                            else
                            {
                                windowPosition.X = rect.Left;
                                windowPosition.Y = rect.Top;
                            }
                        }
                        sizeWidth.Maximum = screen.Width * 10;
                        sizeHeight.Maximum = screen.Height * 10;
                        if (posBefore == null)
                        {
                            posBefore = rect;
                            _ = Invoke((Action)delegate
                            {
                                gotWindowPanel.Visible = true;
                                noWindowPanel.Visible = false;
                                sizeWidth.Value = bmp.Width;
                                sizeHeight.Value = bmp.Height;
                            });
                            MoveWindow(activeWindow, windowPosition.X, windowPosition.Y, bmp.Width, bmp.Height, false);
                        }
                        else if (updateSize)
                        {
                            updateSize = false;
                            MoveWindow(activeWindow, windowPosition.X, windowPosition.Y, (int)sizeWidth.Value, (int)sizeHeight.Value, false);
                            continue;
                        }
                        var ptr = memoryGraph.GetHdc();
                        var printed = PrintWindow(activeWindow, ptr, 0);
                        memoryGraph.ReleaseHdc();
                        if (printed)
                        {
                            var imgWidth = bmp.Width;
                            var imgHeight = bmp.Height;
                            httpStreamer.SetCurrentImage(bmp, encoderParameters);
                            try
                            {
                                _ = Invoke((Action)delegate
                                {
                                    if (IsDisposed)
                                    {
                                        return;
                                    }
                                    if (!updateSize)
                                    {
                                        sizeWidth.Value = imgWidth;
                                        sizeHeight.Value = imgHeight;
                                    }
                                });
                            }
                            catch (System.InvalidOperationException)
                            {
                                break;
                            }
                        }
                        else
                        {
                            doResetFunction();
                            break;
                        }
                        timer.Stop();
                        var sleepTarget = 1000 / targetFps - timer.ElapsedMilliseconds;
                        if (sleepTarget > 0)
                        {
                            Thread.Sleep((int)sleepTarget);
                        }
                        timer.Restart();
                    }
                })
                {
                    Priority = ThreadPriority.Lowest,
                    IsBackground = true
                };
                workerThread.Start();
                return true;
            }
            return false;
        }

        public void formMouseUp(Object sender, MouseEventArgs e)
        {
            WindowFinder.ReleaseCapture();

            var hwnd = WindowFromPoint(System.Windows.Forms.Control.MousePosition);
            if (FromHandle(hwnd) == null)
                streamHwnd(hwnd);
        }

        private void sizeWidth_ValueChanged(object sender, EventArgs e)
        {
            updateSize = true;
        }

        private void sizeHeight_ValueChanged(object sender, EventArgs e)
        {
            updateSize = true;
        }

        private void targetFpsEntry_ValueChanged(object sender, EventArgs e)
        {
            targetFps = ((NumericUpDown)sender).Value;
        }

        private void stopStreaming_Click(object sender, EventArgs e)
        {
            if (resetFunction != null)
                resetFunction();
        }

        private void imageQuality_ValueChanged(object sender, EventArgs e)
        {
            var parameter = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality,
                (long)((NumericUpDown)sender).Value);
            encoderParameters.Param[0] = parameter;
        }

        private void hideStreamedCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            hideStreamed = ((CheckBox)sender).Checked;
            updateSize = true;
        }

        private void serverUrlLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(new ProcessStartInfo(((LinkLabel)sender).Text));
            e.Link.Visited = true;
        }
    }
}
