using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using JBlam.NetflixScrape.Core.Models;

namespace JBlam.NetflixScrape.Server.Platforms
{
    class Win32HostCommandProcessor : HostCommandProcessor
    {
        public static HostCommandProcessor TryCreate()
        {
            try
            {
                _ = NativeMethods.GetCursorPos(out _);
                return new Win32HostCommandProcessor();
            }
            catch (DllNotFoundException)
            {
                return null;
            }
        }

        public override void ClickMouse(MouseButton button) => Console.WriteLine(NativeMethods.SendClick(button));

        public override void MoveMouse(int x, int y)
        {
            NativeMethods.GetCursorPos(out var point);
            SetMouse(point.X + x, point.Y + y);
        }

        public override void SetMouse(int x, int y) => Console.WriteLine(NativeMethods.SetCursorPos(x, y));

        public override void WheelMouse(MouseWheelDirection direction, int amount) => Console.WriteLine(NativeMethods.SendWheelInput(direction, amount));

        static class NativeMethods
        {
            [DllImport("user32.dll")]
            public static extern bool SetCursorPos(int x, int y);

            public struct Point
            {
                public readonly int X;
                public readonly int Y;
            }
            [DllImport("user32.dll")]
            public static extern bool GetCursorPos(out Point lpPoint);

            [DllImport("user32.dll", SetLastError = true)]
            static extern uint SendInput(uint nInputs, ref INPUT pInputs, int cbSize);

            [StructLayout(LayoutKind.Sequential)]
            struct INPUT
            {
                public SendInputEventType type;
                public MouseKeybdhardwareInputUnion mkhi;
            }
            [StructLayout(LayoutKind.Explicit)]
            struct MouseKeybdhardwareInputUnion
            {
                [FieldOffset(0)]
                public MouseInputData mi;

                [FieldOffset(0)]
                public KEYBDINPUT ki;

                [FieldOffset(0)]
                public HARDWAREINPUT hi;
            }
            [StructLayout(LayoutKind.Sequential)]
            struct KEYBDINPUT
            {
                public ushort wVk;
                public ushort wScan;
                public uint dwFlags;
                public uint time;
                public IntPtr dwExtraInfo;
            }
            [StructLayout(LayoutKind.Sequential)]
            struct HARDWAREINPUT
            {
                public int uMsg;
                public short wParamL;
                public short wParamH;
            }
            struct MouseInputData
            {
                public int dx;
                public int dy;
                public int mouseData;
                public MouseEventFlags dwFlags;
                public uint time;
                public IntPtr dwExtraInfo;
            }
            [Flags]
            enum MouseEventFlags : uint
            {
                MOUSEEVENTF_MOVE = 0x0001,
                MOUSEEVENTF_LEFTDOWN = 0x0002,
                MOUSEEVENTF_LEFTUP = 0x0004,
                MOUSEEVENTF_RIGHTDOWN = 0x0008,
                MOUSEEVENTF_RIGHTUP = 0x0010,
                MOUSEEVENTF_MIDDLEDOWN = 0x0020,
                MOUSEEVENTF_MIDDLEUP = 0x0040,
                MOUSEEVENTF_XDOWN = 0x0080,
                MOUSEEVENTF_XUP = 0x0100,
                MOUSEEVENTF_WHEEL = 0x0800,
                MOUSEEVENTF_VIRTUALDESK = 0x4000,
                MOUSEEVENTF_ABSOLUTE = 0x8000
            }
            enum SendInputEventType : int
            {
                InputMouse,
                InputKeyboard,
                InputHardware
            }

            public static void ClickLeftMouseButton()
            {
                INPUT mouseDownInput = new INPUT();
                mouseDownInput.type = SendInputEventType.InputMouse;
                mouseDownInput.mkhi.mi.dwFlags = MouseEventFlags.MOUSEEVENTF_LEFTDOWN;
                SendInput(1, ref mouseDownInput, Marshal.SizeOf(new INPUT()));

                INPUT mouseUpInput = new INPUT();
                mouseUpInput.type = SendInputEventType.InputMouse;
                mouseUpInput.mkhi.mi.dwFlags = MouseEventFlags.MOUSEEVENTF_LEFTUP;
                SendInput(1, ref mouseUpInput, Marshal.SizeOf(new INPUT()));
            }
            public static void ClickRightMouseButton()
            {
                INPUT mouseDownInput = new INPUT();
                mouseDownInput.type = SendInputEventType.InputMouse;
                mouseDownInput.mkhi.mi.dwFlags = MouseEventFlags.MOUSEEVENTF_RIGHTDOWN;
                SendInput(1, ref mouseDownInput, Marshal.SizeOf(new INPUT()));

                INPUT mouseUpInput = new INPUT();
                mouseUpInput.type = SendInputEventType.InputMouse;
                mouseUpInput.mkhi.mi.dwFlags = MouseEventFlags.MOUSEEVENTF_RIGHTUP;
                SendInput(1, ref mouseUpInput, Marshal.SizeOf(new INPUT()));
            }

            public static void MouseWheel(MouseWheelDirection direction, int amount)
            {
                INPUT wheelInput = new INPUT()
                {
                    type = SendInputEventType.InputMouse
                };
                wheelInput.mkhi.mi.dwFlags = MouseEventFlags.MOUSEEVENTF_WHEEL;
                wheelInput.mkhi.mi.mouseData = (direction == MouseWheelDirection.WheelUp ? amount : -amount) * 120;
                SendInput(1, ref wheelInput, Marshal.SizeOf<INPUT>());
            }

            [DllImport("user32.dll")]
            static extern uint SendInput(uint nInputs, [MarshalAs(UnmanagedType.LPArray), In] ref INPUT[] pInputs, int cbSize);
        }
    }
}
