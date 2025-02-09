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
    private class MouseDownCache
    {
        public MouseKey Key { get; set; }
        public DateTime Time { get; set; }
        public bool Checked { get; set; }
    }

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

    private static List<MouseDownCache> _mouseDownCache = new();

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

    public static void UpdateMouseDowns()
    {
        if (IsMouseDown(MouseKey.Left) && !_mouseDownCache.Any(x => x.Key == MouseKey.Left))
            _mouseDownCache.Add(new MouseDownCache { Key = MouseKey.Left, Time = DateTime.Now, Checked = false });

        if (IsMouseDown(MouseKey.Right) && !_mouseDownCache.Any(x => x.Key == MouseKey.Right))
            _mouseDownCache.Add(new MouseDownCache { Key = MouseKey.Right, Time = DateTime.Now, Checked = false });

        if (IsMouseDown(MouseKey.Middle) && !_mouseDownCache.Any(x => x.Key == MouseKey.Middle))
            _mouseDownCache.Add(new MouseDownCache { Key = MouseKey.Middle, Time = DateTime.Now, Checked = false });

        // remove all that are not down
        _mouseDownCache.RemoveAll(x => x.Key == MouseKey.Left && !IsMouseDown(MouseKey.Left));
        _mouseDownCache.RemoveAll(x => x.Key == MouseKey.Right && !IsMouseDown(MouseKey.Right));
        _mouseDownCache.RemoveAll(x => x.Key == MouseKey.Middle && !IsMouseDown(MouseKey.Middle));
    }

    public static bool WasMousePressed(MouseKey key)
    {
        var cache = _mouseDownCache.FirstOrDefault(x => x.Key == key && !x.Checked && x.Time > DateTime.Now.AddMilliseconds(-100));
        if (cache == null) return false;

        cache.Checked = true;
        return true;
    }
}
