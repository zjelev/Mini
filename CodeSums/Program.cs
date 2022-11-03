using System.Text;

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

string[] directories = Directory.GetDirectories(Environment.CurrentDirectory);

foreach (string dir in directories)
{
    if (!(dir.EndsWith("bin") || dir.EndsWith("obj") || dir.EndsWith(".vscode") || dir.EndsWith("Models")))
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
                    Controller.AddHeader(file);
                    Controller.AddSumsByCode(file, codesSums);
                }
                Console.WriteLine("В директория {0} бяха обработени {1} файла.", subDir, xlsxFiles.Count);
                string? upperFolderName = new DirectoryInfo(Path.GetDirectoryName(subDir)!).Name;
                Controller.InsertCodesSums(codesSums, subDir + "_Recap_" + upperFolderName.ToUpper() + "_MMI.xlsx");
            }
        }
    }
}
