using System.Drawing;
using System.Drawing.Printing;

public class Printer
{
    private static Font printFont;
    private static StreamReader streamToPrint;

    // The PrintPage event is raised for each page to be printed.
    private static void pd_PrintPage(object sender, PrintPageEventArgs ev)
    {
        float linesPerPage = 0;
        float yPos = 0;
        int count = 0;
        float leftMargin = 50;       //ev.MarginBounds.Left
        float topMargin = ev.MarginBounds.Top;
        String line = null;

        // Calculate the number of lines per page.
        linesPerPage = ev.MarginBounds.Height /
           printFont.GetHeight(ev.Graphics);

        // Iterate over the file, printing each line.
        while (count < linesPerPage &&
           ((line = streamToPrint.ReadLine()) != null))
        {
            yPos = topMargin + (count * printFont.GetHeight(ev.Graphics));
            ev.Graphics.DrawString(line, printFont, Brushes.Black,
               leftMargin, yPos, new StringFormat());
            count++;
        }

        // If more lines exist, print another page.
        if (line != null)
            ev.HasMorePages = true;
        else
            ev.HasMorePages = false;
    }

    // Print the file.
    public static void Print(string filePath)
    {
        try
        {
            streamToPrint = new StreamReader(filePath);
            try
            {
                printFont = new Font("Courier New", 10);
                using PrintDocument pd = new PrintDocument();
                pd.PrinterSettings.PrinterName = "Microsoft Print to PDF";
                pd.PrintPage += new PrintPageEventHandler(pd_PrintPage);
                // Print the document.
                pd.PrinterSettings.PrintToFile = true;
                pd.PrinterSettings.PrintFileName = filePath.Remove(filePath.Length - 4) + ".pdf";
                pd.Print();
            }
            finally
            {
                streamToPrint.Close();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}