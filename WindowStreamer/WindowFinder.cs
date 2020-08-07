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

        public WindowFinder()
        {
            InitializeComponent();
            this.MouseDown += new MouseEventHandler(this.formMouseDown);
            this.MouseUp += new MouseEventHandler(this.formMouseUp);
            this.previewBox.MouseDown += new MouseEventHandler(this.formMouseDown);
            // this.MouseCaptureChanged += new Mous

        }

        protected override void OnClosed(EventArgs e)
        {
            if (posBefore.HasValue)
            {
                var value = posBefore.Value;
                MoveWindow(activeWindow, value.Left, value.Top, value.Right - value.Left, value.Bottom - value.Top, false);
            }
            base.OnClosed(e);
        }

        public void formMouseDown(Object sender, MouseEventArgs e)
        {
            WindowFinder.SetCapture(this.Handle);
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
                        workerThread.Abort();
                        if (posBefore.HasValue)
                        {
                            var value = posBefore.Value;
                            MoveWindow(activeWindow, value.Left, value.Top, value.Right - value.Left, value.Bottom - value.Top, false);
                            if (!IsDisposed)
                            {
                                previewBox.Image = null;
                                gotWindowPanel.Visible = false;
                            }
                        }
                        resetFunction = null;
                    });
                    Bitmap[] bmp = { new Bitmap(1, 1), new Bitmap(1, 1) };
                    Graphics[] memoryGraph = { Graphics.FromImage(bmp[0]), Graphics.FromImage(bmp[1]) };
                    var idx = 0;

                    Action doResetFunction = delegate
                    {
                        if (resetFunction != null)
                        {
                            _ = Invoke(resetFunction);
                        }
                    };

                    while (true)
                    {
                        var rect = new RECT();
                        var got_size = GetWindowRect(activeWindow, out rect);
                        if (!got_size)
                        {
                            doResetFunction();
                            break;
                        }
                        if (bmp[idx].Width != rect.Right - rect.Left || bmp[idx].Height != rect.Bottom - rect.Top)
                        {
                            bmp[idx] = new Bitmap(rect.Right - rect.Left, rect.Bottom - rect.Top);
                            memoryGraph[idx] = Graphics.FromImage(bmp[idx]);
                        }
                        var screen = SystemInformation.VirtualScreen;
                        sizeWidth.Maximum = screen.Width;
                        sizeHeight.Maximum = screen.Height;
                        if (posBefore == null)
                        {
                            posBefore = rect;
                            sizeWidth.Value = bmp[idx].Width;
                            sizeHeight.Value = bmp[idx].Height;
                            updateSize = true;
                        }
                        if (updateSize)
                        {
                            updateSize = false;
                            previewBox.Image = null;
                            MoveWindow(activeWindow, screen.Width, screen.Height, (int)sizeWidth.Value, (int)sizeHeight.Value, false);
                            continue;
                        }
                        var ptr = memoryGraph[idx].GetHdc();
                        var printed = PrintWindow(activeWindow, ptr, 0);
                        memoryGraph[idx].ReleaseHdc();
                        if (printed)
                        {
                            var image = bmp[idx];
                            try
                            {
                                _ = Invoke((Action)(delegate
                                {
                                    if (IsDisposed)
                                    {
                                        return;
                                    }
                                    sizeWidth.Value = image.Width;
                                    sizeHeight.Value = image.Height;
                                    gotWindowPanel.Visible = true;
                                    if (previewBox.Image == null)
                                    {
                                        idx ^= 1;
                                        previewBox.Image = image;
                                    }
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
                        Thread.Sleep(1000 / 30);
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
    }
}
