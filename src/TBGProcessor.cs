using weiduize.Model;
using System.Collections.Generic;
using System.IO;

namespace weiduize
{
    public class TBGProcessor
    {
        public List<(byte[] file, string filename, List<(int offset, string text)> strings, IEGame game)> Process(List<(byte[] bytes, string filename, IEGame game)> filesToProcess)
        {
            var files = new List<(byte[] file, string filename, List<(int offset, string text)> strings, IEGame game)>();
            foreach (var file in filesToProcess)
            {
                if (file.filename.ToUpper().EndsWith("TBG"))
                {
                    var processedTBG = false;
                    if (file.bytes[0] == 'T' && file.bytes[1] == 'B' && file.bytes[2] == 'G' && file.bytes[3] == '4' && !processedTBG)
                    {
                        using var ms = new MemoryStream(file.bytes);
                        using var br = new BinaryReader(ms);
                        var result = HandleTBG4(br, file.game);
                        if (result.file != null)
                        {
                            files.Add(result);
                            processedTBG = true;
                        }
                    }

                    if (file.bytes[0] == 'T' && file.bytes[1] == 'B' && file.bytes[2] == 'G' && file.bytes[3] == '3' && !processedTBG)
                    {
                        using var ms = new MemoryStream(file.bytes);
                        using var br = new BinaryReader(ms);
                        var result = HandleTBG3(br, file.game);
                        if (result.file != null)
                        {
                            files.Add(result);
                            processedTBG = true;
                        }
                    }

                    if (!processedTBG)
                    {
                        using var ms = new MemoryStream(file.bytes);
                        using var br = new BinaryReader(ms);
                        var result = HandleTBG2(br, file.game);
                        if (result.file != null)
                        {
                            files.Add(result);
                            processedTBG = true;
                        }
                    }

                    if (!processedTBG)
                    {
                        // If all else fails, maybe it's a TBG1?
                        using var ms = new MemoryStream(file.bytes);
                        using var br = new BinaryReader(ms);
                        var result = HandleTBG1(br);
                        if (result.file != null)
                        {
                            // TBG1 doesn't provide a filename, so we'll assume the output filename matches the input filename
                            // TBG1 only supports ITM files so we'll rename any output files to that extension
                            result.filename = Path.ChangeExtension(Path.GetFileName(file.filename), ".itm");
                            files.Add(result);
                            processedTBG = true;
                        }
                    }
                }
                else
                {
                    files.Add((file.bytes, file.filename, new List<(int offset, string text)>(), IEGame.Unknown));
                }
            }
            return files;
        }

        private static (byte[] file, string filename, List<(int offset, string text)> strings, IEGame game) HandleTBG1(BinaryReader br)
        {
            const int TBG1_HeaderSize = 52;
            if (br.BaseStream.Length > TBG1_HeaderSize)
            {
                br.BaseStream.Seek(0, SeekOrigin.Begin);
                var fileOffset = br.ReadInt32();
                var fileLength = br.ReadInt32();
                var unidentifiedNameOffset = br.ReadInt32();
                var unidentifiedNameLength = br.ReadInt32();
                var identifiedNameOffset = br.ReadInt32();
                var identifiedNameLength = br.ReadInt32();
                var unidentifiedDescriptionOffset = br.ReadInt32();
                var unidentifiedDescriptionLength = br.ReadInt32();
                var identifiedDescriptionOffset = br.ReadInt32();
                var identifiedDescriptionLength = br.ReadInt32();

                // The actual file
                br.BaseStream.Seek(fileOffset, SeekOrigin.Begin);
                var file = br.ReadBytes(fileLength);

                br.BaseStream.Seek(unidentifiedNameOffset, SeekOrigin.Begin);
                var unidentifiedName = string.Join("", br.ReadChars(unidentifiedNameLength));

                br.BaseStream.Seek(identifiedNameOffset, SeekOrigin.Begin);
                var identifiedName = string.Join("", br.ReadChars(identifiedNameLength));

                br.BaseStream.Seek(unidentifiedDescriptionOffset, SeekOrigin.Begin);
                var unidentifiedDescription = string.Join("", br.ReadChars(unidentifiedDescriptionLength));

                br.BaseStream.Seek(identifiedDescriptionOffset, SeekOrigin.Begin);
                var identifiedDescription = string.Join("", br.ReadChars(identifiedDescriptionLength));

                // TBG1 only supports ITM files, so the offsets for text are hardcoded
                var strings = new List<(int offset, string text)>
                {
                    (8, unidentifiedName),
                    (12, identifiedName),
                    (80, unidentifiedDescription),
                    (84, identifiedDescription)
                };

                //TBG1 only supports Baldur's Gate I
                return (file, string.Empty, strings, IEGame.BaldursGateI);
            }

            return (null, string.Empty, new List<(int offset, string text)>(), IEGame.BaldursGateI);
        }

        private static (byte[] file, string filename, List<(int offset, string text)> strings, IEGame game) HandleTBG2(BinaryReader br, IEGame game)
        {
            const int TBG2_HeaderSize = 53;
            if (br.BaseStream.Length > TBG2_HeaderSize)
            {
                // TBG2 files don't have a file signature, so we'll check if there's a supported output filetype in the expected position
                br.BaseStream.Seek(40, SeekOrigin.Begin);
                var filename = string.Join("", br.ReadChars(12)).Replace("\0", "");
                if (filename.ToUpper().EndsWith("ITM") || filename.ToUpper().EndsWith("SPL"))
                {
                    // This looks to be a TBG2 file
                    br.BaseStream.Seek(0, SeekOrigin.Begin);

                    var fileOffset = br.ReadInt32();
                    var fileLength = br.ReadInt32();
                    var unidentifiedNameOffset = br.ReadInt32();
                    var unidentifiedNameLength = br.ReadInt32();
                    var identifiedNameOffset = br.ReadInt32();
                    var identifiedNameLength = br.ReadInt32();
                    var unidentifiedDescriptionOffset = br.ReadInt32();
                    var unidentifiedDescriptionLength = br.ReadInt32();
                    var identifiedDescriptionOffset = br.ReadInt32();
                    var identifiedDescriptionLength = br.ReadInt32();
                    filename = string.Join("", br.ReadChars(12)).Replace("\0", "");
                    var tbgGame = br.ReadByte();

                    // The actual file
                    br.BaseStream.Seek(fileOffset, SeekOrigin.Begin);
                    var file = br.ReadBytes(fileLength);

                    br.BaseStream.Seek(unidentifiedNameOffset, SeekOrigin.Begin);
                    var unidentifiedName = string.Join("", br.ReadChars(unidentifiedNameLength));

                    br.BaseStream.Seek(identifiedNameOffset, SeekOrigin.Begin);
                    var identifiedName = string.Join("", br.ReadChars(identifiedNameLength));

                    br.BaseStream.Seek(unidentifiedDescriptionOffset, SeekOrigin.Begin);
                    var unidentifiedDescription = string.Join("", br.ReadChars(unidentifiedDescriptionLength));

                    br.BaseStream.Seek(identifiedDescriptionOffset, SeekOrigin.Begin);
                    var identifiedDescription = string.Join("", br.ReadChars(identifiedDescriptionLength));

                    // TBG1 only supports ITM and SPL files, so the offsets for text are hardcoded (and thankfully the same for both filetypes)
                    var strings = new List<(int offset, string text)>
                    {
                        (8, unidentifiedName),
                        (12, identifiedName),
                        (80, unidentifiedDescription),
                        (84, identifiedDescription)
                    };

                    return (file, filename, strings, GamePreference(game, (IEGame)tbgGame));
                }
            }

            return (null, string.Empty, new List<(int offset, string text)>(), IEGame.Unknown);
        }

        private static (byte[] file, string filename, List<(int offset, string text)> strings, IEGame game) HandleTBG3(BinaryReader br, IEGame game)
        {
            var signature = br.ReadChars(4);
            var unused = br.ReadInt32();
            var filename = string.Join("", br.ReadChars(12)).Replace("\0", "");
            var fileOffset = br.ReadInt32();
            var fileLength = br.ReadInt32();
            var tbgGame = br.ReadInt32();
            var textOffset = br.ReadInt32();
            var textHeaderCount = br.ReadInt32();
            var textHeaderOffset = br.ReadInt32();

            // The actual file
            br.BaseStream.Seek(fileOffset, SeekOrigin.Begin);
            var file = br.ReadBytes(fileLength);

            // Text header
            var textHeaders = new List<TBG3_TextHeader>();
            const int TextHeaderSize = 20;
            for (int i = 0; i < textHeaderCount; i++)
            {
                br.BaseStream.Seek(textHeaderOffset + (i * TextHeaderSize), SeekOrigin.Begin);
                // read textHeaderCount
                var offset = br.ReadInt32();
                var textLength = br.ReadInt32();
                var strrefWriteLocation = br.ReadInt32();
                var soundFilename = br.ReadChars(8);

                var textHeader = new TBG3_TextHeader();
                textHeader.Offset = offset;
                textHeader.Length = textLength;
                textHeader.StrrefWriteLocation = strrefWriteLocation;
                textHeader.SoundFilename = soundFilename;
                textHeaders.Add(textHeader);
            }

            // The actual text
            var texts = new List<string>();
            for (int i = 0; i < textHeaderCount; i++)
            {
                br.BaseStream.Seek(textOffset + textHeaders[i].Offset, SeekOrigin.Begin);
                var text = br.ReadChars(textHeaders[i].Length);
                texts.Add(string.Join("", text));
            }

            var strings = new List<(int offset, string text)>();
            for (int i = 0; i < textHeaders.Count; i++)
            {
                strings.Add((textHeaders[i].StrrefWriteLocation, texts[i]));
            }

            return (file, filename, strings, GamePreference(game, (IEGame)tbgGame));
        }

        private static (byte[] file, string filename, List<(int offset, string text)> strings, IEGame game) HandleTBG4(BinaryReader br, IEGame game)
        {
            var signature = br.ReadChars(4);
            var unused = br.ReadInt32();
            var filename = string.Join("", br.ReadChars(12)).Replace("\0", "");
            var fileOffset = br.ReadInt32();
            var fileLength = br.ReadInt32();
            var tbgGame = br.ReadInt32();
            var textOffset = br.ReadInt32();
            var textHeaderCount = br.ReadInt32();
            var textHeaderOffset = br.ReadInt32();
            var strrefCount = br.ReadInt32();
            var strrefOffset = br.ReadInt32();
            var asciiCount = br.ReadInt32();
            var asciiOffset = br.ReadInt32();

            // The actual file
            br.BaseStream.Seek(fileOffset, SeekOrigin.Begin);
            var file = br.ReadBytes(fileLength);

            // Text header
            var textHeaders = new List<TBG4_TextHeader>();
            const int TextHeaderSize = 50;
            for (int i = 0; i < textHeaderCount; i++)
            {
                br.BaseStream.Seek(textHeaderOffset + (i * TextHeaderSize), SeekOrigin.Begin);
                // read textHeaderCount
                var offset = br.ReadInt32();
                var textLength = br.ReadInt32();
                var soundFilename = br.ReadChars(8);
                var volumeVariance = br.ReadInt32();
                var pitchVariance = br.ReadInt32();
                var dialogEntryType = br.ReadInt16();
                var strrefForThisTextHeaderOffset = br.ReadInt32(); // which strref entry to use (might not be in order)
                var strrefForThisTextHeaderCount = br.ReadInt32();
                var aac = br.ReadInt32();
                var aad = br.ReadInt32();
                var aae = br.ReadInt32();
                var aaf = br.ReadInt32();
                var textHeader = new TBG4_TextHeader();
                textHeader.Offset = offset;
                textHeader.Length = textLength;
                textHeader.SoundFilename = soundFilename;
                textHeader.VolumeVariance = volumeVariance;
                textHeader.PitchVariance = pitchVariance;
                textHeader.EntryType = dialogEntryType;
                textHeader.StrrefOffset = strrefForThisTextHeaderOffset;
                textHeader.StrrefCount = strrefForThisTextHeaderCount;
                textHeader.Offset2 = aac;
                textHeader.Count2 = aad;
                textHeader.Offset3 = aae;
                textHeader.Count3 = aaf;
                textHeaders.Add(textHeader);
            }

            // The actual text
            var texts = new List<string>();
            for (int i = 0; i < textHeaderCount; i++)
            {
                br.BaseStream.Seek(textOffset + textHeaders[i].Offset, SeekOrigin.Begin);
                var text = br.ReadChars(textHeaders[i].Length);
                texts.Add(string.Join("", text));
            }

            // Text header (offsets in the resultant file to write a strref entry)
            var strrefs = new List<int>();
            const int StrrefSize = 4;
            for (int i = 0; i < strrefCount; i++)
            {
                br.BaseStream.Seek(strrefOffset + (i * StrrefSize), SeekOrigin.Begin);
                var offset = br.ReadInt32();
                strrefs.Add(offset);
            }

            // Ascii
            var asciis = new List<int>();
            const int asciiSize = 4;
            for (int i = 0; i < asciiCount; i++)
            {
                br.BaseStream.Seek(asciiOffset + (i * asciiSize), SeekOrigin.Begin);
                var offset = br.ReadInt32();
                asciis.Add(offset);
            }

            var strings = new List<(int offset, string text)>();
            for (int i = 0; i < textHeaders.Count; i++)
            {
                strings.Add((strrefs[textHeaders[i].StrrefOffset], texts[i]));
            }

            return (file, filename, strings, GamePreference(game, (IEGame)tbgGame));
        }

        private static IEGame GamePreference(IEGame gameFromIAP, IEGame gameFromTBG)
        {
            return gameFromIAP != IEGame.Unknown ? gameFromIAP : gameFromTBG;
        }
    }
}