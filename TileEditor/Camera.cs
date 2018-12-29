using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace TileEditor
{
    public class Camera
    {
        public Vector2 Position { get; set; }
        public float Rotation { get; set; }
        public float Zoom
        {
            get
            {
                return this._zoom;
            }
            set
            {
                this._zoom = Math.Max(Math.Min(value, 10), 0.005f);
            }
        }
        public Rectangle WorldViewBounds { get; private set; }
        private float _zoom = 1f;
        private Matrix _transform;
        private Matrix _inverse;

        public void UpdateMatrix(GraphicsDevice graphicsDevice)
        {
            _transform =
              Matrix.CreateTranslation(new Vector3(-Position.X, -Position.Y, 0)) *
                                         Matrix.CreateRotationZ(MathHelper.ToRadians(-Rotation)) *
                                         Matrix.CreateScale(new Vector3(Zoom, Zoom, 1)) *
                                         Matrix.CreateTranslation(new Vector3(graphicsDevice.Viewport.Width * 0.5f, graphicsDevice.Viewport.Height * 0.5f, 0));

            var reversed = Matrix.Invert(Main.Camera.GetMatrix());
            var topLeft = Vector2.Transform(new Vector2(0, 0), reversed);
            var bottomRight = Vector2.Transform(new Vector2(Main.Graphics.PreferredBackBufferWidth, Main.Graphics.PreferredBackBufferHeight), reversed);

            var r = WorldViewBounds;
            r.X = (int)topLeft.X;
            r.Y = (int)topLeft.Y;
            r.Width = (int)Math.Ceiling(bottomRight.X - topLeft.X);
            r.Height = (int)Math.Ceiling(bottomRight.Y - topLeft.Y);
            WorldViewBounds = r;

            // Inverted.
            _inverse = Matrix.Invert(Main.Camera.GetMatrix());
        }

        public Vector2 ScreenToWorldPosition(Vector2 screenPos)
        {
            return Vector2.Transform(screenPos, _inverse);
        }

        public Matrix GetMatrix()
        {
            return _transform;
        }
    }
}
