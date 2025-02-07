using CS2.Core.Memory;
using CS2.Hacks;
using GameOverlay.Drawing;

namespace CS2.Core;

public class Settings
{
    private static bool _menuOpen = false;
    private static DateTime _lastMenuInteraction = DateTime.Now;

    private static int _x = 300;
    private static int _y = 100;
    private static int _width = 200;
    private static int _height = 250;

    private static string _currentOpenMenu = "Triggerbot";

    private static bool _triggerBotKeyOpen = false;
    private static int _triggerBotKeyScrollPos = 0;

    private static bool _aimbotKeyOpen = false;
    private static int _aimbotKeyScrollPos = 0;
    private static bool _aimbotBoneOpen = false;
    private static int _aimbotBoneScrollPos = 0;

    public static void DrawSettings(Overlay overlay, Graphics gfx, System.Drawing.Point cursorPos)
    {
        int? menuKey = ProcessHelper.keyMap.ContainsKey(Config.MenuKey) ? ProcessHelper.keyMap[Config.MenuKey] : null;
        if (menuKey == null)
            return;

        if (menuKey.HasValue && ProcessHelper.GetAsyncKeyState(menuKey.Value) != 0 && (DateTime.Now - _lastMenuInteraction).TotalMilliseconds > 200)
        {
            _menuOpen = !_menuOpen;
            _lastMenuInteraction = DateTime.Now;
        }

        if (!_menuOpen)
            return;

        Windows.DrawWindow("Settings", _x, _y, _width, _height, overlay, gfx);
        (_x, _y) = Windows.MoveWindow("settings", _x, _y, _width, _height);
        (_width, _height) = Windows.ResizeWindow("settings", _x, _y, _width, _height, overlay, gfx);
        (_width, _height) = (Math.Max(_width, 400), Math.Max(_height, 250));

        // left side menu select
        // Windows.DrawRectangle(gfx, _x + 10, _y + 30, 180, 20, overlay.colors["black"]);
        var optionY = 0;

        void DrawOption(string text)
        {
            var textHeight = gfx.MeasureString(overlay.fonts["arial"], 14, text).Y;
            
            if (_currentOpenMenu == text)
            {
                // Windows.DrawRectangle(gfx, _x + 10, _y + 30 + optionY, 180, 20, overlay.colors["black"]);
                Windows.DrawRectangle(gfx, _x + 10, _y + 30 + optionY, 180, (int)textHeight + 10, overlay.colors["black"]);
            }

            // gfx.DrawText(overlay.fonts["arial"], 12, overlay.colors["white"], _x + 15, _y + 35 + optionY, text);
            gfx.DrawText(overlay.fonts["arial"], 14, overlay.colors["white"], _x + 15, _y + 30 + optionY + ((int)textHeight + 10)/2 - (int)textHeight/2, text);
            optionY += (int)textHeight + 10;

            if (Windows.IsMouseInPosition(_x + 10, _y + 30 + optionY - (int)textHeight - 10, 180, (int)textHeight + 10) && MouseHelper.IsMouseDown(MouseKey.Left))
            {
                _currentOpenMenu = text;
            }
        }

        DrawOption("Triggerbot");
        DrawOption("Aimbot");
        DrawOption("ESP");
        DrawOption("Settings");

        // draw vertical line between menu and settings
        Windows.DrawRectangle(gfx, _x + 200, _y + 30, 1, _height - 40, overlay.colors["black"]);

        if (_currentOpenMenu == "Triggerbot")
        {
            Windows.DrawCheckbox("Triggerbot Enabled", _x + 210, _y + 30, ref Config.TriggerBot.Enabled, overlay, gfx);
            Windows.DrawCheckbox("Friendly Fire", _x + 210, _y + 60, ref Config.TriggerBot.FriendlyFire, overlay, gfx);
            Windows.DrawSlider("Shot Delay", _x + 210, _y + 90, _width - 220, 20, ref Config.TriggerBot.ShotDelay, 0, 1000, overlay, gfx);
            Windows.DrawSlider("Delay Between Shots", _x + 210, _y + 120, _width - 220, 20, ref Config.TriggerBot.DelayBetweenShots, 0, 1000, overlay, gfx);
            Windows.DrawSelect("Triggerbot key", _x + 210, _y + 145, _width - 220, 20, ref Config.TriggerBot.Key, ProcessHelper.keyMap.Keys.ToList(), ref _triggerBotKeyOpen, ref _triggerBotKeyScrollPos, 10, overlay, gfx);
        }
        else if (_currentOpenMenu == "Aimbot")
        {
            Windows.DrawCheckbox("Aimbot Enabled", _x + 210, _y + 30, ref Config.Aimbot.Enabled, overlay, gfx);
            Windows.DrawCheckbox("Friendly Fire", _x + 210, _y + 60, ref Config.Aimbot.FriendlyFire, overlay, gfx);
            Windows.DrawSlider("Aimbot smooth", _x + 210, _y + 180, _width - 220, 20, ref Config.Aimbot.Smooth, 0, 1, overlay, gfx);
            Windows.DrawSlider("Aimbot FOV", _x + 210, _y + 205, _width - 220, 20, ref Config.Aimbot.FovInPx, 0, 1000, overlay, gfx);
            Windows.DrawSelect("Aimbot bone", _x + 210, _y + 135, _width - 220, 20, ref Config.Aimbot.Bone, Entity.BoneOffsets.Keys.ToList(), ref _aimbotBoneOpen, ref _aimbotBoneScrollPos, 10, overlay, gfx);
            Windows.DrawSelect("Aimbot key", _x + 210, _y + 90, _width - 220, 20, ref Config.Aimbot.Key, ProcessHelper.keyMap.Keys.ToList(), ref _aimbotKeyOpen, ref _aimbotKeyScrollPos, 10, overlay, gfx);
        }
        else if (_currentOpenMenu == "ESP")
        {
            Windows.DrawCheckbox("Box", _x + 210, _y + 30, ref Config.Esp.Box, overlay, gfx);
            Windows.DrawCheckbox("Bones", _x + 210, _y + 60, ref Config.Esp.Bones, overlay, gfx);
            Windows.DrawCheckbox("Friendly Fire", _x + 210, _y + 90, ref Config.Esp.FriendlyFire, overlay, gfx);
        }
        else if (_currentOpenMenu == "Settings")
        {
            // draw rectangle with text: Save config
            var textSize = gfx.MeasureString(overlay.fonts["arial"], 14, "Save config");
            Windows.DrawRectangle(gfx, _x + 210, _y + 80, (int)textSize.X + 20, (int)textSize.Y + 10, overlay.colors["black"]);
            gfx.DrawText(overlay.fonts["arial"], 14, overlay.colors["white"], _x + 210 + 10, _y + 80 + 5, "Save config");

            if (Windows.IsMouseInPosition(_x + 210, _y + 80, (int)textSize.X + 20, (int)textSize.Y + 10) && MouseHelper.IsMouseDown(MouseKey.Left))
            {
                Config.SaveConfig();
            }

            // menu key
            Windows.DrawSelect("Menu key", _x + 210, _y + 30, _width - 220, 20, ref Config.MenuKey, ProcessHelper.keyMap.Keys.ToList(), ref _triggerBotKeyOpen, ref _triggerBotKeyScrollPos, 10, overlay, gfx);
        }
    }
}