using System.Numerics;
using CS2.Core;
using CS2.Core.Memory;
using GameOverlay.Drawing;
using Newtonsoft.Json;

namespace CS2.Hacks;

public class Esp
{
    private static readonly List<(string, string)> BoneConnections = new()
    {
        // { "head", "neck_0" },
        // { "neck_0", "spine_1" },
        // { "spine_1", "spine_2" },
        // { "spine_2", "pelvis" },
        // { "spine_1", "arm_upper_L" },
        // { "arm_upper_L", "arm_lower_L" },
        // { "arm_lower_L", "hand_L" },
        // { "spine_1", "arm_upper_R" },
        // { "arm_upper_R", "arm_lower_R" },
        // { "arm_lower_R", "hand_R" },
        // { "pelvis", "leg_upper_L" },
        // { "leg_upper_L", "leg_lower_L" },
        // { "leg_lower_L", "ankle_L" },
        // { "pelvis", "leg_upper_R" },
        // { "leg_upper_R", "leg_lower_R" },
        // { "leg_lower_R", "ankle_R" }

        ("head", "neck_0"),
        ("neck_0", "spine_1"),
        ("spine_1", "spine_2"),
        ("spine_2", "pelvis"),
        ("spine_1", "arm_upper_L"),
        ("arm_upper_L", "arm_lower_L"),
        ("arm_lower_L", "hand_L"),
        ("spine_1", "arm_upper_R"),
        ("arm_upper_R", "arm_lower_R"),
        ("arm_lower_R", "hand_R"),
        ("pelvis", "leg_upper_L"),
        ("leg_upper_L", "leg_lower_L"),
        ("leg_lower_L", "ankle_L"),
        ("pelvis", "leg_upper_R"),
        ("leg_upper_R", "leg_lower_R"),
        ("leg_lower_R", "ankle_R")
    };

    public static async Task Loop(Overlay overlay, Graphics gfx)
    {
        var localPlayer = Globals.MemoryReader!.GetLocalPlayer();
        if (localPlayer == null)
            return;

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

            var screenPos = WorldToScreen(viewMatrix, bonePos.X, bonePos.Y, bonePos.Z, gfx.Width, gfx.Height);

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
            foreach (var boneConnection in BoneConnections)
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

    public static int[] WorldToScreen(float[] mtx, float posX, float posY, float posZ, float width, float height)
    {
        float screenW = (mtx[12] * posX) + (mtx[13] * posY) + (mtx[14] * posZ) + mtx[15];

        if (screenW > 0.001f)
        {
            float screenX = (mtx[0] * posX) + (mtx[1] * posY) + (mtx[2] * posZ) + mtx[3];
            float screenY = (mtx[4] * posX) + (mtx[5] * posY) + (mtx[6] * posZ) + mtx[7];

            float camX = width / 2;
            float camY = height / 2;

            int x = (int)(camX + (camX * screenX / screenW));
            int y = (int)(camY - (camY * screenY / screenW));

            if (x < 0 || x > width || y < 0 || y > height || screenW < 0.001f)
                return [-999, -999];

            return [x, y];
        }

        return [-999, -999];
    }
}