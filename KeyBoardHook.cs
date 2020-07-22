using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace WpfApp2
{
    class KeyBoardHook
    {
        private const int WH_KEYBOARD_LL = 13; // The WH_KEYBOARD_LL hook enables you to monitor keyboard input events about to be posted in a thread input queue.
        private const int WM_KEYDOWN = 0x0100; // Posted to the window with the keyboard focus when a nonsystem key is pressed. A nonsystem key is a key that is pressed when the ALT key is not pressed.
        private const int WM_SYSKEYDOWN = 0x0104; //WM_SYSKEYDOWN simulates system commands like ALT + TAB used to switch windows.

        // Use DllImport to import the Win32 MessageBox function.
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)] // Indicates that the attributed method is exposed by an unmanaged dynamic-link library (DLL) as a static entry point.
       
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)] // Use DllImport to import the Win32 MessageBox function.
        [return: MarshalAs(UnmanagedType.Bool)] // Applied to a field within a class.
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)] // Use DllImport to import the Win32 MessageBox function.
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)] // Use DllImport to import the Win32 MessageBox function.
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        public delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        public event EventHandler<KeyPressedArgs> OnKeyPressed;

        private LowLevelKeyboardProc _proc;
        private IntPtr _hookID = IntPtr.Zero;

        public KeyBoardHook()
        {
            _proc = HookCallback;
        }

        public void HookKeyboard()
        {
            _hookID = SetHook(_proc);
        }

        public void UnHookKeyboard()
        {
            UnhookWindowsHookEx(_hookID);
        }

        private IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);

                if (OnKeyPressed != null) { OnKeyPressed(this, new KeyPressedArgs(KeyInterop.KeyFromVirtualKey(vkCode))); }
            }

            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }
    }

    public class KeyPressedArgs : EventArgs
    {
        internal int keycode;

        public Key KeyPressed { get; private set; }

        public KeyPressedArgs(Key key)
        {
            KeyPressed = key;
        }
    }
}
