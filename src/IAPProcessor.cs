using weiduize.Model;
using System.Collections.Generic;
using System.IO;

namespace weiduize
{
    public class IAPProcessor
    {
        public List<(byte[] bytes, string filename, IEGame game)> Process(byte[] bytes)
        {
            var result = new List<(byte[] bytes, string filename, IEGame game)>();
            if (bytes[0] == 'I' && bytes[1] == 'A' && bytes[2] == 'P' && bytes[3] == '\0')
            {
                using var ms = new MemoryStream(bytes);
                using var br = new BinaryReader(ms);

                var signature = br.ReadChars(4);
                var unused = br.ReadInt32();
                var tbgFileCount = br.ReadInt32();
                var otherFileCount = br.ReadInt32();
                var game = (IEGame)br.ReadByte();
                var iapLength = br.ReadInt32();

                var nextHeader = (long)0;
                for (int i = 0; i < tbgFileCount + otherFileCount; i++)
                {
                    if (nextHeader != default)
                    {
                        br.BaseStream.Seek(nextHeader, SeekOrigin.Begin);
                    }
                    var filenameOffset = br.ReadInt32();
                    var filenameLength = br.ReadInt32();
                    var launchFile = br.ReadInt32(); // 0 = No, 1 = Yes
                    var fileOffset = br.ReadInt32();
                    var fileLength = br.ReadInt32();
                    nextHeader = br.BaseStream.Position;

                    br.BaseStream.Seek(fileOffset, SeekOrigin.Begin);
                    var f = br.ReadBytes(fileLength);

                    br.BaseStream.Seek(filenameOffset, SeekOrigin.Begin);
                    var fname = string.Join("", br.ReadChars(filenameLength));

                    result.Add((f, fname, game));
                }
            }
            return result;
        }
    }
}