using CS2.Hacks;
using GameOverlay.Drawing;

namespace CS2.Core;

public enum HoldingWindowType
{
    None,
    Move,
    ResizeWidth,
    ResizeHeight,
}

public class Windows
{
    public static void DrawWatermark(Overlay overlay, Graphics gfx, System.Drawing.Point cursorPos)
	{		
		var infoText = $"{Globals.ProjectName} | FPS: {gfx.FPS} | Cursor: {cursorPos.X}, {cursorPos.Y}";
		gfx.DrawTextWithBackground(overlay.fonts["consolas"], overlay.colors["green"], overlay.colors["black"], 10, 10, infoText);
	}

    public static async Task UseBaseHack(Overlay overlay, Graphics gfx, System.Drawing.Point cursorPos)
    {
        await BaseHack.Loop();
    }

    public static async Task UseAimbot(Overlay overlay, Graphics gfx, System.Drawing.Point cursorPos)
    {
        await Aimbot.Loop(overlay, gfx);
    }

    public static async Task UseTriggerBot(Overlay overlay, Graphics gfx, System.Drawing.Point cursorPos)
    {
        await TriggerBot.Loop();
    }

    public static async Task DrawEsp(Overlay overlay, Graphics gfx, System.Drawing.Point cursorPos)
    {
        await Esp.Loop(overlay, gfx);
    }

    public static bool IsMouseInPosition(int x, int y, int width, int height)
    {
        var cursorPos = Overlay.GetCursorPosition();
        return cursorPos.X >= x && cursorPos.X <= x + width && cursorPos.Y >= y && cursorPos.Y <= y + height;
    }

    public static void DrawRectangle(Graphics gfx, int x, int y, int width, int height, IBrush brush)
    {
        gfx.FillRectangle(brush, x, y, x + width, y + height);
    }
    
    public static void DrawStrokeRectangle(Graphics gfx, int x, int y, int width, int height, IBrush brush, int stroke)
    {
        gfx.DrawRectangle(brush, x, y, x + width, y + height, stroke);
    }

    public static void DrawWindow(string text, int x, int y, int width, int height, Overlay overlay, Graphics gfx)
    {
        DrawRectangle(gfx, x, y, width, height, overlay.colors["background"]);
        DrawStrokeRectangle(gfx, x, y, width, height, overlay.colors["background-border"], 2);
        DrawRectangle(gfx, x, y, width, (int)MathF.Min(20, height), overlay.colors["background-border"]);
        
        var size = gfx.MeasureString(overlay.fonts["consolas"], text);
        gfx.DrawText(overlay.fonts["consolas"], overlay.colors["white"], x + width / 2 - size.X / 2, y + 10 - size.Y / 2, text);
    }

    private static string? _holdingWindowKey;
    private static HoldingWindowType _holdingWindowType = HoldingWindowType.None;
    private static int _holdingWindowX;
    private static int _holdingWindowY;
    private static DateTime _lastGuiInteraction = DateTime.Now;

    public static (int, int) MoveWindow(string key, int x, int y, int width, int height)
    {
        if (_holdingWindowKey == key && _holdingWindowType == HoldingWindowType.Move)
        {
            var cursorPos = Overlay.GetCursorPosition();
            x = cursorPos.X - _holdingWindowX;
            y = cursorPos.Y - _holdingWindowY;

            x = Math.Clamp(x, 0, (Overlay.Width ?? 0) - width);
            y = Math.Clamp(y, 0, (Overlay.Height ?? 0) - height);
            
            if (!MouseHelper.IsMouseDown(MouseKey.Left))
            {
                _holdingWindowKey = null;
                _holdingWindowType = HoldingWindowType.None;
            }
        }

        if (MouseHelper.IsMouseDown(MouseKey.Left) && IsMouseInPosition(x, y, width, 20))
        {
            _holdingWindowKey = key;
            var cursorPos = Overlay.GetCursorPosition();
            _holdingWindowX = cursorPos.X - x;
            _holdingWindowY = cursorPos.Y - y;
            _holdingWindowType = HoldingWindowType.Move;
        }

        return (x, y);
    }

    public static (int, int) ResizeWindow(string key, int x, int y, int width, int height, Overlay overlay, Graphics gfx)
    {
        var cursorPos = Overlay.GetCursorPosition();

        // if is on the right:
        if (IsMouseInPosition(x + width - 3, y, 6, height))
        {
            DrawRectangle(gfx, x + width - 3, y, 6, height, overlay.colors["red"]);
            if (MouseHelper.IsMouseDown(MouseKey.Left))
            {
                _holdingWindowKey = key;
                _holdingWindowType = HoldingWindowType.ResizeWidth;
            }
        }

        if (_holdingWindowKey == key && _holdingWindowType == HoldingWindowType.ResizeWidth)
        {
            width = cursorPos.X - x;
            
            if (!MouseHelper.IsMouseDown(MouseKey.Left))
            {
                _holdingWindowKey = null;
                _holdingWindowType = HoldingWindowType.None;
            }
        }

        // if is on the bottom:
        if (IsMouseInPosition(x, y + height - 3, width, 6))
        {
            DrawRectangle(gfx, x, y + height - 3, width, 6, overlay.colors["red"]);
            if (MouseHelper.IsMouseDown(MouseKey.Left))
            {
                _holdingWindowKey = key;
                _holdingWindowType = HoldingWindowType.ResizeHeight;
            }
        }

        if (_holdingWindowKey == key && _holdingWindowType == HoldingWindowType.ResizeHeight)
        {
            height = cursorPos.Y - y;
            
            if (!MouseHelper.IsMouseDown(MouseKey.Left))
            {
                _holdingWindowKey = null;
                _holdingWindowType = HoldingWindowType.None;
            }
        }

        return (width, height);
    }

    public static void DrawCheckbox(string text, int x, int y, ref bool isChecked, Overlay overlay, Graphics gfx)
    {
        DrawRectangle(gfx, x, y, 20, 20, overlay.colors["background"]);
        DrawStrokeRectangle(gfx, x, y, 20, 20, overlay.colors["background-border"], 2);
        DrawRectangle(gfx, x + 2, y + 2, 16, 16, isChecked ? overlay.colors["green"] : overlay.colors["background"]);
        
        var size = gfx.MeasureString(overlay.fonts["consolas"], text);
        gfx.DrawText(overlay.fonts["consolas"], overlay.colors["white"], x + 30, y + 10 - size.Y / 2, text);

        if (IsMouseInPosition(x, y, 20, 20) && MouseHelper.IsMouseDown(MouseKey.Left) && (DateTime.Now - _lastGuiInteraction).TotalMilliseconds > 100 && _holdingWindowKey == null)
        {
            isChecked = !isChecked;
            _lastGuiInteraction = DateTime.Now;
        }
    }

    public static void DrawSlider(string text, int x, int y, int width, int height, ref float value, float min, float max, Overlay overlay, Graphics gfx)
    {
        var size = gfx.MeasureString(overlay.fonts["consolas"], text);
        var w = width - (int)size.X - 10;

        DrawRectangle(gfx, x, y, w, height, overlay.colors["background"]);
        DrawStrokeRectangle(gfx, x, y, w, height, overlay.colors["background-border"], 2);
        DrawRectangle(gfx, x + 2, y + 2, (int)((w - 4) * (value - min) / (max - min)), height - 4, overlay.colors["green"]);
        
        gfx.DrawText(overlay.fonts["consolas"], overlay.colors["white"], x + w + 10, y + height / 2 - size.Y / 2, text);

        gfx.DrawText(overlay.fonts["consolas"], overlay.colors["black"], x + 3, y + height / 2 - size.Y / 2 + 1, $"{value}");
        gfx.DrawText(overlay.fonts["consolas"], overlay.colors["white"], x + 2, y + height / 2 - size.Y / 2, $"{value}");

        if (IsMouseInPosition(x, y, w, height) && MouseHelper.IsMouseDown(MouseKey.Left) && (DateTime.Now - _lastGuiInteraction).TotalMilliseconds > 100 && _holdingWindowKey == null)
        {
            value = Math.Clamp((Overlay.GetCursorPosition().X - x) * (max - min) / w + min, min, max);
            _lastGuiInteraction = DateTime.Now;
        }
    }

    public static void DrawSlider(string text, int x, int y, int width, int height, ref int value, int min, int max, Overlay overlay, Graphics gfx)
    {
        float tempRef = value;
        DrawSlider(text, x, y, width, height, ref tempRef, min, max, overlay, gfx);
        value = (int)tempRef;
    }

    public static void DrawSelect(string text, int x, int y, int width, int height, ref string value, List<string> values, ref bool open, ref int openScroll, int maxOptionsVisibleAtOnce, Overlay overlay, Graphics gfx)
    {
        var sizeHeight = gfx.MeasureString(overlay.fonts["consolas"], text).Y;
        gfx.DrawText(overlay.fonts["consolas"], overlay.colors["white"], x, y, text);

        y += (int)sizeHeight + 3;
        DrawRectangle(gfx, x, y, width, height, overlay.colors["background"]);
        DrawStrokeRectangle(gfx, x, y, width, height, overlay.colors["background-border"], 2);
        
        var size = gfx.MeasureString(overlay.fonts["consolas"], value);
        gfx.DrawText(overlay.fonts["consolas"], overlay.colors["white"], x + 10, y + height / 2 - size.Y / 2, value);

        if (IsMouseInPosition(x, y, width, height) && MouseHelper.IsMouseDown(MouseKey.Left) && (DateTime.Now - _lastGuiInteraction).TotalMilliseconds > 100 && _holdingWindowKey == null)
        {
            open = !open;
            _lastGuiInteraction = DateTime.Now;
        }

        if (open)
        {
            DrawRectangle(gfx, x, y + height, width, height * Math.Min(values.Count, maxOptionsVisibleAtOnce), overlay.colors["background"]);
            DrawStrokeRectangle(gfx, x, y + height, width, height * Math.Min(values.Count, maxOptionsVisibleAtOnce), overlay.colors["background-border"], 2);
            
            for (var i = openScroll; i < Math.Min(values.Count, openScroll + maxOptionsVisibleAtOnce); i++)
            {
                var color = IsMouseInPosition(x, y + height * (i - openScroll + 1), width, height) ? overlay.colors["red"] : overlay.colors["background"];
                DrawRectangle(gfx, x, y + height * (i - openScroll + 1), width, height, color);
                DrawStrokeRectangle(gfx, x, y + height * (i - openScroll + 1), width, height, overlay.colors["background-border"], 2);
                
                var size2 = gfx.MeasureString(overlay.fonts["consolas"], values[i]);
                gfx.DrawText(overlay.fonts["consolas"], overlay.colors["white"], x + 10, y + height * (i - openScroll + 1) + height / 2 - size2.Y / 2, values[i]);

                if (IsMouseInPosition(x, y + height * (i - openScroll + 1), width, height) && MouseHelper.IsMouseDown(MouseKey.Left) && (DateTime.Now - _lastGuiInteraction).TotalMilliseconds > 100)
                {
                    value = values[i];
                    open = false;
                    _lastGuiInteraction = DateTime.Now;
                }
            }

            var scrollPos = (ProcessHelper.GetAsyncKeyState(0x26) == 0 ? 0 : -1) + (ProcessHelper.GetAsyncKeyState(0x28) == 0 ? 0 : 1);
            if (scrollPos != 0 && (DateTime.Now - _lastGuiInteraction).TotalMilliseconds > 50)
            {
                openScroll = Math.Clamp(openScroll + scrollPos, 0, values.Count - maxOptionsVisibleAtOnce);
                _lastGuiInteraction = DateTime.Now;
            }
            
            // DRAW TEXT "use arrow keys to scroll"
            var scrollText = "Use arrow keys to scroll";
            var scrollTextSize = gfx.MeasureString(overlay.fonts["consolas"], scrollText);
            gfx.DrawText(overlay.fonts["consolas"], overlay.colors["white"], x + width / 2 - scrollTextSize.X / 2, y + height * (maxOptionsVisibleAtOnce + 1) + height / 2 - scrollTextSize.Y / 2, scrollText);
        }
    }

}