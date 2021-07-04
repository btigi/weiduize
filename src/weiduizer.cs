using System;
using System.IO;
using System.Linq;
using System.Text;
using weiduize.Model;

namespace weiduize
{
    public class weiduizer
    {
        public void Process((string modName, string author) modData, string inputFilename)
        {
            var outputFilenameTp2 = $"setup-{modData.modName}.tp2";

            var bytes = File.ReadAllBytes(inputFilename);
            if (bytes.Length < 4)
            {
                return;
            }

            var iapProcessor = new IAPProcessor();
            var filesToProcess = iapProcessor.Process(bytes);
            if (filesToProcess.Count == 0)
            {
                filesToProcess.Add((bytes, inputFilename, IEGame.Unknown));
            }

            var tbgProcessor = new TBGProcessor();
            var files = tbgProcessor.Process(filesToProcess);

            Directory.CreateDirectory(modData.modName);

            var sb = new StringBuilder();
            sb.AppendLine($"BACKUP ~{modData.modName}/backup~");
            if (modData.author != String.Empty)
            {
                sb.AppendLine($"AUTHOR ~{modData.author}~");
            }
            sb.AppendLine($"LANGUAGE \"English(British)\" \"english\"");
            sb.AppendLine($"");
            sb.AppendLine($"BEGIN ~Component~");

            // From all the files we've processed, find the most common game destination and
            // assume all files we've processed should do there. We can't do much else as
            // we shouldn't be in a position where we're dealing with files aimed at
            // multiple games in a single conversion
            var grouped = files.GroupBy(g => g.game);
            var mostCommonGameGrouping = grouped.OrderByDescending(o => o.Count()).First();
            var mostCommonGame = mostCommonGameGrouping.First().game;

            var mappedGame = MapGame(mostCommonGame);
            if (mappedGame != string.Empty)
            {
                sb.AppendLine($"REQUIRE_PREDICATE GAME_IS ~{mappedGame}~");
            }

            foreach (var file in files)
            {
                var baseFilename = Path.GetFileName(file.filename);
                File.WriteAllBytes($"{modData.modName}/{baseFilename}", file.file);

                var extensionsNotToCopyToOverride = new string[] { ".DOC", ".DOCX", ".TXT", ".RTF" };
                if (!extensionsNotToCopyToOverride.Contains(Path.GetExtension(file.filename).ToUpper()))
                {
                    sb.AppendLine();

                    var copyDestination = file.filename.Replace('\\', '/');
                    if (!copyDestination.Contains('/'))
                    {
                        copyDestination = "override/" + copyDestination;
                    }

                    sb.AppendLine($"COPY ~{modData.modName}/{baseFilename}~ ~{copyDestination}~");
                    if (file.strings.Any())
                    {
                        int i = 0;
                        foreach (var textHeader in file.strings)
                        {
                            sb.AppendLine($"  SAY {textHeader.offset} ~{textHeader.text}~");
                            i++;
                        }
                    }
                }
            }

            File.WriteAllText(outputFilenameTp2, sb.ToString());
        }

        private static string MapGame(IEGame game)
        {
            return game switch
            {
                IEGame.BaldursGateI => "bg1 totsc",
                IEGame.BaldursGateII => "bg2 tob",
                IEGame.IcewindDale => "iwd / how / totlm",
                IEGame.PlanescapeTorment => "pst",
                _ => string.Empty,
            };
        }
    }
}