using System.Numerics;
using CS2.Core;
using CS2.Core.Memory;
using GameOverlay.Drawing;

namespace CS2.Hacks;

public class Esp
{
    public static async Task Loop(Overlay overlay, Graphics gfx)
    {
        var localPlayer = Globals.MemoryReader!.GetLocalPlayer();
        if (localPlayer == null)
        {
            Globals.MemoryReader.UpdateModules();
            await Loop(overlay, gfx);
            return;
        }

        var entities = Globals.MemoryReader!.GetEntities();
        var viewMatrix = Globals.MemoryReader!.GetViewMatrix();

        foreach (var entity in entities)
        {
            if (entity.AddressBase == localPlayer.ControllerBase)
                continue;

            entity.UpdateBonePos();

            if (entity.Team != localPlayer.Team || Config.Esp.FriendlyFire)
                DrawBox(overlay, gfx, entity, viewMatrix);
        }

        await Task.CompletedTask;
    }

    public static void DrawBox(Overlay overlay, Graphics gfx, Entity entity, float[] viewMatrix)
    {
        var minPos = new int[] { 9999, 9999 };
        var maxPos = new int[] { -9999, -9999 };
        var bonePositions = new Dictionary<string, int[]>();

        foreach (var bone in entity.BonePos)
        {
            var bonePos = bone.Value;
            if (bonePos == Vector3.Zero)
                continue;

            var screenPos = BaseHack.WorldToScreen(viewMatrix, bonePos.X, bonePos.Y, bonePos.Z, gfx.Width, gfx.Height);

            if (screenPos[0] == -999)
                continue;

            minPos[0] = Math.Min(minPos[0], screenPos[0]);
            minPos[1] = Math.Min(minPos[1], screenPos[1]);
            maxPos[0] = Math.Max(maxPos[0], screenPos[0]);
            maxPos[1] = Math.Max(maxPos[1], screenPos[1]);
            bonePositions[bone.Key] = screenPos;
        }

        if (minPos[0] == 9999 || maxPos[0] == -9999)
            return;
        
        if (Config.Esp.Box)
            gfx.DrawRectangle(overlay.colors["red"], minPos[0], minPos[1], maxPos[0], maxPos[1], 2);

        // draw bone lines
        if (Config.Esp.Bones)
        {
            foreach (var boneConnection in Entity.BoneConnections)
            {
                if (!bonePositions.ContainsKey(boneConnection.Item1) || !bonePositions.ContainsKey(boneConnection.Item2))
                    continue;

                var bonePos1 = bonePositions[boneConnection.Item1];
                var bonePos2 = bonePositions[boneConnection.Item2];
                if (bonePos1[0] == -999 || bonePos2[0] == -999)
                    continue;
                    
                gfx.DrawLine(overlay.colors["green"], bonePos1[0], bonePos1[1], bonePos2[0], bonePos2[1], 2);
            }
        }
    }
}