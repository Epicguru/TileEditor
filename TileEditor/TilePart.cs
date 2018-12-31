using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TileEditor
{
    public enum TilePart
    {
        FULL,
        LEFT,
        TOP,
        RIGHT,
        BOTTOM,
        BOTTOM_LEFT,
        TOP_LEFT,
        TOP_RIGHT,
        BOTTOM_RIGHT,
        BOTTOM_LEFT_I,
        TOP_LEFT_I,
        TOP_RIGHT_I,
        BOTTOM_RIGHT_I,
        CORNER_TOP_LEFT,
        CORNER_TOP_RIGHT,
        CORNER_BOTTOM_LEFT,
        CORNER_BOTTOM_RIGHT
    }

    public static class TilePartUtils
    {
        public static Rectangle GetPartBounds(this TilePart part)
        {
            // Coordinate origin is the top-left of the tile texture.
            const int EDGE = 5;
            const int FULL = 32;

            switch (part)
            {
                case TilePart.FULL:
                    return new Rectangle(EDGE, EDGE, FULL, FULL);
                case TilePart.LEFT:
                    return new Rectangle(0, EDGE, EDGE, FULL);
                case TilePart.TOP:
                    return new Rectangle(EDGE, 0, FULL, EDGE);
                case TilePart.RIGHT:
                    return new Rectangle(EDGE + FULL, EDGE, EDGE, FULL);
                case TilePart.BOTTOM:
                    return new Rectangle(EDGE, EDGE + FULL, FULL, EDGE);
                case TilePart.BOTTOM_LEFT:
                    return new Rectangle(0, EDGE + FULL, EDGE, EDGE);
                case TilePart.TOP_LEFT:
                    return new Rectangle(0, 0, EDGE, EDGE);
                case TilePart.TOP_RIGHT:
                    return new Rectangle(EDGE + FULL, 0, EDGE, EDGE);
                case TilePart.BOTTOM_RIGHT:
                    return new Rectangle(EDGE + FULL, EDGE + FULL, EDGE, EDGE);
                case TilePart.BOTTOM_LEFT_I:
                    return new Rectangle(FULL + EDGE * 2, EDGE, EDGE, EDGE);
                case TilePart.TOP_LEFT_I:
                    return new Rectangle(EDGE * 2 + FULL, 0, EDGE, EDGE);
                case TilePart.TOP_RIGHT_I:
                    return new Rectangle(EDGE * 3 + FULL, 0, EDGE, EDGE);
                case TilePart.BOTTOM_RIGHT_I:
                    return new Rectangle(EDGE * 3 + FULL, EDGE, EDGE, EDGE);
                case TilePart.CORNER_TOP_LEFT:
                    return new Rectangle(EDGE * 2 + FULL, EDGE * 2, FULL, FULL);
                case TilePart.CORNER_TOP_RIGHT:
                    return new Rectangle(EDGE * 2 + FULL * 2, EDGE * 2, FULL, FULL);
                case TilePart.CORNER_BOTTOM_LEFT:
                    return new Rectangle(EDGE * 2 + FULL * 3, EDGE * 2, FULL, FULL);
                case TilePart.CORNER_BOTTOM_RIGHT:
                    return new Rectangle(EDGE * 2 + FULL * 4, EDGE * 2, FULL, FULL);
                default:
                    return new Rectangle();
            }
        }
    }
}
