using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks.Dataflow;
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

        [DllImport("gdi32.dll", EntryPoint = "BitBlt", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool BitBlt([In] IntPtr hdc, int nXDest, int nYDest, int nWidth, int nHeight, [In] IntPtr hdcSrc, int nXSrc, int nYSrc, int dwRop);

        [DllImport("gdi32.dll")]
        static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);

        [DllImport("user32.dll")]
        static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern bool ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("gdi32.dll", SetLastError = true)]
        static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll", EntryPoint = "SelectObject")]
        public static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject(IntPtr hObject);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        public const int HWND_NOTOPMOST = -2;
        public const int HWND_TOPMOST = -1;
        public const int HWND_BOTTOM = 1;
        public const uint SWP_NOREDRAW = 0x8;
        public const uint SWP_NOACTIVE = 0x10;
        public const uint SWP_NOMOVE = 0x2;
        public const uint SWP_NOSIZE = 0x1;
        public const uint SWP_NOZORDER = 0x4;

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

        // This helper static method is required because the 32-bit version of user32.dll does not contain this API
        // (on any versions of Windows), so linking the method will fail at run-time. The bridge dispatches the request
        // to the correct function (GetWindowLong in 32-bit mode and GetWindowLongPtr in 64-bit mode)
        public static IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
        {
            if (IntPtr.Size == 8)
                return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
            else
                return new IntPtr(SetWindowLong32(hWnd, nIndex, dwNewLong.ToInt32()));
        }

        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        private static extern int SetWindowLong32(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
        private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        private static extern IntPtr GetWindowLongPtr32(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
        private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

        // This static method is required because Win32 does not support
        // GetWindowLongPtr directly
        public static IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex)
        {
            if (IntPtr.Size == 8)
                return GetWindowLongPtr64(hWnd, nIndex);
            else
                return GetWindowLongPtr32(hWnd, nIndex);
        }

        const int GWL_EXSTYLE = -20;
        const long WS_EX_TOOLWINDOW = 0x80L;

        class Streamer
        {
            private String boundary;
            private bool die;
            private int port;

            private BroadcastBlock<Lazy<byte[]>> currentImageEncoded;
            private Bitmap currentImage;
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
                        }
                        catch
                        {
                            host = new HttpListener();
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

            private byte[] encodeImage(EncoderParameters encoderParameters)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    var myImage = Interlocked.Exchange(ref currentImage, null);
                    if (myImage == null)
                        return null;
                    myImage.Save(ms, imageCodecInfo, encoderParameters);
                    var maybeNewImage = Interlocked.Exchange(ref currentImage, myImage);
                    // This tries to recycle the old bitmap. If a new bitmap has
                    // already been placed we'll simply drop it, since the 
                    // alternative is ta draw that frame instead, but that
                    // may lead to starvation, since the producer may be
                    // faster than the consumer every frame.
                    if (maybeNewImage != null)
                        maybeNewImage.Dispose();
                    return ms.ToArray();
                }
            }

            public Bitmap SetCurrentImage(Bitmap image, EncoderParameters encoderParameters)
            {
                var lastImage = Interlocked.Exchange(ref currentImage, image);

                currentImageEncoded.SendAsync(new Lazy<byte[]>(() => encodeImage(encoderParameters)));

                return lastImage;
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
                        if (imageData.Value != null)
                        {
                            var value = imageData.Value;
                            writeHeader(stream, value.Length);
                            stream.Write(value, 0, value.Length);
                            writeFooter(stream);
                            stream.Flush();
                        }
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

        public bool streamHwnd(IntPtr hwnd)
        {
            activeWindow = hwnd;
            while (true)
            {
                var hwnd2 = GetParent(activeWindow);
                if (hwnd2 == IntPtr.Zero)
                    break;
                activeWindow = hwnd2;
            }

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
                            var styleBefore = GetWindowLongPtr(activeWindow, GWL_EXSTYLE);
                            var newStyle = new IntPtr(styleBefore.ToInt64() & ~WS_EX_TOOLWINDOW);
                            SetWindowLongPtr(activeWindow, GWL_EXSTYLE, newStyle);

                            SetWindowPos(activeWindow, IntPtr.Zero, value.Left, value.Top, value.Right - value.Left, value.Bottom - value.Top, SWP_NOACTIVE | SWP_NOREDRAW | SWP_NOZORDER);
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
                    Bitmap recycledBitmap = null;
                    while (true)
                    {
                        var rect = new RECT();
                        var got_size = GetWindowRect(activeWindow, out rect);
                        if (!got_size)
                        {
                            doResetFunction();
                            break;
                        }
                        var windowSize = new Size(rect.Right - rect.Left, rect.Bottom - rect.Top);

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
                                windowPosition.X = Math.Max(0, Math.Min(screen.Width - windowSize.Width, rect.Left));
                                windowPosition.Y = Math.Max(0, Math.Min(screen.Height - windowSize.Height, rect.Top));
                                if (windowPosition.X != rect.Left || windowPosition.Y != rect.Top)
                                {
                                    updateSize = true;
                                }
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
                                sizeWidth.Value = windowSize.Width;
                                sizeHeight.Value = windowSize.Height;
                            });
                            var styleBefore = GetWindowLongPtr(activeWindow, GWL_EXSTYLE);
                            var newStyle = new IntPtr(hideStreamed ? styleBefore.ToInt64() | WS_EX_TOOLWINDOW : styleBefore.ToInt64() & ~WS_EX_TOOLWINDOW);
                            SetWindowLongPtr(activeWindow, GWL_EXSTYLE, newStyle);
                            SetWindowPos(activeWindow, IntPtr.Zero, windowPosition.X, windowPosition.Y, windowSize.Width, windowSize.Height, SWP_NOACTIVE | SWP_NOZORDER);
                        }
                        else if (updateSize)
                        {
                            updateSize = false;
                            var styleBefore = GetWindowLongPtr(activeWindow, GWL_EXSTYLE);
                            var newStyle = new IntPtr(hideStreamed ? styleBefore.ToInt64() | WS_EX_TOOLWINDOW : styleBefore.ToInt64() & ~WS_EX_TOOLWINDOW);
                            SetWindowLongPtr(activeWindow, GWL_EXSTYLE, newStyle);
                            SetWindowPos(activeWindow, IntPtr.Zero, windowPosition.X, windowPosition.Y, (int)sizeWidth.Value, (int)sizeHeight.Value, SWP_NOACTIVE | SWP_NOZORDER);
                            continue;
                        }
                        var printed = false;
                        Bitmap bmp = recycledBitmap;
                        recycledBitmap = null;
                        if (bmp == null || !(bmp.Width == windowSize.Width && bmp.Height == windowSize.Height)) 
                        {
                            if (bmp != null)
                                bmp.Dispose();
                            bmp = new Bitmap(windowSize.Width, windowSize.Height);
                        }
                        using (Graphics memoryGraph = Graphics.FromImage(bmp))
                        {
                            if (!hideStreamed)
                            {
                                var hDC = GetDC(activeWindow);
                                var hBmp = memoryGraph.GetHdc();
                                printed = BitBlt(hBmp, 0, 0, windowSize.Width, windowSize.Height, hDC, 0, 0, 0xcc0020);
                                ReleaseDC(activeWindow, hDC);
                                memoryGraph.ReleaseHdc();
                            }
                            else
                            {
                                var ptr = memoryGraph.GetHdc();
                                printed = PrintWindow(activeWindow, ptr, 0);
                                memoryGraph.ReleaseHdc();
                            }
                        }
                        if (printed)
                        {
                            var imgWidth = windowSize.Width;
                            var imgHeight = windowSize.Height;
                            recycledBitmap = httpStreamer.SetCurrentImage(bmp, encoderParameters);
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
                                        updateSize = false;
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
                            bmp.Dispose();
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
            sendToBottomButton.Enabled = !(hideStreamed = ((CheckBox)sender).Checked);
            updateSize = true;
        }

        private void serverUrlLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(new ProcessStartInfo(((LinkLabel)sender).Text));
            e.Link.Visited = true;
        }

        private void sendToBottomButton_Click(object sender, EventArgs e)
        {
            SetWindowPos(activeWindow, new IntPtr(HWND_BOTTOM), 0, 0, 0, 0, SWP_NOACTIVE | SWP_NOMOVE | SWP_NOSIZE);
        }
    }
}
