using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace TileEditor
{
    public static class Input
    {
        public static KeyboardState LastKeyState { get; private set; }
        public static KeyboardState CurrentKeyState { get; private set; }
        public static MouseState LastMouseState { get; private set; }
        public static MouseState CurrentMouseState { get; private set; }

        public static Vector2 MouseWorldPosition { get; private set; }
        public static Vector2 MouseScreenPosition { get; private set; }

        public static void Update(GameTime time)
        {
            LastKeyState = CurrentKeyState;
            CurrentKeyState = Keyboard.GetState();

            LastMouseState = CurrentMouseState;
            CurrentMouseState = Mouse.GetState();

            MouseScreenPosition = CurrentMouseState.Position.ToVector2();

            // The world position is found by translating screen space using the camera matrix.
            MouseWorldPosition = Main.Camera.ScreenToWorldPosition(MouseScreenPosition);
        }

        public static bool KeyDown(Keys key)
        {
            return LastKeyState.IsKeyUp(key) && CurrentKeyState.IsKeyDown(key);
        }

        public static bool KeyPressed(Keys key)
        {
            return CurrentKeyState.IsKeyDown(key);
        }

        public static bool LeftMouseDown()
        {
            if (!(new Rectangle(0, 0, Main.Graphics.PreferredBackBufferWidth, Main.Graphics.PreferredBackBufferHeight).Contains(MouseScreenPosition)))
                return false;
            return LastMouseState.LeftButton == ButtonState.Released && CurrentMouseState.LeftButton == ButtonState.Pressed;
        }

        public static bool LeftMouseUp()
        {
            if (!(new Rectangle(0, 0, Main.Graphics.PreferredBackBufferWidth, Main.Graphics.PreferredBackBufferHeight).Contains(MouseScreenPosition)))
                return false;
            return LastMouseState.LeftButton == ButtonState.Pressed && CurrentMouseState.LeftButton == ButtonState.Released;
        }

        public static bool RightMouseDown()
        {
            if (!(new Rectangle(0, 0, Main.Graphics.PreferredBackBufferWidth, Main.Graphics.PreferredBackBufferHeight).Contains(MouseScreenPosition)))
                return false;
            return LastMouseState.RightButton == ButtonState.Released && CurrentMouseState.RightButton == ButtonState.Pressed;
        }

        public static bool RightMouseUp()
        {
            return LastMouseState.RightButton == ButtonState.Pressed && CurrentMouseState.RightButton == ButtonState.Released;
        }

        public static bool LeftMousePressed()
        {
            if (!(new Rectangle(0, 0, Main.Graphics.PreferredBackBufferWidth, Main.Graphics.PreferredBackBufferHeight).Contains(MouseScreenPosition)))
                return false;
            return CurrentMouseState.LeftButton == ButtonState.Pressed;
        }

        public static bool RightMousePressed()
        {
            if (!(new Rectangle(0, 0, Main.Graphics.PreferredBackBufferWidth, Main.Graphics.PreferredBackBufferHeight).Contains(MouseScreenPosition)))
                return false;
            return CurrentMouseState.RightButton == ButtonState.Pressed;
        }
    }
}
