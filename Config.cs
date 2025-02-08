using Newtonsoft.Json;

public static class Config
{
    public static string MenuKey = "Insert";

    public static class TriggerBot
    {
        public static bool Enabled = true;
        public static bool OnKey = true;
        public static bool FriendlyFire = false;
        public static int ShotDelay = 70;
        public static int DelayBetweenShots = 160;
        public static string Key = "LAlt";
    }

    public static class Esp
    {
        public static bool Box = false;
        public static bool Bones = true;
        public static bool FriendlyFire = false;
    }

    public static class Aimbot
    {
        public static bool Enabled = true;
        public static bool OnKey = true;
        public static bool DrawFOV = true;
        public static bool FriendlyFire = false;
        public static string Key = "LAlt";
        public static string Bone = "head";
        public static float Smooth = 0.25f;
        public static float Fov = 50;
    }

    public static void LoadConfig()
    {
        try
        {
            if (File.Exists("config.json"))
            {
                var json = File.ReadAllText("config.json");
                var config = JsonConvert.DeserializeObject<ConfigFile>(json);
                if (config == null)
                    throw new Exception("Failed to deserialize config.");

                MenuKey = config.MenuKey;

                TriggerBot.Enabled = config.TriggerBot.Enabled;
                TriggerBot.OnKey = config.TriggerBot.OnKey;
                TriggerBot.FriendlyFire = config.TriggerBot.FriendlyFire;
                TriggerBot.ShotDelay = config.TriggerBot.ShotDelay;
                TriggerBot.DelayBetweenShots = config.TriggerBot.DelayBetweenShots;
                TriggerBot.Key = config.TriggerBot.Key;

                Esp.Box = config.Esp.Box;
                Esp.Bones = config.Esp.Bones;
                Esp.FriendlyFire = config.Esp.FriendlyFire;

                Aimbot.FriendlyFire = config.Aimbot.FriendlyFire;
                Aimbot.OnKey = config.Aimbot.OnKey;
                Aimbot.DrawFOV = config.Aimbot.DrawFOV;
                Aimbot.Enabled = config.Aimbot.Enabled;
                Aimbot.Key = config.Aimbot.Key;
                Aimbot.Bone = config.Aimbot.Bone;
                Aimbot.Smooth = config.Aimbot.Smooth;
                Aimbot.Fov = config.Aimbot.Fov;
            }
            else
            {
                SaveConfig();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load config: {ex.Message}");
        }
    }

    public static void SaveConfig()
    {
        try
        {
            var config = new ConfigFile
            {
                MenuKey = MenuKey,
                Aimbot = new AimbotConfig
                {
                    Enabled = Aimbot.Enabled,
                    OnKey = Aimbot.OnKey,
                    DrawFOV = Aimbot.DrawFOV,
                    FriendlyFire = Aimbot.FriendlyFire,
                    Key = Aimbot.Key,
                    Bone = Aimbot.Bone,
                    Smooth = Aimbot.Smooth,
                    Fov = Aimbot.Fov
                },
                TriggerBot = new TriggerBotConfig
                {
                    Enabled = TriggerBot.Enabled,
                    OnKey = TriggerBot.OnKey,
                    FriendlyFire = TriggerBot.FriendlyFire,
                    ShotDelay = TriggerBot.ShotDelay,
                    DelayBetweenShots = TriggerBot.DelayBetweenShots,
                    Key = TriggerBot.Key
                },
                Esp = new EspConfig
                {
                    Box = Esp.Box,
                    Bones = Esp.Bones,
                    FriendlyFire = TriggerBot.FriendlyFire
                }
            };

            var json = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText("config.json", json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to save config: {ex.Message}");
        }
    }

    private class ConfigFile
    {
        public string MenuKey { get; set; } = "Insert";
        public TriggerBotConfig TriggerBot { get; set; } = new TriggerBotConfig();
        public EspConfig Esp { get; set; } = new EspConfig();
        public AimbotConfig Aimbot { get; set; } = new AimbotConfig();
    }

    private class TriggerBotConfig
    {
        public bool Enabled { get; set; } = true;
        public bool OnKey { get; set; } = true;
        public bool FriendlyFire { get; set; }
        public int ShotDelay { get; set; }
        public int DelayBetweenShots { get; set; }
        public string Key { get; set; } = "LAlt";
    }

    private class EspConfig
    {
        public bool Box { get; set; }
        public bool Bones { get; set; }
        public bool FriendlyFire { get; set; }
    }

    private class AimbotConfig
    {
        public bool Enabled { get; set; } = true;
        public bool OnKey { get; set; } = true;
        public bool DrawFOV { get; set; } = true;
        public bool FriendlyFire { get; set; }
        public string Key { get; set; } = "LAlt";
        public string Bone { get; set; } = "head";
        public float Smooth { get; set; } = 0.25f;
        public float Fov { get; set; } = 50;
    }
}