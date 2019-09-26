using System;
using System.IO;
using System.Text;
using CommandLine;

namespace ReplaceInFiles
{
    class Program
    {
        public class Options
        {
            [Option('o', "old text", Required = true, HelpText = "File containing the old (to be replaced) text")]
            public string OldTextFileName { get; set; }

            [Option('n', "new text", Required = true, HelpText = "File containing the new (replacement) text")]
            public string NewTextFileName { get; set; }

            [Option('p', "path", Required = true, HelpText = "Directory path from which to start")]
            public string PathRoot { get; set; }

            [Option('e', "extensions", Required = true, HelpText = "File extansions, such as *.txt")]
            public string Extensions { get; set; }
        }

        public static Encoding GetEncoding(string filename)
        {
            // Read the BOM
            var bom = new byte[4];
            using (var file = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                file.Read(bom, 0, 4);
            }

            // Analyze the BOM
            if (bom[0] == 0x2b && bom[1] == 0x2f && bom[2] == 0x76) return Encoding.UTF7;
            if (bom[0] == 0xef && bom[1] == 0xbb && bom[2] == 0xbf) return Encoding.UTF8;
            if (bom[0] == 0xff && bom[1] == 0xfe) return Encoding.Unicode; //UTF-16LE
            if (bom[0] == 0xfe && bom[1] == 0xff) return Encoding.BigEndianUnicode; //UTF-16BE
            if (bom[0] == 0 && bom[1] == 0 && bom[2] == 0xfe && bom[3] == 0xff) return Encoding.UTF32;
            return Encoding.ASCII;
        }

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
            .WithParsed<Options>(o =>
            {
                var oldString = File.ReadAllText(o.OldTextFileName);
                var newString = File.ReadAllText(o.NewTextFileName);
                var rootPath = o.PathRoot;

                var files = Directory.GetFiles(rootPath, o.Extensions, SearchOption.AllDirectories);

                foreach (var file in files)
                {
                    var fileContent = File.ReadAllText(file);
                    if(fileContent.Contains(oldString))
                    {
                        /*
                        if (fileContent.Contains(newString))
                        {
                            ; // avoid double-replacements
                        }
                        else
                        */
                        {
                            var originalEncoding = GetEncoding(file);
                            Console.WriteLine($"Replacement in file {file}");
                            fileContent = fileContent.Replace(oldString, newString);
                            File.WriteAllText(file, fileContent, originalEncoding);
                        }
                    }
                }
            });
        }
    }
}
