using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Reflection;

namespace CnCMapper
{
    static class MiscExt
    {
        public static int clip(this int val, int min, int max)
        {
            if (val > max) return max;
            if (val < min) return min;
            return val;
        }

        public static int SnapDown(this int value, int multiple)
        {
            //Round down value to nearest multiple of multiple.
            if (value < 0) value -= multiple - 1;
            return (value / multiple) * multiple;
        }

        public static int SnapUp(this int value, int multiple)
        {
            //Round up value to nearest multiple of multiple.
            if (value >= 0) value += multiple - 1;
            return (value / multiple) * multiple;
        }

        public static Rectangle Snap(this Rectangle r, int multipleX, int multipleY)
        {
            return Rectangle.FromLTRB(r.X.SnapDown(multipleX), r.Y.SnapDown(multipleY),
                r.Right.SnapUp(multipleX), r.Bottom.SnapUp(multipleY));
        }

        public static Point getOffset(this Point p, int dx, int dy)
        {
            p.Offset(dx, dy);
            return p;
        }

        public static Point getOffset(this Point p, Point point)
        {
            return p.getOffset(point.X, point.Y);
        }

        public static Rectangle getInflate(this Rectangle r, Size size)
        {
            return r.getInflate(size.Width, size.Height);
        }

        public static Rectangle getInflate(this Rectangle r, int dw, int dh)
        {
            return Rectangle.Inflate(r, dw, dh);
        }

        public static Rectangle getIntersect(this Rectangle r, Rectangle rectangle)
        {
            return Rectangle.Intersect(r, rectangle);
        }

        public static Rectangle getOffset(this Rectangle r, Point point)
        {
            return r.getOffset(point.X, point.Y);
        }

        public static Rectangle getOffset(this Rectangle r, int dx, int dy)
        {
            r.Offset(dx, dy);
            return r;
        }

        public static Point scaleUp(this Point p, int scale)
        {
            p.X *= scale;
            p.Y *= scale;
            return p;
        }

        public static Point scaleDown(this Point p, int scale)
        {
            p.X /= scale;
            p.Y /= scale;
            return p;
        }

        public static Rectangle scaleUp(this Rectangle r, int scale)
        {
            r.X *= scale;
            r.Y *= scale;
            r.Width *= scale;
            r.Height *= scale;
            return r;
        }

        public static Rectangle scaleDown(this Rectangle r, int scale)
        {
            r.X /= scale;
            r.Y /= scale;
            r.Width /= scale;
            r.Height /= scale;
            return r;
        }

        public static string Replace(this string s, int index, string value)
        {
            //Replace chars in string at index with chars in value.
            int endIndex = index + value.Length;
            string start = s.Substring(0, index);
            string end = s.Substring(endIndex, s.Length - endIndex);
            return start + value + end;
        }

        public static string ReplaceEnd(this string s, string value)
        {
            //Replace chars in string at end with chars in value.
            return s.Substring(0, s.Length - value.Length) + value;
        }

        public static string GetFullPathWithEndingSlashes(this string s)
        {
            //Ensure that path string ends with a directory separator.
            return Path.GetFullPath(s).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        }

        public static void AppendLine(this StringBuilder sb, string format, object arg0)
        {
            sb.AppendFormat(format, arg0);
            sb.AppendLine();
        }

        public static void AppendLine(this StringBuilder sb, string format, object arg0, object arg1)
        {
            sb.AppendFormat(format, arg0, arg1);
            sb.AppendLine();
        }

        public static void AppendLine(this StringBuilder sb, string format, object arg0, object arg1, object arg2)
        {
            sb.AppendFormat(format, arg0, arg1, arg2);
            sb.AppendLine();
        }

        public static void AppendLine(this StringBuilder sb, string format, params object[] args)
        {
            sb.AppendFormat(format, args);
            sb.AppendLine();
        }

        public static string ToStringItems<T>(this IEnumerable<T> e)
        {
            //Convert collection items to a string. Mostly to debug print array content.
            StringBuilder sb = new StringBuilder();
            foreach (T item in e)
            {
                sb.Append(item.ToString());
                sb.Append(',');
            }
            return '[' + sb.ToString().TrimEnd(',') + ']';
        }

        public static void AddNotNull<T>(this List<T> l, T item) where T : class
        {
            if (item != null)
            {
                l.Add(item);
            }
        }

        public static List<TOut> ConvertAs<TIn, TOut>(this List<TIn> l)
            where TIn : class
            where TOut : class
        {
            //return l.OfType<TOut>().ToList(); //Also works but a little bit slower.

            List<TOut> listOut = new List<TOut>();
            listOut.AddRangeAs(l);
            return listOut;
        }

        public static void AddRangeAs<TIn, TOut>(this List<TOut> l, IEnumerable<TIn> collection)
            where TIn : class
            where TOut : class
        {
            foreach (TIn itemIn in collection)
            {
                l.AddNotNull(itemIn as TOut);
            }
        }

        public static void AddDerivedRange<TIn, TOut>(this List<TOut> l, IEnumerable<TIn> collection)
            where TIn : TOut
            where TOut : class
        {
            //Add collection where TIn is a class derived from TOut.
            foreach (TIn itemIn in collection)
            {
                l.Add(itemIn);
            }
        }

        //List.Sort() is not stable i.e. original order of equal items is not guaranteed.
        public static void SortStable<T>(this IList<T> list, Comparison<T> comparison)
        {
            //Use a stable sorting method like insertion sort.
            //IEnumerable.OrderBy() is stable, but seems to be a bit slower than this version.
            InsertionSort(list, comparison);
        }

        private static void InsertionSort<T>(IList<T> list, Comparison<T> comparison)
        {
            //https://www.csharp411.com/c-stable-sort/
            if (list == null) throw new ArgumentNullException("list");
            if (comparison == null) throw new ArgumentNullException("comparison");

            int count = list.Count;
            for (int i = 1; i < count; i++)
            {
                T key = list[i];
                int j = i - 1;
                for (; j >= 0 && comparison(list[j], key) > 0; j--)
                {
                    list[j + 1] = list[j];
                }
                list[j + 1] = key;
            }
        }

        public static void Add<TKey, TValue>(this Dictionary<TKey, TValue> d, KeyValuePair<TKey, TValue> key)
        {
            d.Add(key.Key, key.Value);
        }

        public static IEnumerable<T> Reversed<T>(this IEnumerable<T> collection)
        {
            //IEnumerable<T>.Reverse clashes with List<T>.Reverse and you often have to specify type of T.
            //Lets rename it to avoid that i.e. you can use Reversed() instead of Reverse<T>().
            return collection.Reverse<T>();
        }

        //Type.GetProperties() returns properties in bottom to top (reverse) order i.e. properties
        //in inherited class are returned before properties in base class. This alternative method
        //returns properties in a top to bottom (declared) order i.e. base classes first.
        public static PropertyInfo[] GetPropertiesBaseFirst(this Type type)
        {
            return GetPropertiesBaseFirst(type, BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);
        }

        public static PropertyInfo[] GetPropertiesBaseFirst(this Type type, BindingFlags flags)
        {
            List<PropertyInfo> properties = new List<PropertyInfo>();
            do
            {
                //Insert properties at start.
                properties.InsertRange(0, type.GetProperties(flags));
                //Do parent next.
                type = type.BaseType;
            } while (type != null); //No more parent classes?
            return properties.ToArray();
        }

        public static Rectangle getRect(this byte[] bytes, int stride)
        {
            //Return byte[] expressed as a rectangle. Useful when checking for out of bound accesses.
            return new Rectangle(0, 0, stride, bytes.Length / stride);
        }

        public static void clear<T>(this T[] srcArray, T value)
        {
            clear(srcArray, value, 0, srcArray.Length);
        }

        public static void clear<T>(this T[] srcArray, T value, int startIndex, int count)
        {
            int endIndex = startIndex + count;
            for (int i = startIndex; i < endIndex; i++)
            {
                srcArray[i] = value;
            }
        }

        public static void clearBytes(this byte[] dstBytes, byte value)
        {
            for (int i = 0; i < dstBytes.Length; i++)
            {
                dstBytes[i] = value;
            }
        }

        public static T[] takeValues<T>(this T[] srcArray) //Copy value array.
            where T : struct
        {
            T[] take = new T[srcArray.Length];
            Array.Copy(srcArray, take, take.Length);
            return take;

            //A bit faster than doing:
            //return srcArray.ToArray();
        }

        public static byte[] takeBytes(this byte[] srcBytes) //Copy byte array.
        {
            return takeBytes(srcBytes, 0, srcBytes.Length);
        }

        public static byte[] takeBytes(this byte[] srcBytes, int srcIndex, int length) //Copy byte array.
        {
            if (srcBytes.Length < (srcIndex + length))
            {
                throw new ArgumentException(string.Format("Array is too short to take '{0}' bytes starting from '{1}'!", length, srcIndex));
            }
            byte[] take = new byte[length];
            Buffer.BlockCopy(srcBytes, srcIndex, take, 0, take.Length);
            return take;
        }

        public static void drawRect(this byte[] dstPixels, int dstStride, Rectangle dstClip, Rectangle dstRect, byte value)
        {
            //Draws a rectangle with a 1 pixel wide line.
            if (dstRect.Width * dstRect.Height < 1)
            {
                throw new ArgumentException("Rectangle size is smaller than 1!");
            }
            if (dstPixels.Length % dstStride != 0)
            {
                throw new ArgumentException("Destination stride is not a multiple of destination array length!");
            }
            if (!getRect(dstPixels, dstStride).Contains(dstClip))
            {
                throw new ArgumentException("Destination clip is outside destination array!");
            }

            drawRectInner(dstPixels, dstStride, dstClip, dstRect, value, true, true, true, true);
        }

        public static void drawRectCorners(this byte[] dstPixels, int dstStride, Rectangle dstClip, Rectangle dstRect, int cornerLength, byte value)
        {
            drawRectCorners(dstPixels, dstStride, dstClip, dstRect, cornerLength, value, true, true, true, true);
        }

        public static void drawRectCorners(this byte[] dstPixels, int dstStride, Rectangle dstClip, Rectangle dstRect, int cornerLength, byte value,
            bool doTop, bool doBot, bool doLef, bool doRig)
        {
            //Draws corners of a rectangle (center of sides are open) with a 1 pixel wide line.
            //Flags "doTop", "doBot", "doLef" and "doRig" controls if that side of the rectangle should be drawn.
            //Useful when drawing many rectangles and you don't want to draw sides adjacent to other rectangles.
            if (dstRect.Width * dstRect.Height < 1)
            {
                throw new ArgumentException("Rectangle size is smaller than 1!");
            }
            if (cornerLength > dstRect.Width / 2 && cornerLength > dstRect.Height / 2)
            {
                throw new ArgumentException("Corner length is longer than half side of rectangle!");
            }
            if (dstPixels.Length % dstStride != 0)
            {
                throw new ArgumentException("Destination stride is not a multiple of destination array length!");
            }
            if (!getRect(dstPixels, dstStride).Contains(dstClip))
            {
                throw new ArgumentException("Destination clip is outside destination array!");
            }

            Rectangle rc = new Rectangle(dstRect.X, dstRect.Y, cornerLength, cornerLength);
            drawRectInner(dstPixels, dstStride, dstClip, rc, value, doTop, false, doLef, false); //Top left corner.
            rc.X = dstRect.Right - cornerLength;
            drawRectInner(dstPixels, dstStride, dstClip, rc, value, doTop, false, false, doRig); //Top right corner.

            rc.X = dstRect.X;
            rc.Y = dstRect.Bottom - cornerLength;
            drawRectInner(dstPixels, dstStride, dstClip, rc, value, false, doBot, doLef, false); //Bottom left corner.
            rc.X = dstRect.Right - cornerLength;
            drawRectInner(dstPixels, dstStride, dstClip, rc, value, false, doBot, false, doRig); //Bottom right corner.
        }

        private static void drawRectInner(this byte[] dstPixels, int dstStride, Rectangle dstClip, Rectangle dstRect, byte value,
            bool doTop, bool doBot, bool doLef, bool doRig)
        {
            Rectangle rc = dstRect;
            rc.Height = 1;
            if (doTop) //Top horizontal line.
            {
                drawRectFilledInner(dstPixels, dstStride, dstClip, rc, value);
            }
            rc.Y = dstRect.Bottom - 1;
            if (doBot) //Bottom horizontal line.
            {
                drawRectFilledInner(dstPixels, dstStride, dstClip, rc, value);
            }
            rc = dstRect;
            rc.Width = 1;
            if (doLef) //Left vertical line.
            {
                drawRectFilledInner(dstPixels, dstStride, dstClip, rc, value);
            }
            rc.X = dstRect.Right - 1;
            if (doRig) //Right vertical line.
            {
                drawRectFilledInner(dstPixels, dstStride, dstClip, rc, value);
            }
        }

        public static void drawRectFilled(this byte[] dstPixels, int dstStride, Rectangle dstClip, Rectangle dstRect, byte value)
        {
            //Draws a filled rectangle. Drawn pixels are clipped inside destination.
            if (dstRect.Width * dstRect.Height < 1)
            {
                throw new ArgumentException("Rectangle size is smaller than 1!");
            }
            if (dstPixels.Length % dstStride != 0)
            {
                throw new ArgumentException("Destination stride is not a multiple of destination array length!");
            }
            if (!getRect(dstPixels, dstStride).Contains(dstClip))
            {
                throw new ArgumentException("Destination clip is outside destination array!");
            }

            drawRectFilledInner(dstPixels, dstStride, dstClip, dstRect, value);
        }

        private static void drawRectFilledInner(this byte[] dstPixels, int dstStride, Rectangle dstClip, Rectangle dstRect, byte value)
        {
            dstRect.Intersect(dstClip); //Clip draw rectangle.
            int k = dstRect.X + (dstRect.Y * dstStride);
            for (int y = 0; y < dstRect.Height; y++, k += dstStride)
            {
                for (int x = 0; x < dstRect.Width; x++)
                {
                    dstPixels[k + x] = value;
                }
            }
        }

        public static void drawPixels(this byte[] srcPixels, int srcStride, Rectangle srcRect,
            byte[] dstPixels, int dstStride, Rectangle dstClip, Point dstPos)
        {
            drawPixels(srcPixels, srcStride, srcRect, dstPixels, dstStride, dstClip, dstPos, null, null);
        }

        public static void drawPixels(this byte[] srcPixels, int srcStride, Rectangle srcRect,
            byte[] dstPixels, int dstStride, Rectangle dstClip, Point dstPos, byte[] remap)
        {
            drawPixels(srcPixels, srcStride, srcRect, dstPixels, dstStride, dstClip, dstPos, remap, null);
        }

        public static void drawPixels(this byte[] srcPixels, int srcStride, Rectangle srcRect,
            byte[] dstPixels, int dstStride, Rectangle dstClip, Point dstPos, byte[] remap, byte[] filter)
        {
            //Draws part of "srcPixels" specified by "srcRect" at "dstPos" in "dstPixels".
            //Source and destination arrays are 8-bit indexed pixels.
            //Source pixels with a 0 index aren't drawn (transparent).
            //Drawn pixels are clipped inside "dstClip".
            //Optionally (if not null) uses "remap"-table to remap pixel indices (re-coloring).
            //Optionally (if not null) uses "filter"-table to remap existing pixel indices (shadow effect).
            if (srcPixels.Length % srcStride != 0)
            {
                throw new ArgumentException("Source stride is not a multiple of source array length!");
            }
            if (dstPixels.Length % dstStride != 0)
            {
                throw new ArgumentException("Destination stride is not a multiple of destination array length!");
            }
            if (!getRect(srcPixels, srcStride).Contains(srcRect))
            {
                throw new ArgumentException("Source rectangle is outside source array!");
            }
            if (!getRect(dstPixels, dstStride).Contains(dstClip))
            {
                throw new ArgumentException("Destination clip is outside destination array!");
            }

            //Calculate actual source rectangle and draw position after clipping.
            Rectangle drawRect = new Rectangle(dstPos, srcRect.Size);
            drawRect.Intersect(dstClip); //Clip draw rectangle.
            //Adjust source rectangle and destination with any changes clipping made to draw rectangle.
            srcRect.Offset(drawRect.X - dstPos.X, drawRect.Y - dstPos.Y);
            srcRect.Size = drawRect.Size;
            dstPos = drawRect.Location;

            //Draw part of source specified by "srcRect" at "dstPos" in destination.
            int srcInd = srcRect.X + (srcRect.Y * srcStride);
            int dstInd = dstPos.X + (dstPos.Y * dstStride);
            for (int y = 0; y < srcRect.Height; y++, srcInd += srcStride, dstInd += dstStride)
            {
                for (int x = 0; x < srcRect.Width; x++)
                {
                    byte b = srcPixels[srcInd + x];
                    if (remap != null) //Remap index?
                    {
                        b = remap[b];
                    }
                    if (b != 0) //Not a transparent pixel?
                    {
                        if (filter != null) //Filter existing index?
                        {
                            b = filter[dstPixels[dstInd + x]];
                        }
                        dstPixels[dstInd + x] = b;
                    }
                }
            }
        }

        public static void copyBytes(this byte[] srcBytes, int srcStride, byte[] dstBytes, int dstStride, Point dstPos)
        {
            copyBytes(srcBytes, srcStride, new Point(0, 0), dstBytes, dstStride, dstPos, new Size(srcStride, srcBytes.Length / srcStride));
        }

        public static void copyBytes(this byte[] srcBytes, int srcStride, Rectangle srcRect, byte[] dstBytes, int dstStride, Point dstPos)
        {
            copyBytes(srcBytes, srcStride, srcRect.Location, dstBytes, dstStride, dstPos, srcRect.Size);
        }

        private static void copyBytes(this byte[] srcBytes, int srcStride, Point srcPos, byte[] dstBytes, int dstStride, Point dstPos, Size size)
        {
            //Size = size of region to copy.
            if (size.Width * size.Height < 1)
            {
                throw new ArgumentException("Size of region to copy is smaller than 1!");
            }
            if (srcBytes.Length % srcStride != 0)
            {
                throw new ArgumentException("Source stride is not a multiple of source array length!");
            }
            if (dstBytes.Length % dstStride != 0)
            {
                throw new ArgumentException("Destination stride is not a multiple of destination array length!");
            }
            if (!getRect(srcBytes, srcStride).Contains(new Rectangle(srcPos, size)))
            {
                throw new ArgumentException("Source rectangle is outside source array!");
            }
            if (!getRect(dstBytes, dstStride).Contains(new Rectangle(dstPos, size)))
            {
                throw new ArgumentException("Destination rectangle is outside destination array!");
            }

            int srcInd = srcPos.X + (srcPos.Y * srcStride);
            int dstInd = dstPos.X + (dstPos.Y * dstStride);
            for (int row = 0; row < size.Height; row++, srcInd += srcStride, dstInd += dstStride)
            {
                //Buffer.BlockCopy seems a bit faster on byte[] than Array.Copy.
                Buffer.BlockCopy(srcBytes, srcInd, dstBytes, dstInd, size.Width);
            }
        }

        public static void savePixels(this byte[] srcPixels, int srcStride, ColorPalette palette, string filePath)
        {
            Rectangle rect = getRect(srcPixels, srcStride);
            savePixels(srcPixels, srcStride, rect, palette, filePath);
        }

        public static void savePixels(this byte[] srcPixels, int srcStride, Rectangle srcRect, ColorPalette palette, string filePath)
        {
            //Saves source pixels as an 8-bit indexed bitmap.
            if (!getRect(srcPixels, srcStride).Contains(srcRect))
            {
                throw new ArgumentException("Source rectangle is outside source array!");
            }

            using (Bitmap bmp = new Bitmap(srcRect.Width, srcRect.Height, PixelFormat.Format8bppIndexed))
            {
                if (palette != null) //Use custom palette instead of default?
                {
                    bmp.Palette = palette;
                }
                BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, bmp.PixelFormat);
                int dstStride = bmpData.Stride;
                int srcInd = srcRect.X + (srcRect.Y * srcStride);
                long dstInd = bmpData.Scan0.ToInt64();
                if (srcRect.X == 0 && srcStride == srcRect.Width && srcStride == dstStride) //Can copy all pixels at once?
                {
                    Marshal.Copy(srcPixels, srcInd, (IntPtr)dstInd, srcRect.Width * srcRect.Height);
                }
                else //Copy line-by-line.
                {
                    for (int row = 0; row < srcRect.Height; row++, srcInd += srcStride, dstInd += dstStride)
                    {
                        Marshal.Copy(srcPixels, srcInd, (IntPtr)dstInd, srcRect.Width);
                    }
                }
                bmp.UnlockBits(bmpData);
                bmp.Save(filePath);
            }
        }

        public static byte[] getPixels(this Bitmap bmp)
        {
            //Returns bitmap's pixels as a byte array.
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, bmp.PixelFormat);
            int bitsPerPixel = Bitmap.GetPixelFormatSize(bmpData.PixelFormat);
            int bmpStride = bmpData.Stride;
            int arrStride = (bmpData.Width * bitsPerPixel + 7) / 8; //Round up.
            byte[] pixels = new byte[arrStride * bmpData.Height];
            long bmpInd = bmpData.Scan0.ToInt64();
            int arrInd = 0;
            if (bmpStride == arrStride) //Can read all pixels at once if same stride.
            {
                Marshal.Copy((IntPtr)bmpInd, pixels, arrInd, pixels.Length);
            }
            else //Read per line if different stride.
            {
                for (int y = 0; y < bmpData.Height; y++, bmpInd += bmpStride, arrInd += arrStride)
                {
                    Marshal.Copy((IntPtr)bmpInd, pixels, arrInd, arrStride);
                }
            }
            bmp.UnlockBits(bmpData);
            return pixels;
        }
    }
}
