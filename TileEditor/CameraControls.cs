using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TileEditor
{
    public static class CameraControls
    {
        public static float TargetZoom = 1f;

        public static void Update()
        {
            if (!(new Rectangle(0, 0, Main.Graphics.PreferredBackBufferWidth, Main.Graphics.PreferredBackBufferHeight).Contains(Input.MouseScreenPosition)))
                return;

            // Zoom...
            int scrollDelta = Input.CurrentMouseState.ScrollWheelValue - Input.LastMouseState.ScrollWheelValue;
            if(scrollDelta > 0)
            {
                TargetZoom *= 1.1f;
            }
            else if(scrollDelta < 0)
            {
                TargetZoom /= 1.1f;
            }
            TargetZoom = MathHelper.Clamp(TargetZoom, Camera.MIN_ZOOM, Camera.MAX_ZOOM);
            Main.Camera.Zoom = MathHelper.Lerp(Main.Camera.Zoom, TargetZoom, 0.5f);


            // Panning...
            bool down = Input.CurrentMouseState.MiddleButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed;
            Point pixelDelta = Input.CurrentMouseState.Position - Input.LastMouseState.Position;
            if (down)
            {
                Main.Camera.Position -= new Vector2(pixelDelta.X / Main.Camera.Zoom, pixelDelta.Y / Main.Camera.Zoom);
            }
        }
    }
}
