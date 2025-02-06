namespace CS2.Hacks;

public class BaseHack
{
    public static async Task Loop()
    {
        var localPlayer = Globals.MemoryReader!.GetLocalPlayer();
        if (localPlayer == null)
        {
            Globals.MemoryReader.UpdateModules();
            await Loop();
            return;
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