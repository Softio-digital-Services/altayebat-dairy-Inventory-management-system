using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace InventorySystem.Helpers
{
    /// <summary>
    /// Keeps borderless maximized windows inside the working area so the Windows taskbar stays visible.
    /// </summary>
    public static class BorderlessFormHelper
    {
        private const int WM_GETMINMAXINFO = 0x0024;

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MINMAXINFO
        {
            public POINT ptReserved;
            public POINT ptMaxSize;
            public POINT ptMaxPosition;
            public POINT ptMinTrackSize;
            public POINT ptMaxTrackSize;
        }

        /// <summary>
        /// Call from a form's WndProc before base.WndProc. Adjusts maximize bounds to WorkingArea.
        /// </summary>
        public static void HandleGetMinMaxInfo(Form form, ref Message m)
        {
            if (m.Msg != WM_GETMINMAXINFO || form == null || form.IsDisposed)
                return;

            Screen screen = form.IsHandleCreated
                ? Screen.FromHandle(form.Handle)
                : Screen.PrimaryScreen;

            Rectangle working = screen.WorkingArea;
            Rectangle monitor = screen.Bounds;

            MINMAXINFO mmi = Marshal.PtrToStructure<MINMAXINFO>(m.LParam);
            mmi.ptMaxPosition.X = Math.Abs(working.Left - monitor.Left);
            mmi.ptMaxPosition.Y = Math.Abs(working.Top - monitor.Top);
            mmi.ptMaxSize.X = working.Width;
            mmi.ptMaxSize.Y = working.Height;
            mmi.ptMaxTrackSize.X = working.Width;
            mmi.ptMaxTrackSize.Y = working.Height;
            Marshal.StructureToPtr(mmi, m.LParam, true);
        }

        /// <summary>
        /// Fit a borderless form to the current screen working area (taskbar-safe maximize).
        /// </summary>
        public static void FitToWorkingArea(Form form)
        {
            if (form == null || form.IsDisposed) return;
            Screen screen = form.IsHandleCreated
                ? Screen.FromHandle(form.Handle)
                : Screen.PrimaryScreen;
            form.WindowState = FormWindowState.Normal;
            form.Bounds = screen.WorkingArea;
        }
    }
}
