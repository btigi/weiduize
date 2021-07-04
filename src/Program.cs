using weiduize.Model;
using System;
using System.IO;


namespace weiduize
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("weiduize ");
                Console.WriteLine("Usage");
                Console.WriteLine("  weiduize file.tbg");
                Console.WriteLine("  weiduize file.iap");
                return;
            }

            var inputFilename = args[0];
            if (!File.Exists(inputFilename))
            {
                Console.WriteLine($"Input file {inputFilename} does not exist");
            }

            var modData = GetModData(args);

            var weiduizer = new weiduizer();
            weiduizer.Process(modData, inputFilename);

            Console.WriteLine("Conversion complete");
        }

        private static (string modName, string author) GetModData(string[] args)
        {
            var defaultModName = Path.GetFileNameWithoutExtension(args[0]);

            Console.WriteLine($"Enter the modname ([enter] for '{defaultModName}'):");
            var modName = Console.ReadLine().Replace(" ", "");
            if (modName == String.Empty)
            {
                modName = defaultModName;
            }

            Console.WriteLine("Enter the mod author ([enter] for blank):");
            var author = Console.ReadLine();

            return (modName, author);
        }
    }
}