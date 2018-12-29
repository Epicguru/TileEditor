using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace TileEditor
{
    public class Main : Game
    {
        public const string VERSION = "0.0.1";
        public static GraphicsDeviceManager Graphics;
        public static SpriteBatch SpriteBatch;
        public static Camera Camera;
        public static EventDispatcher Dispatcher;
        public static MouseState CurrentMouseState;
        public static Texture2D Pixel;
        public static Texture2D CurrentTexture;
        public static SpriteFont SmallFont;

        private RenderTarget2D FrameBuffer;
        private static string CurrentFileName;
        private static string CurrentFilePath;
        private static MouseState PrevousMouseState;

        public Main()
        {
            Graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            base.IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            base.Initialize();

            Window.Title = "Tile Editor v" + VERSION;
            Window.AllowUserResizing = true;
            Window.AllowAltF4 = false;

            Camera = new Camera();
            Dispatcher = new EventDispatcher();
            ResizeBuffer(Graphics.PreferredBackBufferWidth, Graphics.PreferredBackBufferHeight);
        }

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            SpriteBatch = new SpriteBatch(GraphicsDevice);

            SmallFont = Content.Load<SpriteFont>("Fonts/Small");
            Pixel = Content.Load<Texture2D>("Pixel");
        }

        protected override void UnloadContent()
        {
            base.UnloadContent();

            if(CurrentTexture != null)
            {
                CurrentTexture.Dispose();
                CurrentTexture = null;
            }
        }

        protected void ResizeBuffer(int w, int h)
        {
            if(FrameBuffer != null)
            {
                FrameBuffer.Dispose();
                FrameBuffer = null;
            }

            if (w <= 0)
                w = 1;
            if (h <= 0)
                h = 1;

            FrameBuffer = new RenderTarget2D(GraphicsDevice, w, h, false, SurfaceFormat.Color, DepthFormat.None);
            Console.WriteLine("Resized frame buffer to {0}x{1}", w, h);
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if(Graphics.PreferredBackBufferWidth != FrameBuffer.Width || Graphics.PreferredBackBufferHeight != FrameBuffer.Height)
            {
                ResizeBuffer(Graphics.PreferredBackBufferWidth, Graphics.PreferredBackBufferHeight);
            }

            PrevousMouseState = CurrentMouseState;
            CurrentMouseState = Mouse.GetState();

            Dispatcher.DispatchEvents();
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.SetRenderTarget(FrameBuffer);
            GraphicsDevice.Clear(Color.LightSlateGray);

            Camera.UpdateMatrix(GraphicsDevice);
            SpriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, Camera.GetMatrix());
            DrawScene();
            SpriteBatch.End();
            GraphicsDevice.SetRenderTarget(null);

            SpriteBatch.Begin();
            // Draw real scene from render target...
            SpriteBatch.Draw(FrameBuffer, Vector2.Zero, Color.White);
            DrawUI();
            SpriteBatch.End();

            base.Draw(gameTime);
        }

        private void DrawUI()
        {     
            // Save and load buttons.
            bool openPressed = DrawButton(SmallFont, new Vector2(0, 0), "Open...");
            if (openPressed && !FileDialogeOpener.AnyOpen)
            {
                FileDialogeOpener.OpenDialoge(FileSelected);
            }

            bool savePressed = DrawButton(SmallFont, new Vector2(0, 24), "Save...");
            if (savePressed)
            {
                SaveCurrent();
            }

            // Name of current file.
            SpriteBatch.DrawString(SmallFont, "Editing: " + (CurrentFileName ?? "None"), new Vector2(65, 0), Color.Black);

            // Current color and label.
            float width = SmallFont.MeasureString("Active Color").X;
            SpriteBatch.DrawString(SmallFont, "Active Color", new Vector2(Graphics.PreferredBackBufferWidth - width - 3, 2), Color.Black);
            const int SIZE = 75;
            const int PADDING = 5;
            DrawRect(new Rectangle(Graphics.PreferredBackBufferWidth - SIZE - PADDING, PADDING + 18, SIZE, SIZE), Color.SaddleBrown, Color.White, 3);
        }

        private void DrawScene()
        {
            if(CurrentTexture != null)
                SpriteBatch.Draw(CurrentTexture, Vector2.Zero, Color.White);
        }

        private void FileSelected(object[] args)
        {
            DialogResult result = (DialogResult)args[0];
            string path = (string)args[1];

            Console.WriteLine(result);
            if (result != DialogResult.OK)
                return;

            bool exists = File.Exists(path);
            if (!exists)
            {
                Console.WriteLine("File does not exist!");
                return;
            }

            bool isFile = !Directory.Exists(path);
            if (!isFile)
            {
                Console.WriteLine("File is actually a directory! Not cool :(");
                return;
            }

            bool isPNG = Path.GetExtension(path) == ".png";
            if (!isPNG)
            {
                Console.WriteLine("File is not a .png file! It is a " + Path.GetExtension(path));
                return;
            }

            LoadAndSetCurrent(path);
        }

        public void LoadAndSetCurrent(string path)
        {
            var stream = new FileStream(path, FileMode.Open);
            var loaded = Texture2D.FromStream(GraphicsDevice, stream);
            stream.Close();
            stream.Dispose();
            stream = null;

            int width = loaded.Width;
            int height = loaded.Height;
            var format = loaded.Format;
            Console.WriteLine("Loaded image - Format: {0}, Size: {1}x{2}", format, width, height);

            bool cancel = false;

            if (format != SurfaceFormat.Color)
            {
                Console.WriteLine("Wrong loaded image format: {0}, expected Color", format);
                cancel = true;
            }
            else if (width != 170)
            {
                Console.WriteLine("Wrong loaded image width: {0}, expected 170", width);
                cancel = true;
            }
            else if (height != 42)
            {
                Console.WriteLine("Wrong loaded image height: {0}, expected 42", height);
                cancel = true;
            }

            if (cancel)
            {
                loaded.Dispose();
                return;
            }

            if (CurrentTexture != null)
            {
                CurrentTexture.Dispose();
            }
            CurrentTexture = loaded;
            CurrentFileName = Path.GetFileName(path);
            CurrentFilePath = path;
        }

        public void SaveCurrent()
        {
            if (string.IsNullOrWhiteSpace(CurrentFilePath))
                return;
            if (CurrentTexture == null)
                return;

            Stream s = new FileStream(Path.Combine(Path.GetDirectoryName(CurrentFilePath), "Test.png"), FileMode.Create);
            int width = CurrentTexture.Width;
            int height = CurrentTexture.Height;
            CurrentTexture.SaveAsPng(s, width, height);
            s.Close();
            s.Dispose();

            Console.WriteLine("Saved current image to {0}", CurrentFilePath);
        }

        public void ReloadCurrent()
        {
            if (string.IsNullOrWhiteSpace(CurrentFilePath))
                return;
            if (!File.Exists(CurrentFilePath))
            {
                Console.WriteLine("'{0}' does not exist, cannot reload current file.");
                return;
            }

            LoadAndSetCurrent(CurrentFilePath);
        }

        public static void DrawRect(Rectangle bounds, Color interior, Color exterior, int thickness)
        {
            if(thickness >= 1)
                SpriteBatch.Draw(Pixel, bounds, exterior);
            SpriteBatch.Draw(Pixel, new Rectangle(bounds.X + thickness, bounds.Y + thickness, bounds.Width - thickness * 2, bounds.Height - thickness * 2), interior);
        }

        public static bool DrawButton(SpriteFont font, Rectangle bounds, string text, bool allowHolding = false)
        {
            return DrawButton(font, bounds, text, Color.LightGray, Color.DarkGray, Color.Black, allowHolding);
        }

        public static bool DrawButton(SpriteFont font, Vector2 pos, string text, bool allowHolding = false)
        {
            return DrawButton(font, pos, text, Color.LightGray, Color.DarkGray, Color.Black, allowHolding);
        }

        public static bool DrawButton(SpriteFont font, Vector2 pos, string text, Color neutral, Color pressed, Color fontColor, bool allowHolding)
        {
            const int SIDE_PADDING = 6;
            const int VERTICAL_PADDING = 4;

            var size = font.MeasureString(text);
            size.X += SIDE_PADDING;
            size.Y += VERTICAL_PADDING;

            return DrawButton(font, new Rectangle(pos.ToPoint(), size.ToPoint()), text, neutral, pressed, fontColor, allowHolding);
        }

        public static bool DrawButton(SpriteFont font, Rectangle bounds, string text, Color neutral, Color pressed, Color fontColor, bool allowHolding)
        {
            Color EDGE_COLOR = Color.Black;
            const int EDGE_WIDTH = 2;

            bool over = bounds.Contains(CurrentMouseState.Position);
            bool mouseDown = CurrentMouseState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed;
            bool oldMouseDown = PrevousMouseState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed;

            bool isDown = over && (allowHolding ? (mouseDown) : (mouseDown && !oldMouseDown));

            DrawRect(bounds, isDown ? pressed : neutral, EDGE_COLOR, EDGE_WIDTH);

            var size = font.MeasureString(text);
            SpriteBatch.DrawString(font, text, bounds.Center.ToVector2() - size * 0.5f, Color.Black);

            return isDown;
        }
    }
}
