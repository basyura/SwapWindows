using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;

namespace SwapWindows
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder lpWindowText, int nMaxCount);

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
        const int SW_SHOWNORMAL = 1;
        const int SW_SHOWMINIMIZED = 2;
        const int SW_SHOWMAXIMIZED = 3;
        const int SW_SHOWNOACTIVATE = 4;
        const int SW_SHOW = 5;
        const int SW_MINIMIZE = 6;
        const int SW_SHOWMINNOACTIVE = 7;
        const int SW_SHOWNA = 8;
        const int SW_RESTORE = 9;

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        const uint SWP_NOSIZE = 0x0001;
        const uint SWP_NOZORDER = 0x0004;
        const uint SWP_NOACTIVATE = 0x0010;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        // 指定されたウィンドウの座標を取得する
        public static RECT GetWindowPosition(IntPtr hWnd)
        {
            RECT rect;
            GetWindowRect(hWnd, out rect);
            return rect;
        }


        [DllImport("user32.dll")]
        static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        public struct WINDOWPLACEMENT
        {
            public int length;
            public int flags;
            public int showCmd;
            public POINT ptMinPosition;
            public POINT ptMaxPosition;
            public RECT rcNormalPosition;
        }

        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int x;
            public int y;
        }
        /// <summary>
        /// 
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // 最大化されていて該当キーで終わるウインドウのいちを変える
            List<string> keys = new List<string>()
            {
                "- Microsoft Visual Studio",
                " | Microsoft Teams",
            };

            EnumWindows(new EnumWindowsProc((handle, pointer) =>
            {
                const int nChars = 256;
                StringBuilder buf = new StringBuilder(nChars);
                GetWindowText(handle, buf, nChars);

                string title = buf.ToString();
                foreach (string key in keys)
                {
                    if (title.EndsWith(key) && IsWindowMaximized(handle))
                    {
                        Swap(handle);
                    }
                }

                return true;

            }), IntPtr.Zero);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="handle"></param>
        private void Swap(IntPtr handle)
        {
            if (handle == IntPtr.Zero)
            {
                return;
            }

            ShowWindow(handle, SW_SHOWNORMAL);
            if (IsLeft(handle))
            {
                SetWindowPos(handle, IntPtr.Zero, 2200, 0, 0, 0, SWP_NOSIZE | SWP_NOZORDER | SWP_NOACTIVATE);
            }
            else
            {
                SetWindowPos(handle, IntPtr.Zero, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOZORDER | SWP_NOACTIVATE);
            }
            ShowWindow(handle, SW_SHOWMAXIMIZED);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Inf(object sender, RoutedEventArgs e)
        {
            LogText.Text = "";
            EnumWindows(new EnumWindowsProc((handle, pointer) =>
            {
                const int nChars = 256;
                StringBuilder buf = new StringBuilder(nChars);
                GetWindowText(handle, buf, nChars);

                string title = buf.ToString();
                if (!string.IsNullOrEmpty(title))
                {
                    RECT rect = GetWindowPosition(handle);
                    LogText.Text += handle + " : " + title + ", left: " + rect.left + Environment.NewLine;
                }
                
                return true;

            }), IntPtr.Zero);
        } 
        /// <summary>
        /// 
        /// </summary>
        /// <param name="hWnd"></param>
        /// <returns></returns>
        private bool IsLeft(IntPtr hWnd)
        {
            RECT rect = GetWindowPosition(hWnd);
            return rect.left < 1600;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="hWnd"></param>
        /// <returns></returns>
        private bool IsWindowMaximized(IntPtr hWnd)
        {
            WINDOWPLACEMENT placement = new WINDOWPLACEMENT();
            placement.length = Marshal.SizeOf(placement);
            GetWindowPlacement(hWnd, ref placement);
            return placement.showCmd == 3;
        }
    }
}
