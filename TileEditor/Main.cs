using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace TileEditor
{
    public class Main : Game
    {
        public const string VERSION = "0.0.2";
        public static GraphicsDeviceManager Graphics;
        public static SpriteBatch SpriteBatch;
        public static Camera Camera;
        public static EventDispatcher Dispatcher;
        public static Texture2D ToolsTexture;
        public static Texture2D Pixel;
        public static Texture2D CurrentTexture;
        public static Color[] CurrentData;
        public static SpriteFont SmallFont;
        public static UIWindow ToolWindow;
        public static List<UIElement> UI = new List<UIElement>();
        public static Point MousePixelPos = new Point();
        public static bool TextureDirty { get; private set; }
        public static GameWindow GameWindow
        {
            get
            {
                return Instance.Window;
            }
        }
        public static FileSystemWatcher Watcher;

        private static Main Instance;
        private RenderTarget2D FrameBuffer;
        private static string CurrentFileName;
        private static string CurrentFilePath;
        private static List<SceneTilePart> sceneParts = new List<SceneTilePart>();
        private static bool editorMode = false;
        private static TilePart editorPlacingPart;
        private static string SavePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Temp Editor Save.txt");

        public Main()
        {
            Instance = this;
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

            ToolWindow = new UIWindow();
            ToolWindow.Title = "Tools";
            int w = 32 + ToolWindow.Padding * 2;
            int h = 100;
            ToolWindow.Bounds = new Rectangle(20, Graphics.PreferredBackBufferHeight / 2 - h / 2, w, h);
            UI.Add(ToolWindow);

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
            ToolsTexture = Content.Load<Texture2D>("Buttons/Tools");

            LoadSceneParts(File.ReadAllText(SavePath));
        }

        protected override void UnloadContent()
        {
            base.UnloadContent();

            if(CurrentTexture != null)
            {
                CurrentTexture.Dispose();
                CurrentTexture = null;
                CurrentData = null;
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
            Input.Update(gameTime);
            base.Update(gameTime);

            if(Window.ClientBounds.Width != Graphics.PreferredBackBufferWidth || Window.ClientBounds.Height != Graphics.PreferredBackBufferHeight)
            {
                Graphics.PreferredBackBufferWidth = Window.ClientBounds.Width;
                Graphics.PreferredBackBufferHeight = Window.ClientBounds.Height;
                Graphics.ApplyChanges();
                Console.WriteLine("Detected change in window vs backbuffer size, resizing backbuffer to {0}x{1}", Window.ClientBounds.Width, Window.ClientBounds.Height);
            }

            if(Graphics.PreferredBackBufferWidth != FrameBuffer.Width || Graphics.PreferredBackBufferHeight != FrameBuffer.Height)
            {
                ResizeBuffer(Graphics.PreferredBackBufferWidth, Graphics.PreferredBackBufferHeight);
            }

            Dispatcher.DispatchEvents();

            if (Input.KeyPressed(Microsoft.Xna.Framework.Input.Keys.LeftShift) && Input.KeyDown(Microsoft.Xna.Framework.Input.Keys.F1))
            {
                editorMode = !editorMode;
            }

            var scenePos = Camera.ScreenToWorldPosition(Input.MouseScreenPosition);
            MousePixelPos.X = (int)(scenePos.X);
            MousePixelPos.Y = (int)(scenePos.Y);

            CameraControls.Update();
            UpdateUIElements();
        }

        public static Color GetPixel(int x, int y)
        {
            if (CurrentTexture == null)
                return Color.HotPink;

            if (x < 0 || x >= CurrentTexture.Width || y < 0 || y >= CurrentTexture.Height)
                return Color.HotPink;

            return CurrentData[x + y * CurrentTexture.Width];
        }

        public static void SetPixel(int x, int y, Color c)
        {
            if (CurrentTexture == null)
                return;

            int index = x + y * CurrentTexture.Width;
            if(CurrentData[index] != c)
            {
                TextureDirty = true;
                CurrentData[index] = c;
                TextureDirty = true;
            }          
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.SetRenderTarget(FrameBuffer);
            GraphicsDevice.Clear(Color.LightSlateGray);

            Camera.UpdateMatrix(GraphicsDevice);
            SpriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, Camera.GetMatrix());
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

            //bool savePressed = DrawButton(SmallFont, new Vector2(0, 24), "Save...");
            //if (savePressed)
            //{
            //    SaveCurrent();
            //}

            // Name of current file.
            SpriteBatch.DrawString(SmallFont, "Editing: " + (CurrentFileName ?? "None"), new Vector2(65, 0), Color.Black);

            // Current color and label.
            float width = SmallFont.MeasureString("Active Color").X;
            SpriteBatch.DrawString(SmallFont, "Active Color", new Vector2(Graphics.PreferredBackBufferWidth - width - 3, 2), Color.Black);
            const int SIZE = 75;
            const int PADDING = 5;
            DrawRect(new Rectangle(Graphics.PreferredBackBufferWidth - SIZE - PADDING, PADDING + 18, SIZE, SIZE), GetScenePixel(MousePixelPos.X, MousePixelPos.Y), Color.White, 3);

            DrawUIElements();

            // Tool window.
            ToolWindow.DrawButton(ToolsTexture, new Rectangle(0, 0, 32, 32), false);
            ToolWindow.DrawButton(ToolsTexture, new Rectangle(32, 0, 32, 32), false);

            if (editorMode)
            {
                if (Input.KeyDown(Microsoft.Xna.Framework.Input.Keys.Left))
                {
                    int current = (int)editorPlacingPart;
                    if (current > 0)
                        current--;
                    editorPlacingPart = (TilePart)current;
                }
                else if (Input.KeyDown(Microsoft.Xna.Framework.Input.Keys.Right))
                {
                    int current = (int)editorPlacingPart;
                    if (current < 20)
                        current++;
                    editorPlacingPart = (TilePart)current;
                }

                SpriteBatch.DrawString(SmallFont, "EDITOR MODE", new Vector2(200, 5), Color.Red);
                SpriteBatch.DrawString(SmallFont, "Placing: " + editorPlacingPart.ToString(), new Vector2(200, 20), Color.Black);
                SpriteBatch.DrawString(SmallFont, "Mouse: " + MousePixelPos, new Vector2(200, 35), Color.Black);
            }
        }

        private void DrawScene()
        {
            if(CurrentTexture != null)
            {
                //SpriteBatch.Draw(CurrentTexture, Vector2.Zero, Color.White);

                if (TextureDirty)
                {
                    CurrentTexture.SetData(CurrentData);
                    TextureDirty = false;
                }

                foreach (var item in sceneParts)
                {
                    DrawTilePart(item.TextureBounds, new Vector2(item.SceneBounds.X, item.SceneBounds.Y));
                }
            }            

            if (editorMode)
            {
                foreach (var item in sceneParts)
                {
                    Color placedColor = Color.IndianRed;
                    placedColor.A = 100;
                    SpriteBatch.Draw(Pixel, item.SceneBounds, placedColor);
                }

                Rectangle texBounds = editorPlacingPart.GetPartBounds();
                Rectangle sceneB = new Rectangle(MousePixelPos.X, MousePixelPos.Y, texBounds.Width, texBounds.Height);
                Color activeColor = Color.LightGoldenrodYellow;
                activeColor.A = 100;
                SpriteBatch.Draw(Pixel, sceneB, activeColor);

                if (Input.LeftMouseDown())
                {
                    sceneParts.Add(new SceneTilePart() { Part = editorPlacingPart, SceneBounds = sceneB, TextureBounds = texBounds });
                }
                else if (Input.RightMouseDown())
                {
                    Rectangle sB = new Rectangle(), tB = new Rectangle();
                    bool anythingThere = GetTextureBoundsForScenePixel(MousePixelPos.X, MousePixelPos.Y, ref sB, ref tB);
                    if (anythingThere)
                    {
                        for (int i = 0; i < sceneParts.Count; i++)
                        {
                            var thing = sceneParts[i];
                            if(thing.SceneBounds == sB)
                            {
                                sceneParts.RemoveAt(i);
                                break;
                            }
                        }
                    }
                }

                if (Input.KeyDown(Microsoft.Xna.Framework.Input.Keys.S))
                {
                    // Save to file...
                    StringBuilder s = new StringBuilder();
                    bool f = true;
                    foreach (var item in sceneParts)
                    {
                        if(!f)
                            s.AppendLine();
                        f = false;
                        s.Append((int)item.Part);

                        s.AppendLine();
                        s.Append(item.SceneBounds.X);
                        s.Append(", ");
                        s.Append(item.SceneBounds.Y);
                        s.Append(", ");
                        s.Append(item.SceneBounds.Width);
                        s.Append(", ");
                        s.Append(item.SceneBounds.Height);
                    }
                    File.WriteAllText(SavePath, s.ToString());
                }
            }
        }

        private void LoadSceneParts(string data)
        {
            string[] lines = data.Split('\n');
            for (int i = 0; i < lines.Length / 2; i++)
            {
                int partID = int.Parse(lines[i * 2]);
                string rectData = lines[i * 2 + 1];
                string[] rectParts = rectData.Split(',');
                int x = int.Parse(rectParts[0]);
                int y = int.Parse(rectParts[1]);
                int w = int.Parse(rectParts[2]);
                int h = int.Parse(rectParts[3]);

                TilePart part = (TilePart)partID;
                Rectangle textureBounds = part.GetPartBounds();
                Rectangle sceneBounds = new Rectangle(x, y, w, h);

                sceneParts.Add(new SceneTilePart() { Part = part, SceneBounds = sceneBounds, TextureBounds = textureBounds });
            }
        }

        private static Color GetScenePixel(int x, int y)
        {
            Rectangle sceneBounds = new Rectangle();
            Rectangle textureBounds = new Rectangle();
            bool found = GetTextureBoundsForScenePixel(x, y, ref sceneBounds, ref textureBounds);

            if (!found)
                return Color.HotPink;

            int adjustedX = x - sceneBounds.X;
            int adjustedY = y - sceneBounds.Y;

            Color color = GetPixel(textureBounds.X + adjustedX, textureBounds.Y + adjustedY);
            return color;
        }

        private static bool GetTextureBoundsForScenePixel(int x, int y, ref Rectangle sceneBounds, ref Rectangle textureBounds)
        {
            int count = sceneParts.Count;
            for (int i = count - 1; i >= 0; i--)
            {
                var item = sceneParts[i];

                if (item.SceneBounds.Contains(x, y))
                {
                    sceneBounds = item.SceneBounds;
                    textureBounds = item.TextureBounds;
                    return true;
                }
            }

            sceneBounds = new Rectangle();
            textureBounds = new Rectangle();
            return false;
        }

        private void DrawTilePart(Rectangle part, Vector2 position)
        {
            if (CurrentTexture == null)
                return;

            var textureBounds = part;
            var drawnPos = new Rectangle((int)position.X, (int)position.Y, textureBounds.Width, textureBounds.Height);
            SpriteBatch.Draw(CurrentTexture, drawnPos, textureBounds, Color.White);
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

        public void LoadAndSetCurrent(string path, bool fromFilePathChange = false)
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
            CurrentData = new Color[loaded.Width * loaded.Height];
            loaded.GetData(CurrentData);
            CurrentFileName = Path.GetFileName(path);
            CurrentFilePath = path;

            if (!fromFilePathChange)
            {
                string dir = new FileInfo(path).Directory.FullName;
                Watcher = new FileSystemWatcher(dir, "*.*");
                Watcher.NotifyFilter = NotifyFilters.LastWrite;
                Watcher.Changed += Watcher_Changed;
                Watcher.EnableRaisingEvents = true;

                Console.WriteLine("Created watcher for " + dir);
            }
        }

        private void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            if(e.ChangeType == WatcherChangeTypes.Changed && e.FullPath == CurrentFilePath)
            {
                // Reload the image...
                ReloadCurrent(true);
            }
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

        public void ReloadCurrent(bool fromFilePathChange = false)
        {
            if (string.IsNullOrWhiteSpace(CurrentFilePath))
                return;
            if (!File.Exists(CurrentFilePath))
            {
                Console.WriteLine("'{0}' does not exist, cannot reload current file.");
                return;
            }

            LoadAndSetCurrent(CurrentFilePath, fromFilePathChange);
        }

        private void UpdateUIElements()
        {
            foreach (var item in UI)
            {
                if(item != null)
                    item.Update();
            }
        }

        private void DrawUIElements()
        {
            foreach (var item in UI)
            {
                if (item != null)
                    item.Draw();
            }
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

            bool over = bounds.Contains(Input.MouseScreenPosition);

            // TODO: Make the holding mode require for the mouse to be pressed within first.
            bool isDown = over && (allowHolding ? (Input.LeftMousePressed()) : Input.LeftMouseDown()) && FocusTracker.CurrentlyTracked == null;

            DrawRect(bounds, isDown ? pressed : neutral, EDGE_COLOR, EDGE_WIDTH);

            var size = font.MeasureString(text);
            SpriteBatch.DrawString(font, text, bounds.Center.ToVector2() - size * 0.5f, Color.Black);

            return isDown;
        }

        public static bool DrawButton(Texture2D texture, Rectangle textureBounds, Rectangle bounds, Color pressed, bool allowHolding)
        {
            bool over = bounds.Contains(Input.MouseScreenPosition);

            bool isDown = over && (allowHolding ? Input.LeftMousePressed() : Input.LeftMouseDown()) && FocusTracker.CurrentlyTracked == null;

            SpriteBatch.Draw(texture, bounds, textureBounds, isDown ? pressed : Color.White);

            return isDown;
        }
    }
}
