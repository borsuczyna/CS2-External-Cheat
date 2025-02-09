using System.Numerics;
using CS2.Core;
using GameOverlay.Drawing;

namespace CS2.Hacks;

public class RecoilControl
{
    private static Vector2 _lastHitOffset = Vector2.Zero;
    private static Vector2 _toApply = Vector2.Zero;

    public static async Task Loop(Overlay overlay, Graphics gfx)
    {
        if (!Config.RecoilControl.Enabled)
            return;

        if (Config.RecoilControl.OnKey)
        {
            if (!ProcessHelper.IsKeyDown(Config.RecoilControl.Key))
                return;
        }

        var localPlayer = Globals.MemoryReader!.GetLocalPlayer();
        if (localPlayer == null)
            return;

        var hitOffset = localPlayer.HitOffset;
        if (hitOffset == Vector2.Zero)
            return;

        var viewAngle = Globals.MemoryReader!.GetViewAngles();
        var diff = (hitOffset - _lastHitOffset) * 2;
        _lastHitOffset = hitOffset;

        var applyNow = new Vector2(diff.X * (1 - Config.RecoilControl.Smooth), diff.Y * (1 - Config.RecoilControl.Smooth));
        var applyLater = new Vector2(diff.X * Config.RecoilControl.Smooth, diff.Y * Config.RecoilControl.Smooth);
        var applyNowCache = _toApply * (1 - Config.RecoilControl.Smooth);
        _toApply -= applyNowCache;
        applyNow += applyNowCache;

        viewAngle[0] -= applyNow.X;
        viewAngle[1] -= applyNow.Y;

        _toApply += applyLater;

        Globals.MemoryReader!.SetViewAngles(viewAngle[0], viewAngle[1]);

        await Task.CompletedTask;
    }
}