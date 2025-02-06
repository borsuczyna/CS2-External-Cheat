using System.Numerics;

namespace CS2.Core.Memory;

public class Entity
{
    public IntPtr ControllerBase;
    public IntPtr AddressBase;
    public static readonly Dictionary<string, int> BoneOffsets = new()
    {
        { "head", 6 },
        { "neck_0", 5 },
        { "spine_1", 4 },
        { "spine_2", 2 },
        { "pelvis", 0 },
        { "arm_upper_L", 8 },
        { "arm_lower_L", 9 },
        { "hand_L", 10 },
        { "arm_upper_R", 13 },
        { "arm_lower_R", 14 },
        { "hand_R", 15 },
        { "leg_upper_L", 22 },
        { "leg_lower_L", 23 },
        { "ankle_L", 24 },
        { "leg_upper_R", 25 },
        { "leg_lower_R", 26 },
        { "ankle_R", 27 }
    };

    public Dictionary<string, Vector3> BonePos { get; } = new()
    {
        { "head", Vector3.Zero },
        { "neck_0", Vector3.Zero },
        { "spine_1", Vector3.Zero },
        { "spine_2", Vector3.Zero },
        { "pelvis", Vector3.Zero },
        { "arm_upper_L", Vector3.Zero },
        { "arm_lower_L", Vector3.Zero },
        { "hand_L", Vector3.Zero },
        { "arm_upper_R", Vector3.Zero },
        { "arm_lower_R", Vector3.Zero },
        { "hand_R", Vector3.Zero },
        { "leg_upper_L", Vector3.Zero },
        { "leg_lower_L", Vector3.Zero },
        { "ankle_L", Vector3.Zero },
        { "leg_upper_R", Vector3.Zero },
        { "leg_lower_R", Vector3.Zero },
        { "ankle_R", Vector3.Zero }
    };

    public static readonly List<(string, string)> BoneConnections = new()
    {
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

    public Entity(IntPtr controllerBase, IntPtr addressBase)
    {
        ControllerBase = controllerBase;
        AddressBase = addressBase;
    }

    public int Team
    {
        get
        {
            try
            {
                var m_iTeamNum = Globals.ClientOffsets!.Read<int>("client.dll:classes:C_BaseEntity:fields:m_iTeamNum");
                return Globals.MemoryReader!.ReadInt(ControllerBase + m_iTeamNum);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error reading entity team: {e.Message}");
                return 0;
            }
        }
    }

    public int Health
    {
        get
        {
            try
            {
                var m_iHealth = Globals.ClientOffsets!.Read<int>("client.dll:classes:C_BaseEntity:fields:m_iHealth");
                return Globals.MemoryReader!.ReadInt(ControllerBase + m_iHealth);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error reading entity health: {e.Message}");
                return 0;
            }
        }
    }

    
    public int Health2
    {
        get
        {
            try
            {
                var m_iHealth = Globals.ClientOffsets!.Read<int>("client.dll:classes:C_BaseEntity:fields:m_iHealth");
                return Globals.MemoryReader!.ReadInt(AddressBase + m_iHealth);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error reading entity health: {e.Message}");
                return 0;
            }
        }
    }

    public int AimingAtEntityId
    {
        get
        {
            try
            {
                var m_iIDEntIndex = Globals.ClientOffsets!.Read<int>("client.dll:classes:C_CSPlayerPawnBase:fields:m_iIDEntIndex");
                return Globals.MemoryReader!.ReadInt(ControllerBase + m_iIDEntIndex);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error reading entity ID: {e.Message}");
                return 0;
            }
        }
    }

    public string Name
    {
        get
        {
            try
            {
                var m_iszPlayerName = Globals.ClientOffsets!.Read<int>("client.dll:classes:CBasePlayerController:fields:m_iszPlayerName");
                return Globals.MemoryReader!.ReadString(ControllerBase + m_iszPlayerName);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error reading entity name: {e.Message}");
                return string.Empty;
            }
        }
    }

    public void UpdateBonePos()
    {
        try
        {
            var gameSceneNode = Globals.MemoryReader!.ReadLongLong(AddressBase + Globals.ClientOffsets!.Read<int>("client.dll:classes:C_BaseEntity:fields:m_pGameSceneNode"));
            var boneArray = Globals.MemoryReader!.ReadLongLong(gameSceneNode + Globals.ClientOffsets!.Read<int>("client.dll:classes:CSkeletonInstance:fields:m_modelState") + 128);
            if (boneArray == IntPtr.Zero)
                return;

            foreach (var bone in BoneOffsets)
            {
                var boneAddress = boneArray + bone.Value * 32;
                var bonePos = Globals.MemoryReader!.ReadVector3(boneAddress);
                BonePos[bone.Key] = bonePos;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error updating bone positions: {e.Message}\n{e.InnerException}");
        }
    }

    public Vector3 GetBonePos(string bone)
    {
        return BonePos.ContainsKey(bone) ? BonePos[bone] : Vector3.Zero;
    }

    public override string ToString()
    {
        return $"Entity @ {ControllerBase}";
    }
}