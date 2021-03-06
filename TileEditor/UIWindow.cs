﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TileEditor
{
    public class UIWindow : UIElement
    {
        public string Title = "Window";
        public Rectangle Bounds = new Rectangle(30, 30, 70, 130);
        public int Padding = 5;
        public int InternalPadding = 4;
        public bool Dragging { get; private set; }

        private int currentY;
        private Point dragOffset;

        public bool DrawButton(SpriteFont font, string text, bool allowHolding = false)
        {
            int dy = (int)Math.Ceiling(font.MeasureString(text).Y);

            Rectangle bounds = new Rectangle(Bounds.X + Padding, Bounds.Y + currentY, Bounds.Width - Padding * 2, dy + 2);

            currentY += dy + InternalPadding;

            return Main.DrawButton(font, bounds, text, allowHolding);
        }

        public bool DrawButton(Texture2D texture, Rectangle textureBounds, bool allowHolding = false)
        {
            int dy = textureBounds.Height;
            Rectangle bounds = new Rectangle(Bounds.X + Padding, Bounds.Y + currentY, textureBounds.Width, textureBounds.Height);

            currentY += dy + InternalPadding;

            return Main.DrawButton(texture, textureBounds, bounds, Color.SlateGray, allowHolding);
        }

        public void Update()
        {
            var size = Main.SmallFont.MeasureString(Title);
            int width = (int)size.X;
            int height = (int)size.Y;            

            Rectangle dragPart = new Rectangle(Bounds.X, Bounds.Y, Bounds.Width, Padding * 2 + height);
            bool mouseOver = dragPart.Contains(Input.CurrentMouseState.Position);

            if (!Dragging && mouseOver && Input.LeftMouseDown())
            {
                // Clicked inside the window for the first time to drag...
                FocusTracker.CurrentlyTracked = this;
                Dragging = true;
                dragOffset = new Point(Bounds.X - (int)Input.MouseScreenPosition.X, Bounds.Y - (int)Input.MouseScreenPosition.Y);
            }            
        }

        public void Draw()
        {
            var size = Main.SmallFont.MeasureString(Title);
            int width = (int)size.X;
            int height = (int)size.Y;

            currentY = height + Padding * 2;            

            if (Dragging)
            {                
                if(FocusTracker.CurrentlyTracked != this)
                {
                    Dragging = false;
                }
                else if(!Input.LeftMousePressed())
                {
                    Dragging = false;
                    FocusTracker.CurrentlyTracked = null;
                }
            }

            if (Dragging)
            {
                Bounds.X = (int)Input.MouseScreenPosition.X + dragOffset.X;
                Bounds.Y = (int)Input.MouseScreenPosition.Y + dragOffset.Y;
            }

            Main.DrawRect(new Rectangle(Bounds.X, Bounds.Y, Bounds.Width, Bounds.Height), Color.Wheat, Color.Black, 2);
            Main.SpriteBatch.DrawString(Main.SmallFont, Title, new Vector2(Bounds.X + Bounds.Width / 2f - width / 2f, Bounds.Y + Padding), Color.Black);
        }
    }
}
