using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace LFE.Core.Utils
{
    public static class ImageHelper
    {
        public static Bitmap Url2Bitmap(this string url)
        {
            var request =System.Net.WebRequest.Create(url);
            var response = request.GetResponse();
            var responseStream = response.GetResponseStream();
            return responseStream == null ? null : new Bitmap(responseStream);
        }

        public static Bitmap LimitBitmapSize(Bitmap inputBmp, int maxWidth, int maxHeight)
        {
            if ((inputBmp.Width <= maxWidth) && (inputBmp.Height <= maxHeight))
            {
                // Within the size limits
                return inputBmp.Clone(new Rectangle(0, 0, inputBmp.Width, inputBmp.Height), inputBmp.PixelFormat);
            }

            var w = maxWidth;
            var h = maxHeight;
            if (inputBmp.Width * maxHeight > inputBmp.Height * maxWidth)
            {
                // More Wide
                //h = Convert.ToInt32(maxWidth * inputBmp.Height / inputBmp.Width);
                w = Convert.ToInt32(maxHeight * inputBmp.Width / inputBmp.Height);
            }
            else
            {
                //w = Convert.ToInt32(maxHeight * inputBmp.Width / inputBmp.Height);
                h = Convert.ToInt32(maxWidth * inputBmp.Height / inputBmp.Width);
            }

            var newBmp = new Bitmap(w, h);
            using (var g = Graphics.FromImage(newBmp)) g.DrawImage(inputBmp, 0, 0, w, h);
            return CropImage(newBmp, maxWidth, maxHeight);
        }

        public static Bitmap CropImage(Bitmap inputBmp, int width, int height)
        {
            if (inputBmp.Width == width && inputBmp.Height == height) return inputBmp;

            var marginSide = 0;
            var marginTop = 0;

            if (inputBmp.Width > inputBmp.Height)
            {
                marginSide = (inputBmp.Width - width) / 2;
            }
            else
            {
                marginTop = (inputBmp.Height - height) / 2;
            }

            var croppedBitmap = inputBmp.Clone(
            new Rectangle(marginSide, marginTop,
                inputBmp.Width - marginSide - marginSide,
                inputBmp.Height - marginTop - marginTop),
                System.Drawing.Imaging.PixelFormat.DontCare);

            return croppedBitmap;

        }

        public static Bitmap FitBitmapKeepAspectRatio(Bitmap inputBmp, int newWidth, int newHeight)
        {
            Rectangle rec;
            if (inputBmp.Width * newHeight > inputBmp.Height * newWidth)
            {
                // More Wide
                var w = Convert.ToInt32(inputBmp.Height * newWidth / newHeight);
                rec = new Rectangle(Convert.ToInt32((inputBmp.Width - w) / 2), 0, w, inputBmp.Height);
            }
            else
            {
                var h = Convert.ToInt32(inputBmp.Width * newHeight / newWidth);
                var y = Convert.ToInt32((inputBmp.Height - h) / 2);
                rec = new Rectangle(0, y, inputBmp.Width, h);

            }

            var bmpCrop = inputBmp.Clone(rec, inputBmp.PixelFormat);
            var newBmp = new Bitmap(newWidth, newHeight);
            using (var g = Graphics.FromImage(newBmp)) g.DrawImage(bmpCrop, 0, 0, newWidth, newHeight);
            return newBmp;
        }

        public static ImageFormat GetContentType(byte[] imageBytes)
        {
            var ms = new MemoryStream(imageBytes);

            using (var br = new BinaryReader(ms))
            {
                var maxMagicBytesLength = imageFormatDecoders.Keys.OrderByDescending(x => x.Length).First().Length;

                var magicBytes = new byte[maxMagicBytesLength];

                for (var i = 0; i < maxMagicBytesLength; i += 1)
                {
                    magicBytes[i] = br.ReadByte();

                    foreach (var kvPair in imageFormatDecoders)
                    {
                        if (magicBytes.StartsWith(kvPair.Key))
                        {
                            return kvPair.Value;
                        }
                    }
                }

                return null;
            }
        }

        private static bool StartsWith(this byte[] thisBytes, byte[] thatBytes)
        {
            for (var i = 0; i < thatBytes.Length; i += 1)
            {
                if (thisBytes[i] != thatBytes[i])
                {
                    return false;
                }
            }
            return true;
        }

        private static readonly Dictionary<byte[], ImageFormat> imageFormatDecoders = new Dictionary<byte[], ImageFormat>
        {
            { new byte[]{ 0x42, 0x4D }, ImageFormat.Bmp},
            { new byte[]{ 0x47, 0x49, 0x46, 0x38, 0x37, 0x61 }, ImageFormat.Gif },
            { new byte[]{ 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 }, ImageFormat.Gif },
            { new byte[]{ 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }, ImageFormat.Png },
            { new byte[]{ 0xff, 0xd8 }, ImageFormat.Jpeg },
        };
    }
}
