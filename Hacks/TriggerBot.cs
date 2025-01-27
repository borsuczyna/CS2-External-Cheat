namespace CS2.Hacks;

public class TriggerBot
{
    public static DateTime LastShot = DateTime.Now;
    public static bool Shooting = false;

    public static async Task Loop()
    {
        if (Shooting || DateTime.Now.Subtract(LastShot).TotalMilliseconds < Config.TriggerBot.DelayBetweenShots)
            return;
        
        var localPlayer = Globals.MemoryReader!.GetLocalPlayer();
        if (localPlayer == null)
        {
            Globals.MemoryReader.UpdateModules();
            await Loop();
            return;
        }

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