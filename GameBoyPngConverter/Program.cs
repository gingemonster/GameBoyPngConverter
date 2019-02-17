using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace GameBoyPngConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var pngStream = new FileStream(args[0], FileMode.Open, FileAccess.Read))
            using (var image = new Bitmap(pngStream))
            {
                var colorpalette = ExtractColorPalette(image);
                if(image.Width % 8 != 0 || image.Height % 8 != 0)
                {
                    Console.WriteLine("The height and width of your image must be a multiple of 8 pixels, please fix and try again");
                    return;
                }

                if(colorpalette.Count > 4)
                {
                    Console.WriteLine("There are more than 4 colors in your image, please fix and try again");
                    return;
                }

                OrderPaletteByBrigtness(colorpalette);

                // loop each 8x8 sprite converting colors to hex values
                var widthinsprites = image.Width / 8;
                var heightinsprites = image.Height / 8;

                var spriteBytes = new List<Byte>();

                for(var spriterow = 1; spriterow <= heightinsprites; spriterow++)
                {
                    for (var spritecol = 1; spritecol <= widthinsprites; spritecol++)
                    {
                        // go row by row in this sprite
                        for (var y = (spriterow * 8 - 8); y < spriterow * 8; y++)
                        {
                            var rowBitsOne = new List<bool>();
                            var rowBitsTwo = new List<bool>();
                            for (var x = (spritecol * 8 - 8); x < spritecol * 8; x++)
                            {
                                var pixelcolor = image.GetPixel(x, y);
                                if(pixelcolor == colorpalette[0])
                                {
                                    // darkest color
                                    rowBitsOne.Add(true);
                                    rowBitsTwo.Add(true);
                                }
                                else if (pixelcolor == colorpalette[1])
                                {

                                    rowBitsOne.Add(false);
                                    rowBitsTwo.Add(true);
                                }
                                else if (pixelcolor == colorpalette[2])
                                {
                                    rowBitsOne.Add(true);
                                    rowBitsTwo.Add(false);
                                }
                                else
                                {
                                    // lightest color
                                    rowBitsOne.Add(false);
                                    rowBitsTwo.Add(false);
                                }
                            }
                            // revese bits before converting to bytes
                            rowBitsOne.Reverse();
                            rowBitsTwo.Reverse();
                            // add rows bits to byte array for entire image
                            spriteBytes.Add(ConvertToByte(new BitArray(rowBitsOne.ToArray())));
                            spriteBytes.Add(ConvertToByte(new BitArray(rowBitsTwo.ToArray())));
                        }
                    }
                }
                var result = ByteArrayToString(spriteBytes.ToArray());
                var map = new StringBuilder();
                for(var i = 0; i < 20*18; i++)
                {
                    map.AppendFormat("0x{0:X2}, ", i);
                }
                var mapstring = map.ToString();

                if(spriteBytes.Count/16 > 256)
                {
                    Console.WriteLine("Warning you have more than 256 tiles making it very difficault to display them all on the gameboy at the same time, try an image that could have more repeated tiles");
                }
            }
        }

        private static string ByteArrayToString(byte[] ba)
        {
            // TODO nice linebreaks every 16
            // TODO remove last ,
            var hex = new StringBuilder(ba.Length * 2);

            for(var i = 0; i < ba.Length; i++)
            {
                hex.AppendFormat("0x{0:X2}", ba[i]);
                if (i < ba.Length - 1)
                {
                    hex.Append(",");
                }
                if((i+1) % 16 == 0)
                {
                    hex.AppendLine();
                }
            }
                
            return hex.ToString();
        }

        private static byte ConvertToByte(BitArray bits)
        {
            if (bits.Count != 8)
            {
                throw new ArgumentException("bits");
            }
            byte[] bytes = new byte[1];
            bits.CopyTo(bytes, 0);
            return bytes[0];
        }

        private static List<Color> ExtractColorPalette(Bitmap image)
        {
            List<Color> colors = new List<Color>();

            for(var x = 0; x < image.Width; x++)
            {
                for(var y=0; y < image.Height; y++)
                {
                    var pixelcolor = image.GetPixel(x, y);
                    if (!colors.Contains(pixelcolor))
                    {
                        colors.Add(pixelcolor);
                    }
                }
            }

            return colors;
        }

        private static void OrderPaletteByBrigtness(List<Color> palette)
        {
            palette.Sort((a, b) => a.GetBrightness().CompareTo(b.GetBrightness()));
        }
    }
}
