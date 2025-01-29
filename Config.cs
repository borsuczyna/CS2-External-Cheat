using System;
using System.IO;
using Newtonsoft.Json;

public static class Config
{
    public static class TriggerBot
    {
        public static bool Enabled = true;
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

                TriggerBot.FriendlyFire = config.TriggerBot.FriendlyFire;
                TriggerBot.ShotDelay = config.TriggerBot.ShotDelay;
                TriggerBot.DelayBetweenShots = config.TriggerBot.DelayBetweenShots;
                TriggerBot.Key = config.TriggerBot.Key;

                Esp.Box = config.Esp.Box;
                Esp.Bones = config.Esp.Bones;
                Esp.FriendlyFire = config.Esp.FriendlyFire;
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
                TriggerBot = new TriggerBotConfig
                {
                    Enabled = TriggerBot.Enabled,
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
        public TriggerBotConfig TriggerBot { get; set; } = new TriggerBotConfig();
        public EspConfig Esp { get; set; } = new EspConfig();
    }

    private class TriggerBotConfig
    {
        public bool Enabled { get; set; } = true;
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
}