using System;
using System.Runtime.InteropServices;

namespace MultipleClipboards.Interop
{
    public static class Win32API
    {
        public const int CP_NOCLOSE_BUTTON = 0x200;
        public const int WM_HOTKEY = 0x312;
        public const int WM_DRAWCLIPBOARD = 0x308;
        public const int WM_CHANGECBCHAIN = 0x30D;
        public const int MOD_NOREPEAT = 0x4000;

        [DllImport("user32", SetLastError = true)]
        public static extern int RegisterHotKey(IntPtr hwnd, int id, int fsModifiers, int vk);

        [DllImport("user32", SetLastError = true)]
        public static extern int UnregisterHotKey(IntPtr hwnd, int id);

        [DllImport("user32", SetLastError = true)]
        public static extern short GetAsyncKeyState(int vKey);

		[DllImport("user32", SetLastError = true)]
    	public static extern IntPtr GetOpenClipboardWindow();

        [DllImport("user32", CharSet = CharSet.Auto)]
        public static extern IntPtr SetClipboardViewer(IntPtr hWndNewViewer);

        [DllImport("user32", CharSet = CharSet.Auto)]
        public static extern bool ChangeClipboardChain(IntPtr hWndRemove, IntPtr hWndNewNext);

        [DllImport("user32", CharSet = CharSet.Auto)]
        public static extern int SendMessage(IntPtr hwnd, int wMsg, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32", SetLastError = true)]
        public static extern short GlobalAddAtom(string lpString);

        [DllImport("kernel32", SetLastError = true)]
        public static extern short GlobalDeleteAtom(short nAtom);

		[DllImport("kernel32.dll")]
		public static extern int GetCurrentThreadId();
    }
}
