using System.Windows.Input;
using System.Runtime.InteropServices;

namespace CS2.Hacks;

public class TriggerBot
{
    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);

    public static DateTime LastShot = DateTime.Now;
    public static bool Shooting = false;

    static Dictionary<string, int> keyMap = new Dictionary<string, int>
    {
        { "LButton", 0x01 },       // Left mouse button
        { "RButton", 0x02 },       // Right mouse button
        { "Cancel", 0x03 },
        { "MButton", 0x04 },       // Middle mouse button
        { "Backspace", 0x08 },
        { "Tab", 0x09 },
        { "Clear", 0x0C },
        { "Enter", 0x0D },
        { "Shift", 0x10 },
        { "Ctrl", 0x11 },
        { "Alt", 0x12 },
        { "Pause", 0x13 },
        { "CapsLock", 0x14 },
        { "Esc", 0x1B },
        { "Space", 0x20 },
        { "PageUp", 0x21 },
        { "PageDown", 0x22 },
        { "End", 0x23 },
        { "Home", 0x24 },
        { "LeftArrow", 0x25 },
        { "UpArrow", 0x26 },
        { "RightArrow", 0x27 },
        { "DownArrow", 0x28 },
        { "Select", 0x29 },
        { "Print", 0x2A },
        { "Execute", 0x2B },
        { "PrintScreen", 0x2C },
        { "Insert", 0x2D },
        { "Delete", 0x2E },
        { "Help", 0x2F },
        { "0", 0x30 },
        { "1", 0x31 },
        { "2", 0x32 },
        { "3", 0x33 },
        { "4", 0x34 },
        { "5", 0x35 },
        { "6", 0x36 },
        { "7", 0x37 },
        { "8", 0x38 },
        { "9", 0x39 },
        { "A", 0x41 },
        { "B", 0x42 },
        { "C", 0x43 },
        { "D", 0x44 },
        { "E", 0x45 },
        { "F", 0x46 },
        { "G", 0x47 },
        { "H", 0x48 },
        { "I", 0x49 },
        { "J", 0x4A },
        { "K", 0x4B },
        { "L", 0x4C },
        { "M", 0x4D },
        { "N", 0x4E },
        { "O", 0x4F },
        { "P", 0x50 },
        { "Q", 0x51 },
        { "R", 0x52 },
        { "S", 0x53 },
        { "T", 0x54 },
        { "U", 0x55 },
        { "V", 0x56 },
        { "W", 0x57 },
        { "X", 0x58 },
        { "Y", 0x59 },
        { "Z", 0x5A },
        { "LWin", 0x5B },
        { "RWin", 0x5C },
        { "Apps", 0x5D },
        { "Sleep", 0x5F },
        { "Numpad0", 0x60 },
        { "Numpad1", 0x61 },
        { "Numpad2", 0x62 },
        { "Numpad3", 0x63 },
        { "Numpad4", 0x64 },
        { "Numpad5", 0x65 },
        { "Numpad6", 0x66 },
        { "Numpad7", 0x67 },
        { "Numpad8", 0x68 },
        { "Numpad9", 0x69 },
        { "Multiply", 0x6A },
        { "Add", 0x6B },
        { "Separator", 0x6C },
        { "Subtract", 0x6D },
        { "Decimal", 0x6E },
        { "Divide", 0x6F },
        { "F1", 0x70 },
        { "F2", 0x71 },
        { "F3", 0x72 },
        { "F4", 0x73 },
        { "F5", 0x74 },
        { "F6", 0x75 },
        { "F7", 0x76 },
        { "F8", 0x77 },
        { "F9", 0x78 },
        { "F10", 0x79 },
        { "F11", 0x7A },
        { "F12", 0x7B },
        { "NumLock", 0x90 },
        { "ScrollLock", 0x91 },
        { "LShift", 0xA0 },
        { "RShift", 0xA1 },
        { "LControl", 0xA2 },
        { "RControl", 0xA3 },
        { "LAlt", 0xA4 },
        { "RAlt", 0xA5 },
        { "VolumeMute", 0xAD },
        { "VolumeDown", 0xAE },
        { "VolumeUp", 0xAF },
        { "MediaNext", 0xB0 },
        { "MediaPrev", 0xB1 },
        { "MediaStop", 0xB2 },
        { "MediaPlayPause", 0xB3 },
        { "LaunchMail", 0xB4 },
        { "BrowserBack", 0xA6 },
        { "BrowserForward", 0xA7 }
    };

    public static async Task Loop()
    {
        var localPlayer = Globals.MemoryReader!.GetLocalPlayer();
        if (localPlayer == null)
        {
            Globals.MemoryReader.UpdateModules();
            await Loop();
            return;
        }

        var key = keyMap.ContainsKey(Config.TriggerBot.Key) ? keyMap[Config.TriggerBot.Key] : 0;
        if (key == 0)
            return;

        if (GetAsyncKeyState(key) == 0)
            return;

        if (Shooting || DateTime.Now.Subtract(LastShot).TotalMilliseconds < Config.TriggerBot.DelayBetweenShots)
            return;

        var entityId = localPlayer.EntityId;

        if (entityId > 0)
        {
            var entity = Globals.MemoryReader!.GetEntity(entityId);
            if (entity == null)
                return;

            if ((entity.Team != localPlayer.Team || Config.TriggerBot.FriendlyFire) && entity.Health > 0)
            {
                Shooting = true;

                if (Config.TriggerBot.ShotDelay > 0)
                {
                    await Task.Delay(Config.TriggerBot.ShotDelay);
                }
                
                Globals.Input.Mouse.LeftButtonClick();
                LastShot = DateTime.Now;
                Shooting = false;
            }
        }
    }
}