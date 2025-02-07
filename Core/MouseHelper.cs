using System;
using System.Runtime.InteropServices;

public enum MouseKey
{
    Left,
    Right,
    Middle
}

public class MouseHelper
{
    [StructLayout(LayoutKind.Sequential)]
    struct INPUT
    {
        public int type;
        public MOUSEINPUT mi;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct MOUSEINPUT
    {
        public int dx;
        public int dy;
        public int mouseData;
        public int dwFlags;
        public int time;
        public IntPtr dwExtraInfo;
    }

    const int INPUT_MOUSE = 0;
    const int MOUSEEVENTF_MOVE = 0x0001;
    const int VK_LBUTTON = 0x01; // Left mouse button virtual-key code

    [DllImport("user32.dll", SetLastError = true)]
    static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

    [DllImport("user32.dll")]
    static extern short GetAsyncKeyState(int vKey);

    public static void MoveMouseRelative(int deltaX, int deltaY)
    {
        INPUT[] inputs =
        {
            new INPUT
            {
                type = INPUT_MOUSE,
                mi = new MOUSEINPUT
                {
                    dx = deltaX,
                    dy = deltaY,
                    dwFlags = MOUSEEVENTF_MOVE
                }
            }
        };

        SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
    }

    public static bool IsMouseDown(MouseKey key)
    {
        return GetAsyncKeyState(key switch
        {
            MouseKey.Left => VK_LBUTTON,
            MouseKey.Right => 0x02, // Right mouse button virtual-key code
            MouseKey.Middle => 0x04, // Middle mouse button virtual-key code
            _ => throw new ArgumentOutOfRangeException(nameof(key))
        }) < 0;
    }
}
