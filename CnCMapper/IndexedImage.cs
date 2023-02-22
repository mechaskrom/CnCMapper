using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace CnCMapper
{
    class IndexedImage : Frame //Palette indexed 8-bit image. Frame with a clipping hint.
    {
        private Rectangle mClip; //Don't have to draw outside this area. Used to optimize some drawing.

        public IndexedImage(int width, int height)
            : base(width, height)
        {
            mClip = new Rectangle(0, 0, width, height);
        }

        public Rectangle Clip
        {
            get { return mClip; }
            set { mClip = value.getIntersect(new Rectangle(0, 0, Width, Height)); }
        }

        protected override Rectangle getClip()
        {
            return Clip;
        }

        public void drawOutsideShade(Rectangle borders, byte[] shadeRemap) //Shade area outside borders.
        {
            Rectangle clipRect = Clip;
            borders.Intersect(clipRect); //Make sure borders are inside clip.
            //Shade the four regions between the clip rectangle and the borders.
            Rectangle topRect = new Rectangle(clipRect.X, clipRect.Y, clipRect.Width, borders.Y - clipRect.Y);
            drawOutsideShadeRect(topRect, shadeRemap);
            Rectangle leftRect = new Rectangle(clipRect.X, borders.Y, borders.X - clipRect.X, borders.Height);
            drawOutsideShadeRect(leftRect, shadeRemap);
            Rectangle rightRect = new Rectangle(borders.Right, borders.Y, clipRect.Right - borders.Right, borders.Height);
            drawOutsideShadeRect(rightRect, shadeRemap);
            Rectangle bottomRect = new Rectangle(clipRect.X, borders.Bottom, clipRect.Width, clipRect.Bottom - borders.Bottom);
            drawOutsideShadeRect(bottomRect, shadeRemap);
        }

        private void drawOutsideShadeRect(Rectangle shadeRect, byte[] remapShade)
        {
            drawOutsideShadeRect(shadeRect, remapShade, Stride, Pixels);
        }

        private static void drawOutsideShadeRect(Rectangle shadeRect, byte[] remapShade, int stride, byte[] pixels)
        {
            for (int y = 0, k = shadeRect.Y * stride; y < shadeRect.Height; y++, k += stride)
            {
                for (int x = 0, i = k + shadeRect.X; x < shadeRect.Width; x++, i++)
                {
                    pixels[i] = remapShade[pixels[i]];
                }
            }
        }

        public IndexedImage getImageTrimmed(Rectangle trimRect)
        {
            //If saved image should have a margin the clip area is inflated. The inflated area could be
            //outside the image e.g. clip area is near the edges and/or a large margin is used.
            //Meaning of clip and trim rectangle in this method:
            //-Clip rectangle = Inflated with the margin and intersected inside image.
            //-Trim rectangle = Inflated with the margin. Same as clip if inside image.
            //This method will return a new image with a larger size if needed i.e. trim is outside image.

            System.Diagnostics.Debug.Assert(trimRect.getIntersect(new Rectangle(0, 0, Width, Height)) == Clip,
                "Trim rectangle intersected inside image should be equal to clip rectangle!");

            if (trimRect != Clip) //Trim is outside image?
            {
                //Image with new size.
                IndexedImage imageNew = new IndexedImage(trimRect.Width, trimRect.Height);

                //If trim position is outside image use it as an offset to center image.
                Point pos = new Point(Math.Max(0, -trimRect.X), Math.Max(0, -trimRect.Y));
                imageNew.write(this, Clip, pos); //Copy pixels to the new image.
                return imageNew;
            }
            return this; //Trim is inside image so no resize needed.
        }
    }
}
