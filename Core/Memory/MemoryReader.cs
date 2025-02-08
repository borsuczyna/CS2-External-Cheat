using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

namespace CS2.Core.Memory;

public struct c_utl_vector
{
    public ulong count;
    public ulong data;

    public override string ToString()
    {
        return $"count: {count}, data: {data}";
    }
}

public class MemoryReader
{
    private string _processName;
    private IntPtr processHandle;
    private Dictionary<string, IntPtr> _moduleOffsets = new();

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool ReadProcessMemory(
        IntPtr hProcess,
        IntPtr lpBaseAddress,
        byte[] lpBuffer,
        int dwSize,
        out int lpNumberOfBytesRead);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool WriteProcessMemory(
        IntPtr hProcess,
        IntPtr lpBaseAddress,
        byte[] lpBuffer,
        int nSize,
        out int lpNumberOfBytesWritten);

    public MemoryReader(string processName)
    {
        _processName = processName;
        UpdateModules();
    }

    public void UpdateModules()
    {
        // Get the process by name
        var process = Process.GetProcessesByName(_processName).FirstOrDefault();
        if (process == null)
        {
            throw new Exception("Process not found.");
        }

        processHandle = process.Handle;

        // Find the base address of the specified module
        foreach (ProcessModule module in process.Modules)
        {
            _moduleOffsets[module.ModuleName] = module.BaseAddress;
        }
    }

    public IntPtr GetModuleBase(string moduleName)
    {
        return _moduleOffsets[moduleName];
    }

    public IntPtr ReadLongLong(IntPtr address)
    {
        byte[] buffer = new byte[8]; // A long long is 8 bytes
        ReadProcessMemory(processHandle, address, buffer, buffer.Length, out _);
        return (IntPtr)BitConverter.ToInt64(buffer, 0);
    }

    public float[] ReadFloatArray(IntPtr address, int length)
    {
        byte[] buffer = new byte[length * 4]; // A float is 4 bytes
        ReadProcessMemory(processHandle, address, buffer, buffer.Length, out _);
        float[] floats = new float[length];
        for (int i = 0; i < length; i++)
        {
            floats[i] = BitConverter.ToSingle(buffer, i * 4);
        }
        return floats;
    }

    public void WriteFloatArray(IntPtr address, float[] floats)
    {
        byte[] buffer = new byte[floats.Length * 4]; // A float is 4 bytes
        for (int i = 0; i < floats.Length; i++)
        {
            byte[] floatBytes = BitConverter.GetBytes(floats[i]);
            Array.Copy(floatBytes, 0, buffer, i * 4, 4);
        }
        WriteProcessMemory(processHandle, address, buffer, buffer.Length, out _);
    }

    public int ReadInt(IntPtr address)
    {
        byte[] buffer = new byte[4]; // An int is 4 bytes
        ReadProcessMemory(processHandle, address, buffer, buffer.Length, out _);
        return BitConverter.ToInt32(buffer, 0);
    }

    public Vector3 ReadVector3(IntPtr address)
    {
        byte[] buffer = new byte[12]; // A Vector3 is 12 bytes
        ReadProcessMemory(processHandle, address, buffer, buffer.Length, out _);
        return new Vector3(
            BitConverter.ToSingle(buffer, 0),
            BitConverter.ToSingle(buffer, 4),
            BitConverter.ToSingle(buffer, 8)
        );
    }

    public Vector2 ReadVector2(IntPtr address)
    {
        byte[] buffer = new byte[8]; // A Vector2 is 8 bytes
        ReadProcessMemory(processHandle, address, buffer, buffer.Length, out _);
        return new Vector2(
            BitConverter.ToSingle(buffer, 0),
            BitConverter.ToSingle(buffer, 4)
        );
    }

    public string ReadString(IntPtr lpBaseAddress, int maxLength = 256)
    {
        byte[] buffer = new byte[maxLength];
        ReadProcessMemory(processHandle, lpBaseAddress, buffer, buffer.Length, out _);
        int nullCharIndex = Array.IndexOf(buffer, (byte)'\0');
        return nullCharIndex >= 0
            ? Encoding.UTF8.GetString(buffer, 0, nullCharIndex + 1).Trim()
            : Encoding.UTF8.GetString(buffer).Trim();
    }

    public c_utl_vector ReadUtlVector(IntPtr address)
    {
        byte[] buffer = new byte[16]; // A c_utl_vector is 16 bytes
        ReadProcessMemory(processHandle, address, buffer, buffer.Length, out _);
        return new c_utl_vector
        {
            count = BitConverter.ToUInt64(buffer, 0),
            data = BitConverter.ToUInt64(buffer, 8)
        };
    }

    public Entity? GetLocalPlayer()
    {
        try
        {
            // Read the local player base address
            var dwLocalPlayerOffset = Globals.Offsets!.Read<int>("client.dll:dwLocalPlayerPawn");
            var address = ReadLongLong(GetModuleBase("client.dll") + dwLocalPlayerOffset);
            if (address == IntPtr.Zero)
            {
                return null;
            }

            return new Entity(address, 0x0);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error reading local player: {e.Message}");
            return null;
        }
    }

    public Entity? GetEntity(int index)
    {
        try
        {
            // Read the entity list base address
            var dwEntityListOffset = Globals.Offsets!.Read<int>("client.dll:dwEntityList");
            IntPtr entList = ReadLongLong(GetModuleBase("client.dll") + dwEntityListOffset);
            
            // Calculate the entity entry
            IntPtr listEntryFirst = ReadLongLong(entList + 0x8 * (index >> 9) + 0x10);
            
            // Read and return the entity address
            var controllerBase = ReadLongLong(listEntryFirst + 0x78 * (index & 0x1FF));
            if (controllerBase == IntPtr.Zero)
            {
                return null;
            }

            var playerPawn = ReadInt(controllerBase + Globals.ClientOffsets!.Read<int>("client.dll:classes:CBasePlayerController:fields:m_hPawn"));
            var listEntrySecond = ReadLongLong(entList + 0x8 * ((playerPawn & 0x7FFF) >> 9) + 0x10);

            var addressBase = listEntrySecond == IntPtr.Zero
                ? IntPtr.Zero
                : ReadLongLong(listEntrySecond + 120 * (playerPawn & 0x1FF));

            return new Entity(controllerBase, addressBase);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error reading entity: {e.Message}");
            return null;
        }
    }

    public List<Entity> GetEntities()
    {
        var entities = new List<Entity>();
        for (int i = 0; i < 64; i++)
        {
            var entity = GetEntity(i);
            if (entity != null)
            {
                entities.Add(entity);
            }
        }

        return entities;
    }

    private static Matrix4x4 GetMatrixViewport(int width, int height, int x = 0, int y = 0, float minDepth = 0, float maxDepth = 1)
    {
        return new Matrix4x4
        {
            M11 = width * 0.5f,
            M12 = 0,
            M13 = 0,
            M14 = 0,

            M21 = 0,
            M22 = -height * 0.5f,
            M23 = 0,
            M24 = 0,

            M31 = 0,
            M32 = 0,
            M33 = maxDepth - minDepth,
            M34 = 0,

            M41 = x + width * 0.5f,
            M42 = y + height * 0.5f,
            M43 = minDepth,
            M44 = 1
        };
    }

    public float[] GetViewMatrix()
    {
        var dwViewMatrixOffset = Globals.Offsets!.Read<int>("client.dll:dwViewMatrix");
        var viewMatrixProjection = ReadFloatArray(GetModuleBase("client.dll") + dwViewMatrixOffset, 16);
        return viewMatrixProjection;
    }

    // bool PlayerPawn::GetViewAngle()
    // {
    //     return GetDataAddressWithOffset<Vec2>(Address, Offset::Pawn.angEyeAngles, this->ViewAngle);
    // }
    public float[] GetViewAngles()
    {
        var dwViewAnglesOffset = Globals.Offsets!.Read<int>("client.dll:dwViewAngles");
        var viewAngles = ReadFloatArray(GetModuleBase("client.dll") + dwViewAnglesOffset, 2);
        return viewAngles;
    }

    public void SetViewAngles(float x, float y)
    {
        var dwViewAnglesOffset = Globals.Offsets!.Read<int>("client.dll:dwViewAngles");
        var viewAngles = GetModuleBase("client.dll") + dwViewAnglesOffset;
        WriteFloatArray(viewAngles, [x, y]);
    }
}