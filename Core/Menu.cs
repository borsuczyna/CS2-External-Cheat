using System.Numerics;
using CS2.Core.Memory;
using CS2.Hacks;
using GameOverlay.Drawing;

namespace CS2.Core;

public class Menu
{
    public static bool _menuOpen = false;
    private static DateTime _lastMenuToggle = DateTime.Now;

    private static string _menuName = Globals.ProjectName.ToUpper();
    private static Vector2 _position = new(300, 100);
    private static Vector2 _size = new(600, 600);
    private static int _leftPanelSize = 170;
    private static int _headerSize = 50;
    private static int _roundiness = 6;

    private static bool _aimbotSelectingKey = false;
    private static bool _aimbotSmoothingSmoothingMethodOpen = false;
    private static int _aimbotSmoothingSmoothingMethodScrollPos = 0;
    private static bool _aimbotBoneOpen = false;
    private static int _aimbotBoneScrollPos = 0;
    
    private static string _selectedConfig = "";
    private static string _configName = "";
    private static int _configListScrollPos = 0;

    private static bool _holdingMenu = false;
    private static Vector2 _holdingMenuOffset = Vector2.Zero;

    private static Dictionary<string, List<string>> _menuPanels = new()
    {
        ["Combat"] = new() { "Aimbot", "Triggerbot", "Recoil control" },
        ["Visuals"] = new() { "Players" },
        ["Menu"] = new() { "Config" },
    };

    private static Dictionary<int, float> _menuPanelCache = new();
    private static int _currentPanel = 0;

    private static int _x => (int)_position.X;
    private static int _y => (int)_position.Y;
    private static int _width => (int)_size.X;
    private static int _height => (int)_size.Y;

    public static void DrawMenu(Overlay overlay, Graphics gfx, System.Drawing.Point cursorPos)
    {
        if (ProcessHelper.IsKeyDown(Config.MenuKey) && (DateTime.Now - _lastMenuToggle).TotalMilliseconds > 300)
        {
            _menuOpen = !_menuOpen;
            _lastMenuToggle = DateTime.Now;
        }

        if (!_menuOpen)
            return;

        DrawMenu(gfx, overlay);
        MoveMenu();
        DrawLeftPanel(gfx, overlay);

        // Draw current panel
        if (_currentPanel == 0)
            DrawAimbotPanel(gfx, overlay);
        else if (_currentPanel == 1)
            DrawTriggerbotPanel(gfx, overlay);
        else if (_currentPanel == 2)
            DrawRecoilControlPanel(gfx, overlay);
        else if (_currentPanel == 3)
            DrawVisualsPanel(gfx, overlay);
        else
            DrawConfigPanel(gfx, overlay);
    }

    private static void DrawMenu(Graphics gfx, Overlay overlay)
    {
        Windows.DrawRoundedRectangle(gfx, _x, _y, _width, _height, overlay.colors["background-dark"], _roundiness);
        Windows.DrawRoundedRectangle(gfx, _x + _leftPanelSize, _y + _headerSize, _width - _leftPanelSize, _height - _headerSize, overlay.colors["background-light"], _roundiness);
        
        // draw line between header and left panel
        gfx.DrawLine(overlay.colors["background-light2"], _x + _leftPanelSize, _y + _headerSize, _x + _leftPanelSize, _y + _height, 1);

        // draw line between header and content
        gfx.DrawLine(overlay.colors["background-light2"], _x, _y + _headerSize, _x + _width, _y + _headerSize, 1);

        var textSize = gfx.MeasureString(overlay.fonts["consolas2"], 24, _menuName);
        gfx.DrawText(overlay.fonts["consolas2"], 24, overlay.colors["white2"], _x + 20, _y + (_headerSize/2) - (int)textSize.Y/2, _menuName);
    }

    private static void MoveMenu()
    {
        var cursorPos = Overlay.GetCursorPosition();
        if (Windows.IsMouseInPosition(_x, _y, _width, _headerSize) && MouseHelper.WasMousePressed(MouseKey.Left))
        {
            _holdingMenu = true;
            _holdingMenuOffset = new Vector2(cursorPos.X - _x, cursorPos.Y - _y);
        }

        if (_holdingMenu && MouseHelper.IsMouseDown(MouseKey.Left))
        {
            _position = new Vector2(cursorPos.X - _holdingMenuOffset.X, cursorPos.Y - _holdingMenuOffset.Y);
        }
        else
        {
            _holdingMenu = false;
        }
    }

    private static void DrawLeftPanel(Graphics gfx, Overlay overlay)
    {
        var panelY = _y + _headerSize + 10;

        void DrawPanelHeader(string text)
        {
            var textSize = gfx.MeasureString(overlay.fonts["arial"], 11, text);
            gfx.DrawText(overlay.fonts["arial"], 11, overlay.colors["white3"], _x + 20, panelY, text);
            panelY += (int)textSize.Y + 4;
        }

        void DrawPanelOption(string text, int index)
        {
            var isActive = _currentPanel == index;
            var textSize = gfx.MeasureString(overlay.fonts["arial"], 14, text);
            var (x, y, w, h) = (_x, panelY, _leftPanelSize, (int)textSize.Y + 12);
            var mouseOver = Windows.IsMouseInPosition(x, y, w, h);

            if (!_menuPanelCache.ContainsKey(index))
                _menuPanelCache[index] = 0;
        
            _menuPanelCache[index] = Math.Clamp(_menuPanelCache[index] + (mouseOver ? 0.4f : -0.4f), 0, 5);

            var color = isActive ?
                overlay.colors["background-hover"] :
                (mouseOver ? overlay.colors["background-hover-transparent"] : null);

            var textColor = isActive ? 
                overlay.colors["active"] : 
                (mouseOver ? overlay.colors["white"] : overlay.colors["white2"]);

            if (color != null)
                Windows.DrawRectangle(gfx, x, y, w, h, color);
            
            gfx.DrawText(overlay.fonts["arial"], 14, textColor, _x + 20 + _menuPanelCache[index], panelY + 6, text);
            panelY += (int)textSize.Y + 13;

            if (mouseOver && MouseHelper.WasMousePressed(MouseKey.Left))
            {
                _currentPanel = index;
            }
        }

        var index = 0;
        foreach (var panel in _menuPanels)
        {
            DrawPanelHeader(panel.Key);
            foreach (var option in panel.Value)
            {
                DrawPanelOption(option, index);
                index++;
            }

            panelY += 10;
        }
    }

    private static void DrawSection(Graphics gfx, Overlay overlay, string text, int x, int y, int width, int height)
    {
        var heightOffset = gfx.MeasureString(overlay.fonts["consolas"], 13, text).Y;

        Windows.DrawRoundedRectangle(gfx, x, y, width, height, overlay.colors["background-dark"], _roundiness);
        gfx.DrawText(overlay.fonts["consolas"], 13, overlay.colors["white3"], x + 12, y + 6, text);
        gfx.DrawLine(overlay.colors["background-light2"], x + 12, y + heightOffset + 12, x + width - 12, y + heightOffset + 6, 1);
    }

    private static void DrawAimbotPanel(Graphics gfx, Overlay overlay)
    {
        Windows.InteractSelect("Smoothing method", _x + _leftPanelSize + 20, _y + _headerSize + 425, _width - _leftPanelSize - 40, ref Config.Aimbot.SmoothingMethod, Aimbot.SmoothingMethods.Keys.ToList(), ref _aimbotSmoothingSmoothingMethodOpen, ref _aimbotSmoothingSmoothingMethodScrollPos, 5, overlay, gfx);
        Windows.InteractSelect("Bone", _x + _leftPanelSize + 20, _y + _headerSize + 105, _width - _leftPanelSize - 40, ref Config.Aimbot.Bone, Entity.BoneOffsets.Keys.ToList(), ref _aimbotBoneOpen, ref _aimbotBoneScrollPos, 5, overlay, gfx);

        var smoothEnabled = Config.Aimbot.Smooth > 0;
        var sectionHeight = 315 + (smoothEnabled ? 105 : 0) + (Config.Aimbot.OnKey ? 55 : 0);
        DrawSection(
            gfx, overlay,
            text: "Aimbot",
            x: _x + _leftPanelSize + 10,
            y: _y + _headerSize + 10,
            width: _width - _leftPanelSize - 20,
            height: sectionHeight
        );

        var y = 15;
        Windows.DrawCheckbox("Enable", _x + _leftPanelSize + 20, _y + _headerSize + (y+=30), ref Config.Aimbot.Enabled, overlay, gfx);
        Windows.DrawCheckbox("Control recoil", _x + _leftPanelSize + 20, _y + _headerSize + (y+=30), ref Config.Aimbot.ControlRecoil, overlay, gfx);
        Windows.DrawCheckbox("Friendly fire", _x + _leftPanelSize + 20, _y + _headerSize + (y+=85), ref Config.Aimbot.FriendlyFire, overlay, gfx);
        Windows.DrawCheckbox("Draw FOV", _x + _leftPanelSize + 20, _y + _headerSize + (y+=30), ref Config.Aimbot.DrawFOV, overlay, gfx);
        Windows.DrawSlider("FOV", _x + _leftPanelSize + 20, _y + _headerSize + (y+=30), _width - _leftPanelSize - 40, ref Config.Aimbot.Fov, 0, 180, overlay, gfx);
        Windows.DrawCheckbox("When holding key", _x + _leftPanelSize + 20, _y + _headerSize + (y+=45), ref Config.Aimbot.OnKey, overlay, gfx);

        if (Config.Aimbot.OnKey)
        {
            y += 30;
            Windows.DrawKeySelect("Key", _x + _leftPanelSize + 20, _y + _headerSize + y, _width - _leftPanelSize - 40, ref Config.Aimbot.Key, ref _aimbotSelectingKey, overlay, gfx);
            y += 55;
        }
        else
        {
            y += 30;
        }
    
        Windows.DrawCheckbox("Smoothing", _x + _leftPanelSize + 20, _y + _headerSize + y, ref smoothEnabled, overlay, gfx);

        if (smoothEnabled)
        {
            Config.Aimbot.Smooth = Math.Max(0.01f, Config.Aimbot.Smooth);

            Windows.DrawSlider("Smooth", _x + _leftPanelSize + 20, _y + _headerSize + (y+=30), _width - _leftPanelSize - 40, ref Config.Aimbot.Smooth, 0, 1, overlay, gfx);
            Windows.DrawSelect("Smoothing method", _x + _leftPanelSize + 20, _y + _headerSize + 425, _width - _leftPanelSize - 40, ref Config.Aimbot.SmoothingMethod, Aimbot.SmoothingMethods.Keys.ToList(), ref _aimbotSmoothingSmoothingMethodOpen, ref _aimbotSmoothingSmoothingMethodScrollPos, 5, overlay, gfx);
        }
        else
        {
            Config.Aimbot.Smooth = 0;
        }

        Windows.DrawSelect("Bone", _x + _leftPanelSize + 20, _y + _headerSize + 105, _width - _leftPanelSize - 40, ref Config.Aimbot.Bone, Entity.BoneOffsets.Keys.ToList(), ref _aimbotBoneOpen, ref _aimbotBoneScrollPos, 5, overlay, gfx);
    }

    private static void DrawTriggerbotPanel(Graphics gfx, Overlay overlay)
    {
        var sectionHeight = 215 + (Config.TriggerBot.OnKey ? 55 : 0);
        DrawSection(
            gfx, overlay,
            text: "Triggerbot",
            x: _x + _leftPanelSize + 10,
            y: _y + _headerSize + 10,
            width: _width - _leftPanelSize - 20,
            height: sectionHeight
        );

        var y = 15;
        Windows.DrawCheckbox("Enable", _x + _leftPanelSize + 20, _y + _headerSize + (y+=30), ref Config.TriggerBot.Enabled, overlay, gfx);
        Windows.DrawCheckbox("Friendly fire", _x + _leftPanelSize + 20, _y + _headerSize + (y+=30), ref Config.TriggerBot.FriendlyFire, overlay, gfx);
        Windows.DrawCheckbox("When holding key", _x + _leftPanelSize + 20, _y + _headerSize + (y+=30), ref Config.TriggerBot.OnKey, overlay, gfx);

        if (Config.TriggerBot.OnKey)
        {
            y += 30;
            Windows.DrawKeySelect("Key", _x + _leftPanelSize + 20, _y + _headerSize + y, _width - _leftPanelSize - 40, ref Config.TriggerBot.Key, ref _aimbotSelectingKey, overlay, gfx);
            y += 55;
        }
        else
        {
            y += 30;
        }

        Windows.DrawSlider("Shot delay", _x + _leftPanelSize + 20, _y + _headerSize + y, _width - _leftPanelSize - 40, ref Config.TriggerBot.ShotDelay, 0, 1000, overlay, gfx);
        Windows.DrawSlider("Delay between shots", _x + _leftPanelSize + 20, _y + _headerSize + (y+=45), _width - _leftPanelSize - 40, ref Config.TriggerBot.DelayBetweenShots, 0, 1000, overlay, gfx);
    }

    private static void DrawRecoilControlPanel(Graphics gfx, Overlay overlay)
    {
        var sectionHeight = 140 + (Config.RecoilControl.OnKey ? 60 : 0);
        DrawSection(
            gfx, overlay,
            text: "Recoil control",
            x: _x + _leftPanelSize + 10,
            y: _y + _headerSize + 10,
            width: _width - _leftPanelSize - 20,
            height: sectionHeight
        );

        var y = 15;
        Windows.DrawCheckbox("Enable", _x + _leftPanelSize + 20, _y + _headerSize + (y+=30), ref Config.RecoilControl.Enabled, overlay, gfx);
        Windows.DrawCheckbox("When holding key", _x + _leftPanelSize + 20, _y + _headerSize + (y+=30), ref Config.RecoilControl.OnKey, overlay, gfx);

        if (Config.RecoilControl.OnKey)
        {
            y += 30;
            Windows.DrawKeySelect("Key", _x + _leftPanelSize + 20, _y + _headerSize + y, _width - _leftPanelSize - 40, ref Config.RecoilControl.Key, ref _aimbotSelectingKey, overlay, gfx);
            y += 55;
        }
        else
        {
            y += 30;
        }

        Windows.DrawSlider("Smooth", _x + _leftPanelSize + 20, _y + _headerSize + y, _width - _leftPanelSize - 40, ref Config.RecoilControl.Smooth, 0, 1, overlay, gfx);
    }

    private static void DrawVisualsPanel(Graphics gfx, Overlay overlay)
    {
        var sectionHeight = 125;
        DrawSection(
            gfx, overlay,
            text: "ESP",
            x: _x + _leftPanelSize + 10,
            y: _y + _headerSize + 10,
            width: _width - _leftPanelSize - 20,
            height: sectionHeight
        );

        var y = 15;
        Windows.DrawCheckbox("Box", _x + _leftPanelSize + 20, _y + _headerSize + (y+=30), ref Config.Esp.Box, overlay, gfx);
        Windows.DrawCheckbox("Bones", _x + _leftPanelSize + 20, _y + _headerSize + (y+=30), ref Config.Esp.Bones, overlay, gfx);
        Windows.DrawCheckbox("Show teammates", _x + _leftPanelSize + 20, _y + _headerSize + (y+=30), ref Config.Esp.FriendlyFire, overlay, gfx);
    }

    // draw 2 buttons: save and load
    private static void DrawConfigPanel(Graphics gfx, Overlay overlay)
    {
        var sectionHeight = 345;
        DrawSection(
            gfx, overlay,
            text: "Config",
            x: _x + _leftPanelSize + 10,
            y: _y + _headerSize + 10,
            width: _width - _leftPanelSize - 20,
            height: sectionHeight
        );

        var y = 15;

        Windows.DrawKeySelect("Menu key", _x + _leftPanelSize + 20, _y + _headerSize + (y+=30), _width - _leftPanelSize - 40, ref Config.MenuKey, ref _aimbotSelectingKey, overlay, gfx);
        Windows.DrawInput("Config name", _x + _leftPanelSize + 20, _y + _headerSize + (y+=55), _width - _leftPanelSize - 140, ref _configName, overlay, gfx);
        _configName = _configName.Substring(0, Math.Min(_configName.Length, 20));
        
        Windows.DrawButton("Save config", _x + _leftPanelSize + 20 + (_width - _leftPanelSize - 132), _y + _headerSize + y + 20, 92, 27, overlay, gfx);

        if (Windows.IsMouseInPosition(_x + _leftPanelSize + 20 + (_width - _leftPanelSize - 132), _y + _headerSize + y + 20, 92, 27) && MouseHelper.WasMousePressed(MouseKey.Left))
        {
            if (string.IsNullOrEmpty(_configName))
                Notifications.AddNotification("Config", "Please enter a config name", NotificationType.Danger);
            else
            {
                Config.SaveConfig(_configName);
                Notifications.AddNotification("Config", $"Config '{_configName}' saved", NotificationType.Success);
            }
        }

        Windows.DrawList("Config", _x + _leftPanelSize + 20, _y + _headerSize + (y+=55), _width - _leftPanelSize - 40, Config.GetConfigList(), ref _selectedConfig, 5, ref _configListScrollPos, overlay, gfx);
        Windows.DrawButton("Load config", _x + _leftPanelSize + 20, _y + _headerSize + y + 160, _width - _leftPanelSize - 40, 27, overlay, gfx);
    
        if (Windows.IsMouseInPosition(_x + _leftPanelSize + 20, _y + _headerSize + y + 160, _width - _leftPanelSize - 40, 27) && MouseHelper.WasMousePressed(MouseKey.Left))
        {
            if (string.IsNullOrEmpty(_selectedConfig))
                Notifications.AddNotification("Config", "Please select a config", NotificationType.Danger);
            else
            {
                Config.LoadConfig(_selectedConfig);
                Notifications.AddNotification("Config", $"Config '{_selectedConfig}' loaded", NotificationType.Success);
            }
        }
    }
}