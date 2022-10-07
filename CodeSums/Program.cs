using System.Text;

namespace CodeSums
{
    class Program
    {
        static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            string currentDirectory = Environment.CurrentDirectory;

            string[] directories = Directory.GetDirectories(currentDirectory);

            foreach (string dir in directories)
            {
                if (!(dir.EndsWith("bin") || dir.EndsWith("obj") || dir.EndsWith(".vscode") || dir.EndsWith("Models") || dir.ToLower().EndsWith("weightnotes")))
                {
                    string[] subDirectories = Directory.GetDirectories(dir);
                    foreach (string subDir in subDirectories)
                    {
                        List<string> xlsxFiles = Directory.GetFiles(subDir, "*.xlsx").ToList();
                        if (xlsxFiles.Count > 0)
                        {
                            var codesSums = new Dictionary<(int, string), CodeSum>();
                            foreach (string file in xlsxFiles)
                            {
                                Controller.AddHeaderToExistingFile(file);
                                Controller.AddSumsByCode(file, codesSums);
                            }
                            Console.WriteLine("В директория {0} бяха обработени {1} файла.", subDir, xlsxFiles.Count);
                            string upperFolderName = new DirectoryInfo(Path.GetDirectoryName(subDir)).Name;
                            Controller.InsertCodesSumsInXlsx(codesSums, subDir + "_Recap_" + upperFolderName.ToUpper() + "_MMI.xlsx");
                        }
                    }
                }
            }
        }
    }
}
