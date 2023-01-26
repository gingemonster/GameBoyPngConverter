using System;
using System.CommandLine;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GameBoyPngConverter
{
    class Program
    {
        private const string TileDataTemplateFileName = "tiledatatemplate.c";
        private const string TileMapTemplateFileName = "tilemaptemplate.c";
        private const int TilePixelSize = 8;
        private const string TokenNumberTiles = "NUMBER_TILES";
        private const string TokenTileDataName = "TILE_DATA_NAME";
        private const string TokenTileData = "TILE_DATA";
        private const string TokenTileMapSize = "TILE_MAP_SIZE";
        private const string TokenTileMapName = "TILE_MAP_NAME";
        private const string TokenTileMap = "TILE_MAP";
        private static bool automated = false;
        private static string filePath = string.Empty;
        private static int offsetFirstTile = 0;
        private static bool bwMode;

        static async Task Main(string[] args)
        {
            var rootCommand = new RootCommand("GameBoy png converter for GBDK.");

            var filePathArgument = new Argument<string>(
                name: "filePath",
                description: "Path to a .png file."
             );
            rootCommand.AddArgument(filePathArgument);

            var automatedOption = new Option<bool>(
                "--automated",
                () => false,
                "Automated generation process.");
            automatedOption.AddAlias("-a");
            rootCommand.AddOption(automatedOption);

            var offsetOption = new Option<int>(
                "--offset",
                () => 0,
                "Set offset of first tile to generate map file.");
            offsetOption.AddAlias("-o");
            rootCommand.AddOption(offsetOption);

            var bwOption = new Option<bool>(
                "--blackwhite",
                () => false,
                "Generate palette of only two color: transparent and the darkest.");
            bwOption.AddAlias("-bw");
            rootCommand.AddOption(bwOption);

            rootCommand.SetHandler((fileValue, automatedOptionValue, offsetOptionValue, bwOptionValue) =>
            {
                automated = automatedOptionValue;

                filePath = fileValue;
                offsetFirstTile = offsetOptionValue;
                bwMode = bwOptionValue;
            }, filePathArgument, automatedOption, offsetOption, bwOption);

            var commandResult = await rootCommand.InvokeAsync(args);

            if (commandResult == 1)
                return;

            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }

            if (!File.Exists(filePath))
            {
                ConsoleStatus.Errored();
                Console.WriteLine("File doesn't exists.");
                Console.WriteLine("Press any key to exit...");
                if (!automated)
                    Console.Read();
                return;
            }

            using (var pngStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            using (var image = new Bitmap(pngStream))
            {
                var fileName = MakeSafeFileName(Path.GetFileNameWithoutExtension(args[0]));


                if (image.Width % TilePixelSize != 0 || image.Height % TilePixelSize != 0)
                {
                    ConsoleStatus.Errored();
                    Console.WriteLine("The height and width of your image must be a multiple of 8 pixels, please fix and try again");
                    Console.WriteLine("Press any key to exit...");
                    if (!automated)
                        Console.Read();
                    return;
                }

                var colorpalette = ExtractColorPalette(image);
                if (colorpalette.Count > 4)
                {
                    ConsoleStatus.Errored();
                    Console.WriteLine("There are more than 4 colors in your image, please fix and try again");
                    Console.WriteLine("Press any key to exit...");
                    if (!automated)
                        Console.Read();
                    return;
                }

                OrderPaletteByBrigtness(colorpalette);


                var uniquesprites = new List<Sprite>();
                var dedupedsprites = new List<Sprite>();

                GenerateSprites(image, colorpalette, uniquesprites, dedupedsprites);

                if (dedupedsprites.Count / TilePixelSize * 2 > 256)
                {
                    ConsoleStatus.Warning();
                    Console.WriteLine("- you have more than 256 tiles making it very difficult to display them all on the gameboy at the same time, try an image that could have more repeated tiles");
                }

                var datastring = GenerateDataFile(dedupedsprites, fileName);
                WriteFile(Path.GetDirectoryName(args[0]), fileName + "_data.c", datastring);

                var mapstring = GenerateMapFile(uniquesprites, dedupedsprites, fileName, image);
                WriteFile(Path.GetDirectoryName(args[0]), fileName + "_map.c", mapstring);
                if (!automated)
                {
                    ConsoleStatus.Completed();
                    Console.WriteLine("Press any key to exit...");
                    Console.Read();
                }
                else
                {
                    ConsoleStatus.Completed();
                    Console.WriteLine();
                }
            }
        }

        private static string MakeSafeFileName(string fileName)
        {
            var chars = Path.GetInvalidFileNameChars();
            foreach (char c in chars)
            {
                fileName = fileName.Replace(c, '_');
            }
            fileName = fileName.Replace(' ', '_');
            return fileName;
        }

        private static void WriteFile(string filepath, string fileName, string filecontent)
        {
            try
            {
                File.WriteAllText(Path.Combine(filepath, fileName), filecontent);
            }
            catch
            {
                Console.WriteLine($"could not write to file '{Path.Combine(filepath, fileName)}' check it is not read only or open in another application");
            }
        }

        private static void GenerateSprites(Bitmap image, List<Color> colorPalette, List<Sprite> uniqueSprites, List<Sprite> dedupedSprites)
        {
            // loop each 8x8 sprite converting colors to hex values
            var widthInSprites = image.Width / 8;
            var heightInSprites = image.Height / 8;

            for (var spriteRow = 1; spriteRow <= heightInSprites; spriteRow++)
            {
                for (var spriteCol = 1; spriteCol <= widthInSprites; spriteCol++)
                {
                    var newSprite = new Sprite();

                    // go row by row in this sprite
                    for (var y = spriteRow * 8 - 8; y < spriteRow * 8; y++)
                    {
                        var rowBitsOne = new List<bool>();
                        var rowBitsTwo = new List<bool>();

                        // loop each column along this row creating two bytes per row
                        for (var x = spriteCol * 8 - 8; x < spriteCol * 8; x++)
                        {
                            var pixelcolor = image.GetPixel(x, y);
                            if (pixelcolor == colorPalette[0])
                            {
                                // darkest color
                                rowBitsOne.Add(true);
                                rowBitsTwo.Add(true);
                            }
                            else if (pixelcolor == colorPalette[1])
                            {

                                rowBitsOne.Add(false);
                                rowBitsTwo.Add(true);
                            }
                            else if (pixelcolor == colorPalette[2])
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


                        newSprite.Bytes.Add(ConvertToByte(new BitArray(rowBitsOne.ToArray())));
                        newSprite.Bytes.Add(ConvertToByte(new BitArray(rowBitsTwo.ToArray())));
                    }

                    // add to unique sprites
                    uniqueSprites.Add(newSprite);

                    // check if is a duplicate and if not add to de-duped sprites
                    if (!dedupedSprites.Exists(s => s.HashCode == newSprite.HashCode))
                    {
                        dedupedSprites.Add(newSprite);
                    }
                }
            }
        }

        private static string GenerateDataFile(List<Sprite> sprites, string fileName)
        {
            // use c file template
            var template = ReadTemplateFile(TileDataTemplateFileName);
            ReplaceToken(ref template, TokenNumberTiles, sprites.Count.ToString());
            ReplaceToken(ref template, TokenTileDataName, fileName + "_data");

            var allbytes = new List<Byte>();
            sprites.ForEach(s => allbytes.AddRange(s.Bytes));

            ReplaceToken(ref template, TokenTileData, ByteArrayToString(allbytes.ToArray()));

            return template;
        }

        private static string GenerateMapFile(List<Sprite> uniqueSprites, List<Sprite> dedupedSprites, string fileName, Bitmap image)
        {
            var hex = new StringBuilder();
            var i = 0;
            var mapTileWidth = (image.Width / TilePixelSize);
            uniqueSprites.ForEach(us =>
            {
                // check position of this unique sprite in dedupedSprites
                var index = dedupedSprites.FindIndex(ds => ds.HashCode == us.HashCode) + offsetFirstTile;
                hex.AppendFormat("0x{0:X2}", index);
                if (i < uniqueSprites.Count - 1)
                {
                    hex.Append(",");
                    if ((i + 1) % mapTileWidth == 0)
                    {
                        hex.AppendLine();
                        hex.Append("\t");
                    }
                }
                i++;
            });

            var template = ReadTemplateFile(TileMapTemplateFileName);
            ReplaceToken(ref template, TokenTileMapSize, mapTileWidth.ToString() + " x " + (image.Height / TilePixelSize).ToString());
            ReplaceToken(ref template, TokenTileMapName, fileName + "_map");
            ReplaceToken(ref template, TokenTileMap, hex.ToString());

            return template;
        }

        private static string ReadTemplateFile(string fileName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "GameBoyPngConverter." + fileName;

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        private static void ReplaceToken(ref string template, string tokenName, string value)
        {
            template = template.Replace("[[" + tokenName + "]]", value);
        }

        private static string ByteArrayToString(byte[] ba)
        {
            var hex = new StringBuilder(ba.Length * 2);

            for (var i = 0; i < ba.Length; i++)
            {
                hex.AppendFormat("0x{0:X2}", ba[i]);
                if (i < ba.Length - 1)
                {
                    hex.Append(",");
                    if ((i + 1) % 16 == 0)
                    {
                        hex.AppendLine();
                        hex.Append("\t");
                    }
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

            for (var x = 0; x < image.Width; x++)
            {
                for (var y = 0; y < image.Height; y++)
                {
                    var pixelColor = image.GetPixel(x, y);
                    if (!colors.Contains(pixelColor))
                    {
                        colors.Add(pixelColor);
                        if (colors.Count > 4)
                        {
                            return colors;
                        }
                    }
                }
            }

            return colors;
        }

        private static void OrderPaletteByBrigtness(List<Color> palette)
        {
            palette.Sort((a, b) => a.GetBrightness().CompareTo(b.GetBrightness()));
            if (bwMode && palette.Count == 2)
            {
                palette.Insert(1, Color.Transparent);
                palette.Insert(2, Color.Transparent);
            }
            else if (bwMode && palette.Count == 3)
            {
                palette.Insert(2, Color.Transparent);
            }
        }
    }
}
