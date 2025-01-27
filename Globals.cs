using CS2.Core;
using CS2.Core.Memory;
using WindowsInput;

namespace CS2;

public static class Globals
{
    public static string ProcessName = "cs2";
    public static string ProjectName = "hoax";
    public static MemoryReader? MemoryReader;
    public static Overlay? Overlay;
    public static Offsets? Offsets;
    public static Offsets? ClientOffsets;
    public static InputSimulator Input = new();

    public static void Initialize()
    {
        Offsets = new Offsets("offsets.json");
        ClientOffsets = new Offsets("client_dll.json");
        MemoryReader = new MemoryReader(ProcessName);
        Overlay = new Overlay(ProcessName);
    }
}