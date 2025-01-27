using System.Runtime.InteropServices;
using System.Drawing;

public static class ProcessHelper
{
    // Import necessary user32.dll functions
    [DllImport("user32.dll")]
    private static extern IntPtr GetCursorPos(out Point lpPoint);

    [DllImport("user32.dll")]
    private static extern IntPtr WindowFromPoint(Point pt);

    [DllImport("user32.dll")]
    private static extern bool ScreenToClient(IntPtr hwnd, ref Point pt);

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hwnd, out uint lpdwProcessId);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool ClientToScreen(IntPtr hWnd, out Point lpPoint);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool GetClientRect(IntPtr hWnd, out Rect lpRect);

    public static Point GetCursorPosition(System.Diagnostics.Process process)
    {
        // Get the cursor position in screen coordinates
        GetCursorPos(out Point cursorPos);

        // Get the handle of the window under the cursor
        IntPtr hwnd = WindowFromPoint(cursorPos);

        // Ensure the window under the cursor matches the desired process
        if (hwnd == process.MainWindowHandle)
        {
            // Convert the screen coordinates to client coordinates of the process window
            ScreenToClient(hwnd, ref cursorPos);
            return cursorPos;
        }

        // Return a default point if the cursor isn't over the target process window
        return Point.Empty;
    }

    public static bool IsProcessActiveWindow(System.Diagnostics.Process process)
    {
        // Get the handle of the active (foreground) window
        IntPtr hwnd = GetForegroundWindow();

        // If the active window is null, return false
        if (hwnd == IntPtr.Zero)
            return false;

        // Get the process ID of the active window
        GetWindowThreadProcessId(hwnd, out uint activeProcessId);

        // Compare the process ID of the active window with the provided process
        return activeProcessId == (uint)process.Id;
    }

    public static Rectangle GetClientRectangle(IntPtr handle)
    {
        return ClientToScreen(handle, out var point) && GetClientRect(handle, out var rect)
            ? new Rectangle(point.X, point.Y, rect.Right - rect.Left, rect.Bottom - rect.Top)
            : default;
    }

    public static (int, int) GetScreenSize()
    {
        var windowHwnd = FindWindow(null!, "Counter-Strike 2");
        if (windowHwnd == IntPtr.Zero)
        {
            throw new Exception("Window not found.");
        }

        var rect = GetClientRectangle(windowHwnd);
        if (rect.Width <= 0 || rect.Height <= 0)
        {
            throw new Exception("Invalid window size.");
        }

        return (rect.Width, rect.Height);
    }
}

[StructLayout(LayoutKind.Sequential)]
public struct Rect
{
    public int Left, Top, Right, Bottom;
}