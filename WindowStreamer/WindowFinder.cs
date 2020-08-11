using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using System.Windows.Forms;

namespace WindowStreamer
{

    public partial class WindowFinder : Form
    {


        private IntPtr activeWindow;
        private Win32.RECT? posBefore;
        private Thread workerThread;
        private Action resetFunction;
        private EncoderParameters encoderParameters;
        private bool updateSize = false;
        private bool hideStreamed = false;
        private decimal targetFps = 30;

        private MjpegStreamer httpStreamer;

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
            httpStreamer = new MjpegStreamer("SOME_BOUNDARY_WHICH_IS_NOT_PRESENT_IN_THE_FILE", 8181);
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
                Win32.SetCapture(this.Handle);
            }
        }

        public bool streamHwnd(IntPtr hwnd)
        {
            activeWindow = hwnd;
            while (true)
            {
                var hwnd2 = Win32.GetParent(activeWindow);
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
                            var styleBefore = Win32.GetWindowLongPtr(activeWindow, Win32.GWL_EXSTYLE);
                            var newStyle = new IntPtr(styleBefore.ToInt64() & ~Win32.WS_EX_TOOLWINDOW);
                            Win32.SetWindowLongPtr(activeWindow, Win32.GWL_EXSTYLE, newStyle);

                            Win32.SetWindowPos(activeWindow, IntPtr.Zero, value.Left, value.Top, value.Right - value.Left, value.Bottom - value.Top, Win32.SWP_NOACTIVE | Win32.SWP_NOREDRAW | Win32.SWP_NOZORDER);
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
                        var rect = new Win32.RECT();
                        var got_size = Win32.GetWindowRect(activeWindow, out rect);
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
                            var styleBefore = Win32.GetWindowLongPtr(activeWindow, Win32.GWL_EXSTYLE);
                            var newStyle = new IntPtr(hideStreamed ? styleBefore.ToInt64() | Win32.WS_EX_TOOLWINDOW : styleBefore.ToInt64() & ~Win32.WS_EX_TOOLWINDOW);
                            Win32.SetWindowLongPtr(activeWindow, Win32.GWL_EXSTYLE, newStyle);
                            Win32.SetWindowPos(activeWindow, IntPtr.Zero, windowPosition.X, windowPosition.Y, windowSize.Width, windowSize.Height, Win32.SWP_NOACTIVE | Win32.SWP_NOZORDER);
                        }
                        else if (updateSize)
                        {
                            updateSize = false;
                            var styleBefore = Win32.GetWindowLongPtr(activeWindow, Win32.GWL_EXSTYLE);
                            var newStyle = new IntPtr(hideStreamed ? styleBefore.ToInt64() | Win32.WS_EX_TOOLWINDOW : styleBefore.ToInt64() & ~Win32.WS_EX_TOOLWINDOW);
                            Win32.SetWindowLongPtr(activeWindow, Win32.GWL_EXSTYLE, newStyle);
                            Win32.SetWindowPos(activeWindow, IntPtr.Zero, windowPosition.X, windowPosition.Y, (int)sizeWidth.Value, (int)sizeHeight.Value, Win32.SWP_NOACTIVE | Win32.SWP_NOZORDER);
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
                                var hDC = Win32.GetDC(activeWindow);
                                var hBmp = memoryGraph.GetHdc();
                                printed = Win32.BitBlt(hBmp, 0, 0, windowSize.Width, windowSize.Height, hDC, 0, 0, 0xcc0020);
                                Win32.ReleaseDC(activeWindow, hDC);
                                memoryGraph.ReleaseHdc();
                            }
                            else
                            {
                                var ptr = memoryGraph.GetHdc();
                                printed = Win32.PrintWindow(activeWindow, ptr, 0);
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
            Win32.ReleaseCapture();

            var hwnd = Win32.WindowFromPoint(System.Windows.Forms.Control.MousePosition);
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
            Win32.SetWindowPos(activeWindow, new IntPtr(Win32.HWND_BOTTOM), 0, 0, 0, 0, Win32.SWP_NOACTIVE | Win32.SWP_NOMOVE | Win32.SWP_NOSIZE);
        }
    }
}
