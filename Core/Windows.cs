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
		var infoText = $"{Globals.ProjectName} | Overlay FPS: {gfx.FPS}";
		gfx.DrawTextWithBackground(overlay.fonts["consolas"], overlay.colors["active"], overlay.colors["background-dark"], 10, 10, infoText);
	}

    public static async Task UseBaseHack(Overlay overlay, Graphics gfx, System.Drawing.Point cursorPos)
    {
        await BaseHack.Loop();
    }

    public static async Task UseAimbot(Overlay overlay, Graphics gfx, System.Drawing.Point cursorPos)
    {
        await Aimbot.Loop(overlay, gfx);
    }

    public static async Task UseRecoilControl(Overlay overlay, Graphics gfx, System.Drawing.Point cursorPos)
    {
        await RecoilControl.Loop(overlay, gfx);
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

    public static void DrawRoundedRectangle(Graphics gfx, int x, int y, int width, int height, IBrush brush, int radius)
    {
        gfx.FillRoundedRectangle(brush, x, y, x + width, y + height, radius);
    }
    
    public static void DrawStrokeRectangle(Graphics gfx, int x, int y, int width, int height, IBrush brush, int stroke)
    {
        gfx.DrawRectangle(brush, x, y, x + width, y + height, stroke);
    }

    public static void DrawRoundedStrokeRectangle(Graphics gfx, int x, int y, int width, int height, IBrush brush, int stroke, int radius)
    {
        gfx.DrawRoundedRectangle(brush, x, y, x + width, y + height, radius, stroke);
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

    public static void DrawCheckbox(string text, int x, int y, ref bool active, Overlay overlay, Graphics gfx)
    {
        var size = gfx.MeasureString(overlay.fonts["arial"], 14, text);
        var mouseOver = IsMouseInPosition(x, y, 27 + (int)size.X, 20);

        var color = active ? (mouseOver ? overlay.colors["active-dark"] : overlay.colors["active"]) : (mouseOver ? overlay.colors["background-light3"] : overlay.colors["background-light2"]);
        var textColor = (mouseOver || active) ? overlay.colors["white2"] : overlay.colors["white3"];

        DrawRoundedRectangle(gfx, x, y, 20, 20, color, 5);

        // checkmark with lines
        if (active)
        {
            gfx.DrawLine(overlay.colors["background-dark"], x + 5, y + 10, x + 8, y + 13, 2);
            gfx.DrawLine(overlay.colors["background-dark"], x + 8, y + 13, x + 15, y + 6, 2);
        }

        gfx.DrawText(overlay.fonts["arial"], 14, textColor, x + 27, y + 10 - size.Y / 2, text);

        if (mouseOver && MouseHelper.WasMousePressed(MouseKey.Left))
        {
            active = !active;
        }
    }

    public static void DrawSlider(string text, int x, int y, int width, ref float value, float min, float max, Overlay overlay, Graphics gfx)
    {
        var size = gfx.MeasureString(overlay.fonts["arial"], 14, text);
        var valueSize = gfx.MeasureString(overlay.fonts["arial"], 14, $"{value:0.00}");
        var progress = (value - min) / (max - min);
        var mouseOver = IsMouseInPosition(x, y + (int)size.Y, width, 17);
        var color = mouseOver ? overlay.colors["active"] : overlay.colors["active-dark"];
        var textColor = mouseOver ? overlay.colors["white2"] : overlay.colors["white3"];

        gfx.DrawText(overlay.fonts["arial"], 14, textColor, x, y, text);
        gfx.DrawText(overlay.fonts["arial"], 14, textColor, x + width - valueSize.X, y, $"{value:0.00}");

        DrawRoundedRectangle(gfx, x, (int)(y + size.Y + 7), width, 8, overlay.colors["background-light2"], 4);
        DrawRoundedRectangle(gfx, x, (int)(y + size.Y + 7), (int)(width * progress), 8, color, 4);

        gfx.FillCircle(overlay.colors["background-light"], x + width * progress, y + size.Y + 11, 9);
        gfx.DrawCircle(color, x + width * progress, y + size.Y + 11, 6, 2);

        if (mouseOver && MouseHelper.IsMouseDown(MouseKey.Left))
        {
            var cursorPos = Overlay.GetCursorPosition();
            value = Math.Clamp((cursorPos.X - x) * (max - min) / width + min, min, max);
        }
    }

    public static void DrawSlider(string text, int x, int y, int width, ref int value, int min, int max, Overlay overlay, Graphics gfx)
    {
        float tempRef = value;
        DrawSlider(text, x, y, width, ref tempRef, min, max, overlay, gfx);
        value = (int)tempRef;
    }

    public static void DrawSelect(string text, int x, int y, int width, ref string value, List<string> values, ref bool open, ref int openScroll, int maxOptionsVisibleAtOnce, Overlay overlay, Graphics gfx)
    {
        var height = 26;
        var sizeHeight = gfx.MeasureString(overlay.fonts["arial"], 14, text).Y;

        gfx.DrawText(overlay.fonts["arial"], 14, overlay.colors["white2"], x, y, text);

        y += (int)sizeHeight + 5;
        var mouseOver = IsMouseInPosition(x, y + 5, width, height);
        var textColor = mouseOver ? overlay.colors["white2"] : overlay.colors["white3"];
        
        DrawRoundedRectangle(gfx, x, y, width, height, overlay.colors["background-light2"], 4);
        DrawRoundedStrokeRectangle(gfx, x, y, width, height, overlay.colors["background-light3"], 1, 4);
        
        var size = gfx.MeasureString(overlay.fonts["arial"], 14, value);
        gfx.DrawText(overlay.fonts["arial"], 14, textColor, x + 10, y + height / 2 - size.Y / 2, value);

        if (open)
        {
            DrawRoundedRectangle(gfx, x, y + height, width, height * Math.Min(values.Count, maxOptionsVisibleAtOnce), overlay.colors["background-light2"], 4);
            DrawRoundedStrokeRectangle(gfx, x, y + height, width, height * Math.Min(values.Count, maxOptionsVisibleAtOnce), overlay.colors["background-light3"], 1, 4);
            
            for (var i = openScroll; i < Math.Min(values.Count, openScroll + maxOptionsVisibleAtOnce); i++)
            {
                var overOption = IsMouseInPosition(x, y + height * (i - openScroll + 1), width, height);

                if (overOption)
                    DrawRoundedRectangle(gfx, x, y + height * (i - openScroll + 1), width, height, overlay.colors["background-light3"], 4);
                
                var size2 = gfx.MeasureString(overlay.fonts["arial"], 14, values[i]);
                gfx.DrawText(overlay.fonts["arial"], 14, overlay.colors["white2"], x + 10, y + height * (i - openScroll + 1) + height / 2 - size2.Y / 2, values[i]);
            }

            var scrollPos = (ProcessHelper.GetAsyncKeyState(0x26) == 0 ? 0 : -1) + (ProcessHelper.GetAsyncKeyState(0x28) == 0 ? 0 : 1);
            if (scrollPos != 0 && (DateTime.Now - _lastGuiInteraction).TotalMilliseconds > 50)
            {
                openScroll = Math.Clamp(openScroll + scrollPos, 0, Math.Max(0, values.Count - maxOptionsVisibleAtOnce));
                _lastGuiInteraction = DateTime.Now;
            }
            
            // DRAW TEXT "use arrow keys to scroll"
            // var scrollText = "Use arrow keys to scroll (⮝/⮟)";
            // var scrollTextSize = gfx.MeasureString(overlay.fonts["arial"], 14, scrollText);
            // gfx.DrawText(overlay.fonts["arial"], 14, overlay.colors["white2"], x + width / 2 - scrollTextSize.X / 2, y + height * (Math.Min(values.Count, maxOptionsVisibleAtOnce) + 1) + 5, scrollText);
        
            DrawScroll(text, x + width + 2, y + height, 5, height * Math.Min(values.Count, maxOptionsVisibleAtOnce), ref openScroll, maxOptionsVisibleAtOnce, values.Count, overlay, gfx);
        }
    }

    public static void InteractSelect(string text, int x, int y, int width, ref string value, List<string> values, ref bool open, ref int openScroll, int maxOptionsVisibleAtOnce, Overlay overlay, Graphics gfx)
    {
        var height = 26;
        var sizeHeight = gfx.MeasureString(overlay.fonts["arial"], 14, text).Y;

        y += (int)sizeHeight + 5;
        
        if (IsMouseInPosition(x, y, width, height) && MouseHelper.WasMousePressed(MouseKey.Left))
        {
            open = !open;
        }

        if (open)
        {
            for (var i = openScroll; i < Math.Min(values.Count, openScroll + maxOptionsVisibleAtOnce); i++)
            {
                var overOption = IsMouseInPosition(x, y + height * (i - openScroll + 1), width, height);

                if (overOption && MouseHelper.WasMousePressed(MouseKey.Left))
                {
                    value = values[i];
                    open = false;
                }
            }
        }
    }

    public static void DrawButton(string text, int x, int y, int width, int height, Overlay overlay, Graphics gfx)
    {
        var mouseOver = IsMouseInPosition(x, y, width, height);
        var color = mouseOver ? overlay.colors["background-light3"] : overlay.colors["background-light2"];
        var textColor = mouseOver ? overlay.colors["white2"] : overlay.colors["white3"];

        DrawRoundedRectangle(gfx, x, y, width, height, color, 4);
        DrawRoundedStrokeRectangle(gfx, x, y, width, height, overlay.colors["background-light3"], 1, 4);

        var size = gfx.MeasureString(overlay.fonts["arial"], 14, text);
        gfx.DrawText(overlay.fonts["arial"], 14, textColor, x + width / 2 - size.X / 2, y + height / 2 - size.Y / 2, text);
    }

    public static void DrawKeySelect(string text, int x, int y, int width, ref int key, ref bool selectingKey, Overlay overlay, Graphics gfx)
    {
        var height = 26;
        var sizeHeight = gfx.MeasureString(overlay.fonts["arial"], 14, text).Y;

        gfx.DrawText(overlay.fonts["arial"], 14, overlay.colors["white2"], x, y, text);

        y += (int)sizeHeight + 5;
        var mouseOver = IsMouseInPosition(x, y + 5, width, height);
        var textColor = mouseOver ? overlay.colors["white2"] : overlay.colors["white3"];
        
        DrawRoundedRectangle(gfx, x, y, width, height, overlay.colors["background-light2"], 4);
        DrawRoundedStrokeRectangle(gfx, x, y, width, height, overlay.colors["background-light3"], 1, 4);
        
        var valueText = selectingKey ? "Press a key or ESC to cancel" : ProcessHelper.GetKeyName(key);
        var size = gfx.MeasureString(overlay.fonts["arial"], 14, valueText);
        gfx.DrawText(overlay.fonts["arial"], 14, textColor, x + 10, y + height / 2 - size.Y / 2, valueText);

        if (!selectingKey && mouseOver && MouseHelper.WasMousePressed(MouseKey.Left))
        {
            selectingKey = true;
            _lastGuiInteraction = DateTime.Now;
        }
        else if(selectingKey && _lastGuiInteraction.AddMilliseconds(100) < DateTime.Now)
        {
            if (ProcessHelper.IsKeyDown(0x1B))
            {
                selectingKey = false;
            }
            else
            {
                for (var i = 0; i < 256; i++)
                {
                    if (ProcessHelper.IsKeyDown(i))
                    {
                        key = i;
                        selectingKey = false;
                        break;
                    }
                }
            }
        }
    }

    public static void DrawList(string text, int x, int y, int width, List<string> values, ref string selectedValue, int maxOptionsVisibleAtOnce, ref int scroll, Overlay overlay, Graphics gfx)
    {
        if (values.Count == 0)
        {
            values.Add("Nothing to show.");
        }

        var height = 26;
        var sizeHeight = gfx.MeasureString(overlay.fonts["arial"], 14, text).Y;
        var totalBoxHeight = height * maxOptionsVisibleAtOnce;

        gfx.DrawText(overlay.fonts["arial"], 14, overlay.colors["white2"], x, y, text);

        y += (int)sizeHeight + 5;
        DrawRoundedRectangle(gfx, x, y, width - 7, totalBoxHeight, overlay.colors["background-light2"], 4);

        for (var i = scroll; i < Math.Min(values.Count, scroll + maxOptionsVisibleAtOnce); i++)
        {
            var overOption = IsMouseInPosition(x, y + height * (i - scroll), width - 7, height);

            if (overOption)
                DrawRoundedRectangle(gfx, x, y + height * (i - scroll), width - 7, height, overlay.colors["background-light3"], 4);

            if (selectedValue == values[i])
                DrawRoundedRectangle(gfx, x, y + height * (i - scroll), width - 7, height, overlay.colors["background-light4"], 4); 
            
            var size2 = gfx.MeasureString(overlay.fonts["arial"], 14, values[i]);
            gfx.DrawText(overlay.fonts["arial"], 14, overlay.colors["white2"], x + 10, y + height * (i - scroll) + height / 2 - size2.Y / 2, values[i]);

            if (overOption && MouseHelper.WasMousePressed(MouseKey.Left) && values[i] != "Nothing to show.")
            {
                selectedValue = values[i];
            }
        }

        DrawScroll(text, x + width - 5, y, 5, totalBoxHeight, ref scroll, maxOptionsVisibleAtOnce, values.Count, overlay, gfx);
    }

    private static string? _holdingScrollKey;
    private static int _holdingScrollPos;
    
    public static void DrawScroll(string key, int x, int y, int w, int h, ref int progress, int maxView, int max, Overlay overlay, Graphics gfx)
    {
        max -= maxView;
        max = Math.Max(max, 1);
        maxView = Math.Max(maxView, 1);

        var scrollHeight = (int)((float)maxView / max * h);
        scrollHeight = Math.Clamp(scrollHeight, 10, h);

        var scrollY = (int)((float)progress / max * (h - scrollHeight));
        scrollY = Math.Clamp(scrollY, 0, h - scrollHeight);

        var mouseOver = IsMouseInPosition(x, y + scrollY, w, scrollHeight);

        DrawRoundedRectangle(gfx, x, y, w, h, overlay.colors["background-light2"], 2);
        DrawRoundedRectangle(gfx, x, y + scrollY, w, scrollHeight, mouseOver ? overlay.colors["active"] : overlay.colors["active-dark"], 2);

        var cursorPos = Overlay.GetCursorPosition();

        if (_holdingScrollKey == null && mouseOver && MouseHelper.WasMousePressed(MouseKey.Left))
        {
            _holdingScrollKey = key;
            _holdingScrollPos = cursorPos.Y - y - scrollY;
        }

        if (_holdingScrollKey == key)
        {
            progress = (int)((cursorPos.Y - y - _holdingScrollPos) / (float)(h - scrollHeight) * max);
            progress = Math.Clamp(progress, 0, Math.Max(0, max - maxView));
            
            if (!MouseHelper.IsMouseDown(MouseKey.Left))
            {
                _holdingScrollKey = null;
            }
        }
    }

    private static string? _activeInputKey;

    public static void DrawInput(string key, int x, int y, int width, ref string value, Overlay overlay, Graphics gfx)
    {
        var drawValue = value;
        if (_activeInputKey == key)
        {
            var carret = (_activeInputKey == key && (DateTime.Now.Millisecond % 600) < 300) ? "|" : "";
            drawValue += carret;
        }
        else if (string.IsNullOrEmpty(value))
        {
            drawValue = key;
        }

        gfx.DrawText(overlay.fonts["arial"], 14, overlay.colors["white2"], x, y, key);

        var sizeHeight = gfx.MeasureString(overlay.fonts["arial"], 14, key).Y;
        y += (int)sizeHeight + 5;
        
        var height = 26;
        var mouseOver = IsMouseInPosition(x, y, width, height);
        var textColor = (mouseOver || _activeInputKey == key) ? overlay.colors["white2"] : overlay.colors["white3"];

        DrawRoundedRectangle(gfx, x, y, width, height, overlay.colors["background-light2"], 4);
        DrawRoundedStrokeRectangle(gfx, x, y, width, height, overlay.colors["background-light3"], 1, 4);
        
        var size = gfx.MeasureString(overlay.fonts["arial"], 14, drawValue);
        gfx.DrawText(overlay.fonts["arial"], 14, textColor, x + 10, y + height / 2 - size.Y / 2, drawValue);

        if (mouseOver && MouseHelper.WasMousePressed(MouseKey.Left))
        {
            _activeInputKey = key;
        }
        else if (!mouseOver && _activeInputKey == key && MouseHelper.IsMouseDown(MouseKey.Left))
        {
            _activeInputKey = null;
        }

        if (_activeInputKey == key)
        {
            var chars = new List<string>() {
                "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M",
                "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z",
                "0", "1", "2", "3", "4", "5", "6", "7", "8", "9",
                "!", "@", "#", "$", "%", "^", "&", "*", "(", ")", "-", "_", "+", "=", "{", "}", "[", "]", "|", "\\", ":", ";", "'", "\"", "<", ">", ",", ".", "?", "/", " ", "Backspace", "Enter"
            };

            var isCapsLock = (ProcessHelper.GetKeyState(0x14) & 0x0001) != 0;

            foreach (var c in chars)
            {
                if (ProcessHelper.WasKeyPressed(c.ToString()))
                {
                    if (c == "Backspace")
                    {
                        if (value.Length > 0)
                        {
                            value = value.Substring(0, value.Length - 1);
                        }
                    }
                    else if (c == "Enter")
                    {
                        _activeInputKey = null;
                    }
                    else
                    {
                        value += isCapsLock ? c : c.ToLower();
                    }
                }
            }
        }
    }
}