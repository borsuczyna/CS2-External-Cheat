using System.Numerics;
using CS2.Core;
using CS2.Core.Memory;
using GameOverlay.Drawing;

namespace CS2.Hacks;

public class Aimbot
{
    public static DateTime LastShot = DateTime.Now;

    public static async Task Loop(Overlay overlay, Graphics gfx)
    {
        if (!Config.Aimbot.Enabled)
            return;

        var key = ProcessHelper.keyMap.ContainsKey(Config.Aimbot.Key) ? ProcessHelper.keyMap[Config.Aimbot.Key] : 0;
        if (key == 0)
            return;

        if (ProcessHelper.GetAsyncKeyState(key) == 0)
            return;

        var localPlayer = Globals.MemoryReader!.GetLocalPlayer();
        if (localPlayer == null)
            return;

        var entities = Globals.MemoryReader!.GetEntities();
        var viewMatrix = Globals.MemoryReader!.GetViewMatrix();

        Entity? closestEntity = null;
        float closestDistance = Config.Aimbot.FovInPx;
        var closestPos = new int[] { -999, -999 };
        var middleOfScreen = new Vector2(gfx.Width / 2, gfx.Height / 2);

        foreach (var entity in entities)
        {
            if (entity.AddressBase == localPlayer.ControllerBase)
                continue;

            if (!(entity.Team != localPlayer.Team || Config.Esp.FriendlyFire))
                continue;

            if (entity.Health2 <= 0)
                continue;

            entity.UpdateBonePos();

            var bonePos = GetBonePosition(gfx, entity, viewMatrix, Config.Aimbot.Bone);
            if (bonePos == null)
                continue;

            var distance = Vector2.Distance(middleOfScreen, new Vector2(bonePos[0], bonePos[1]));
            if (distance < closestDistance)
            {
                closestEntity = entity;
                closestDistance = distance;
                closestPos = bonePos;
            }
        }

        if (closestEntity != null)
        {
            var difference = new Vector2(closestPos[0], closestPos[1]) - middleOfScreen;

            var x = difference.X * Config.Aimbot.Smooth;
            var y = difference.Y * Config.Aimbot.Smooth;
            // add min threshold to 0.1
            if (difference.X != 0 && (int)x == 0)
            {
                x = difference.X > 0 ? 1 : -1;
            }

            if (difference.Y != 0 && (int)y == 0)
            {
                y = difference.Y > 0 ? 1 : -1;
            }

            MouseHelper.MoveMouseRelative((int)x, (int)y);
        }

        await Task.CompletedTask;
    }

    public static int[]? GetBonePosition(Graphics gfx, Entity entity, float[] viewMatrix, string bone)
    {
        if (entity.BonePos.ContainsKey(bone))
        {
            var bonePos = entity.BonePos[bone];
            if (bonePos == Vector3.Zero)
                return null;

            var screenPos = BaseHack.WorldToScreen(viewMatrix, bonePos.X, bonePos.Y, bonePos.Z, gfx.Width, gfx.Height);

            if (screenPos[0] == -999)
                return null;

            return screenPos;
        }

        return null;
    }
}