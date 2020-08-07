using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;
using System.Linq.Expressions;
using System.Reflection;
using System.Web.Http;
using System.Net.Http;
using System.Net;
using System.Drawing.Text;
using System.Threading.Tasks.Dataflow;
using System.IO;
using System.Drawing.Imaging;
using System.Xml;

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

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "SetWindowsHookEx", SetLastError = true)]
        static extern IntPtr SetWindowsHookEx(int idHook, Delegate lpfn, IntPtr hMod, uint dwThreadId);

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
        private bool updateSize = false;
        private decimal targetFps = 30;

        class Streamer : ApiController
        {
            private String boundary;
            private bool die;
            private int port;

            private BroadcastBlock<Lazy<byte[]>> currentImageEncoded;
            private Thread serverThread;
            private HttpListener host;

            public Streamer(string boundary, int? port)
            {
                this.boundary = boundary;
                this.port = port.Value;
                currentImageEncoded = new BroadcastBlock<Lazy<byte[]>>(null);
                serverThread = new Thread(delegate () { Run(); }) { Priority = ThreadPriority.Lowest };
                die = false;
            }


            public void Run()
            {
                host = new HttpListener();
                try
                {
                    host.Prefixes.Add($"http://*:{port}/");
                    host.Start();
                    while (!die)
                    {
                        var context = host.GetContext();
                        var request = context.Request;
                        var response = context.Response;
                        response.Headers.Add($"Content-Type:multipart/x-mixed-replace; boundary={boundary}");
                        response.StatusCode = 200;
                        new Thread(delegate () { streamCallback(response.OutputStream); }) { Priority = ThreadPriority.BelowNormal }.Start();
                    }
                } finally
                {
                    if (host.IsListening)
                    {
                        host.Prefixes.Clear();
                        host.Stop();
                        host.Close();
                    }
                }
            }

            public void Start()
            {
                serverThread.Start();
            }

            public void Stop()
            {
                die = true;
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

            public void SetCurrentImage(Bitmap image)
            {
                currentImageEncoded.SendAsync(new Lazy<byte[]>(() =>
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        image.Save(ms, ImageFormat.Jpeg);
                        return ms.ToArray();
                    }
                }, true));
            }

            private byte[] getCurrentImage()
            {
                var received = currentImageEncoded.Receive();
                return received.Value;
            }

            private void streamCallback(Stream stream)
            {
                try
                {
                    while (!die)
                    {
                        var imageData = getCurrentImage();

                        writeHeader(stream, imageData.Length);
                        stream.Write(imageData, 0, imageData.Length);
                        writeFooter(stream);
                        stream.Flush();
                    }
                }
                catch (HttpListenerException) { }
            }

        }

        public WindowFinder()
        {
            InitializeComponent();
            this.MouseDown += new MouseEventHandler(this.formMouseDown);
            this.MouseUp += new MouseEventHandler(this.formMouseUp);
            // this.MouseCaptureChanged += new Mous

        }

        protected override void OnClosed(EventArgs e)
        {
            if (resetFunction != null)
            {
                resetFunction();
            }
            base.OnClosed(e);
        }

        public void formMouseDown(Object sender, MouseEventArgs e)
        {
            if (!gotWindowPanel.Visible)
                WindowFinder.SetCapture(this.Handle);
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
                    var streamer = new Streamer("SOME_BOUNDARY_WHICH_IS_NOT_PRESENT_IN_THE_FILE", 8181);
                    resetFunction = (delegate
                    {
                        resetFunction = null;
                        streamer.Stop();
                        workerThread.Abort();
                        if (posBefore.HasValue)
                        {
                            var value = posBefore.Value;
                            MoveWindow(activeWindow, value.Left, value.Top, value.Right - value.Left, value.Bottom - value.Top, false);
                            if (!IsDisposed)
                            {
                                gotWindowPanel.Visible = false;
                            }
                        }
                        resetFunction = null;
                    });

                    Action doResetFunction = delegate
                    {
                        if (resetFunction != null)
                        {
                            _ = Invoke(resetFunction);
                        }
                    };

                    streamer.Start();
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
                        sizeWidth.Maximum = screen.Width;
                        sizeHeight.Maximum = screen.Height;
                        if (posBefore == null)
                        {
                            posBefore = rect;
                        }
                        if (updateSize)
                        {
                            updateSize = false;
                            MoveWindow(activeWindow, screen.Width, screen.Height, (int)sizeWidth.Value, (int)sizeHeight.Value, false);
                            continue;
                        }
                        var ptr = memoryGraph.GetHdc();
                        var printed = PrintWindow(activeWindow, ptr, 0);
                        memoryGraph.ReleaseHdc();
                        if (printed)
                        {
                            var imgWidth = bmp.Width;
                            var imgHeight = bmp.Height;
                            streamer.SetCurrentImage(bmp);
                            try
                            {
                                _ = Invoke((Action)(delegate
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
                                    gotWindowPanel.Visible = true;
                                }));
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
                });
                workerThread.Priority = ThreadPriority.Lowest;
                workerThread.Start();
                return true;
            }
            return false;
        }

        public void formMouseUp(Object sender, MouseEventArgs e)
        {
            WindowFinder.ReleaseCapture();

            var hwnd = WindowFromPoint(System.Windows.Forms.Control.MousePosition);
            if (hwnd != Handle)
            {
                streamHwnd(hwnd);
            }
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
    }
}
