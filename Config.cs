using Newtonsoft.Json;

public static class Config
{
    // public static string MenuKey = "Insert";
    public static int MenuKey = 0x2D;

    public static class TriggerBot
    {
        public static bool Enabled = true;
        public static bool OnKey = true;
        public static bool FriendlyFire = false;
        public static int ShotDelay = 70;
        public static int DelayBetweenShots = 160;
        public static int Key = 0x12;
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
        public static bool ControlRecoil = true;
        public static bool OnKey = true;
        public static bool DrawFOV = true;
        public static bool FriendlyFire = false;
        public static int Key = 0x12;
        public static string Bone = "head";
        public static float Smooth = 0.25f;
        public static string SmoothingMethod = "Linear";
        public static float Fov = 50;
    }

    public static void LoadConfig(string name)
    {
        try
        {
            var configName = $"configs/{name}.json";
            if (!Directory.Exists("configs"))
                Directory.CreateDirectory("configs");

            if (File.Exists(configName))
            {
                var json = File.ReadAllText(configName);
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
                Aimbot.ControlRecoil = config.Aimbot.ControlRecoil;
                Aimbot.DrawFOV = config.Aimbot.DrawFOV;
                Aimbot.Enabled = config.Aimbot.Enabled;
                Aimbot.Key = config.Aimbot.Key;
                Aimbot.Bone = config.Aimbot.Bone;
                Aimbot.Smooth = config.Aimbot.Smooth;
                Aimbot.Fov = config.Aimbot.Fov;
            }
            else
            {
                SaveConfig(name);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load config: {ex.Message}");
        }
    }

    public static void SaveConfig(string name)
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

            var configName = $"configs/{name}.json";
            if (!Directory.Exists("configs"))
                Directory.CreateDirectory("configs");
            
            File.WriteAllText(configName, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to save config: {ex.Message}");
        }
    }

    public static List<string> GetConfigList()
    {
        var configs = new List<string>();
        if (!Directory.Exists("configs"))
            Directory.CreateDirectory("configs");

        foreach (var file in Directory.GetFiles("configs"))
        {
            var name = Path.GetFileNameWithoutExtension(file);
            configs.Add(name);
        }

        return configs;
    }

    private class ConfigFile
    {
        public int MenuKey { get; set; } = 0x2D;
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
        public int Key { get; set; } = 0x12;
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
        public bool ControlRecoil { get; set; } = true;
        public bool DrawFOV { get; set; } = true;
        public bool FriendlyFire { get; set; }
        public int Key { get; set; } = 0x12;
        public string Bone { get; set; } = "head";
        public float Smooth { get; set; } = 0.25f;
        public float Fov { get; set; } = 50;
    }
}