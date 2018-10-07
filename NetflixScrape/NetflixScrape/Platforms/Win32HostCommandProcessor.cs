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

        public override void ClickMouse(MouseButton button) => NativeMethods.Click(button);

        public override void MoveMouse(int x, int y)
        {
            NativeMethods.GetCursorPos(out var point);
            SetMouse(point.X + x, point.Y + y);
        }

        public override void SetMouse(int x, int y) => Console.WriteLine(NativeMethods.SetCursorPos(x, y));

        public override void WheelMouse(MouseWheelDirection direction, int amount) => NativeMethods.MouseWheel(direction, amount);

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

            #region SendInput supporting types
            [StructLayout(LayoutKind.Sequential)]
            struct INPUT
            {

                enum SendInputEventType : int
                {
                    InputMouse,
                    InputKeyboard,
                    InputHardware
                }

                readonly SendInputEventType type;
                readonly InputDataUnion data;
                [StructLayout(LayoutKind.Explicit)]
                struct InputDataUnion
                {
                    [FieldOffset(0)]
                    readonly MouseInputData mi;

                    [FieldOffset(0)]
                    readonly KEYBDINPUT ki;

                    [FieldOffset(0)]
                    readonly HARDWAREINPUT hi;

                    public InputDataUnion(MouseInputData mouseData)
                    {
                        ki = default(KEYBDINPUT);
                        hi = default(HARDWAREINPUT);
                        mi = mouseData;
                    }
                }
                public INPUT(MouseInputData mouseData)
                {
                    data = new InputDataUnion(mouseData);
                    type = SendInputEventType.InputMouse;
                }
            }
            [StructLayout(LayoutKind.Sequential)]
            struct KEYBDINPUT
            {
                readonly ushort wVk;
                readonly ushort wScan;
                readonly uint dwFlags;
                readonly uint time;
                readonly IntPtr dwExtraInfo;
            }
            [StructLayout(LayoutKind.Sequential)]
            struct HARDWAREINPUT
            {
                readonly int uMsg;
                readonly short wParamL;
                readonly short wParamH;
            }
            struct MouseInputData
            {
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

                int dx;
                int dy;
                int mouseData;
                MouseEventFlags dwFlags;
                uint time;
                IntPtr dwExtraInfo;

                public static IEnumerable<MouseInputData> GetClickData(MouseButton button)
                {
                    IEnumerable<MouseEventFlags> GetClickFlags(MouseButton b)
                    {
                        switch (b)
                        {
                            case MouseButton.Left:
                                yield return MouseEventFlags.MOUSEEVENTF_LEFTDOWN;
                                yield return MouseEventFlags.MOUSEEVENTF_LEFTUP;
                                yield break;
                            case MouseButton.Right:
                                yield return MouseEventFlags.MOUSEEVENTF_RIGHTDOWN;
                                yield return MouseEventFlags.MOUSEEVENTF_RIGHTUP;
                                yield break;
                            default:
                                break;
                        }
                    }
                    return GetClickFlags(button).Select(f => new MouseInputData { dwFlags = f });
                }

                public static MouseInputData GetWheelData(MouseWheelDirection direction, int amount)
                {
                    return new MouseInputData
                    {
                        dwFlags = MouseEventFlags.MOUSEEVENTF_WHEEL,
                        mouseData = (direction == MouseWheelDirection.WheelUp ? amount : -amount) * 120
                    };
                }
            }
            #endregion

            public static void Click(MouseButton button)
            {
                foreach (var mouse in MouseInputData.GetClickData(button))
                {
                    var input = new INPUT(mouse);
                    SendInput(1, ref input, Marshal.SizeOf<INPUT>());
                }
            }

            public static void MouseWheel(MouseWheelDirection direction, int amount)
            {
                var wheelInput = new INPUT(MouseInputData.GetWheelData(direction, amount));
                SendInput(1, ref wheelInput, Marshal.SizeOf<INPUT>());
            }
        }
    }
}
