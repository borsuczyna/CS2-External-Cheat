using System;
using System.IO;
using Newtonsoft.Json;

public static class Config
{
    public static class TriggerBot
    {
        public static bool FriendlyFire = true;
        public static int ShotDelay = 70;
        public static int DelayBetweenShots = 160;
    }

    public static class Esp
    {
        public static bool Box = false;
        public static bool Bones = true;
    }

    public static void LoadConfig()
    {
        try
        {
            if (File.Exists("config.json"))
            {
                var json = File.ReadAllText("config.json");
                var config = JsonConvert.DeserializeObject<ConfigFile>(json);

                TriggerBot.FriendlyFire = config.TriggerBot.FriendlyFire;
                TriggerBot.ShotDelay = config.TriggerBot.ShotDelay;
                TriggerBot.DelayBetweenShots = config.TriggerBot.DelayBetweenShots;

                Esp.Box = config.Esp.Box;
                Esp.Bones = config.Esp.Bones;
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
                    FriendlyFire = TriggerBot.FriendlyFire,
                    ShotDelay = TriggerBot.ShotDelay,
                    DelayBetweenShots = TriggerBot.DelayBetweenShots
                },
                Esp = new EspConfig
                {
                    Box = Esp.Box,
                    Bones = Esp.Bones
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
        public bool FriendlyFire { get; set; }
        public int ShotDelay { get; set; }
        public int DelayBetweenShots { get; set; }
    }

    private class EspConfig
    {
        public bool Box { get; set; }
        public bool Bones { get; set; }
    }
}