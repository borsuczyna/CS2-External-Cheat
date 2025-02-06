using System.Threading.Tasks;
using CS2.Hacks;
using GameOverlay.Drawing;

namespace CS2.Core;

public class Windows
{
    public static void DrawWatermark(Overlay overlay, Graphics gfx, System.Drawing.Point cursorPos)
	{		
		var infoText = $"{Globals.ProjectName} | FPS: {gfx.FPS} | Cursor: {cursorPos.X}, {cursorPos.Y}";
		gfx.DrawTextWithBackground(overlay.fonts["consolas"], overlay.colors["green"], overlay.colors["black"], 10, 10, infoText);
	}

    public static async Task UseBaseHack(Overlay overlay, Graphics gfx, System.Drawing.Point cursorPos)
    {
        await BaseHack.Loop();
    }

    public static async Task UseAimbot(Overlay overlay, Graphics gfx, System.Drawing.Point cursorPos)
    {
        await Aimbot.Loop(overlay, gfx);
    }

    public static async Task UseTriggerBot(Overlay overlay, Graphics gfx, System.Drawing.Point cursorPos)
    {
        await TriggerBot.Loop();
    }

    public static async Task DrawEsp(Overlay overlay, Graphics gfx, System.Drawing.Point cursorPos)
    {
        await Esp.Loop(overlay, gfx);
    }
}