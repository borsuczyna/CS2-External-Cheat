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

        // draw aim fov
        var middleOfScreen = new Vector2(gfx.Width / 2, gfx.Height / 2);
        
        if (Config.Aimbot.DrawFOV)
        {
            var fovInPx = (int)(gfx.Width * Config.Aimbot.Fov / 180) * 1.3f;
            gfx.DrawCircle(overlay.colors["green"], middleOfScreen.X, middleOfScreen.Y, fovInPx, 2);
        }

        if (Settings._menuOpen)
            return;

        if (Config.Aimbot.OnKey)
        {
            var key = ProcessHelper.keyMap.ContainsKey(Config.Aimbot.Key) ? ProcessHelper.keyMap[Config.Aimbot.Key] : 0;
            if (key == 0)
                return;

            if (ProcessHelper.GetAsyncKeyState(key) == 0)
                return;
        }

        var localPlayer = Globals.MemoryReader!.GetLocalPlayer();
        if (localPlayer == null)
            return;

        var entities = Globals.MemoryReader!.GetEntities();
        var viewMatrix = Globals.MemoryReader!.GetViewMatrix();
        var entityPositions = new List<Vector3>();

        foreach (var entity in entities)
        {
            if (entity.AddressBase == localPlayer.ControllerBase)
                continue;

            if (!(entity.Team != localPlayer.Team || Config.Esp.FriendlyFire))
                continue;

            if (entity.Health2 <= 0)
                continue;

            entity.UpdateBonePos();
            entityPositions.Add(entity.BonePos[Config.Aimbot.Bone]);
        }

        var closestPos = Vector3.Zero;
        var closestYawPitch = Vector2.Zero;
        var closestDistance = Config.Aimbot.Fov;

        foreach (var pos in entityPositions)
        {
            var localPos = localPlayer.CameraPos;
            var viewAngle = Globals.MemoryReader!.GetViewAngles();
            var aimPos = pos;
            var oppPos = aimPos - localPos;

            var distance = Math.Sqrt(Math.Pow(oppPos.X, 2) + Math.Pow(oppPos.Y, 2));
            var yaw = Math.Atan2(oppPos.Y, oppPos.X) * 57.295779513 - viewAngle[1];
            var pitch = -Math.Atan(oppPos.Z / distance) * 57.295779513 - viewAngle[0];
            var norm = Math.Sqrt(Math.Pow(yaw, 2) + Math.Pow(pitch, 2));
            if (norm > Config.Aimbot.Fov)
                continue;

            if (norm < closestDistance)
            {
                closestPos = pos;
                closestYawPitch = new Vector2((float)pitch, (float)yaw);
                closestDistance = (float)norm;
            }
        }

        if (closestPos != Vector3.Zero)
        {
            var viewAngle = Globals.MemoryReader!.GetViewAngles();
            var pitch = closestYawPitch.X;
            var yaw = closestYawPitch.Y;

            yaw = yaw * (1 - Config.Aimbot.Smooth) + viewAngle[1];
            pitch = pitch * (1 - Config.Aimbot.Smooth) + viewAngle[0];

            var hitOffset = localPlayer.HitOffset;
            if (hitOffset != Vector2.Zero)
            {
                yaw -= hitOffset.Y*2;
                pitch -= hitOffset.X*2;
            }

            Globals.MemoryReader!.SetViewAngles(pitch, yaw);
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