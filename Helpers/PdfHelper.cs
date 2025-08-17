using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using System.Text;


namespace RagBasedChatbot.Helpers
{
    public static class PdfHelper
    {
        public static string ExtractText(string pdfPath)
        {
            using (var pdf = new PdfReader(pdfPath))
            using (var pdfDoc = new PdfDocument(pdf))
            {
                StringBuilder text = new StringBuilder();
                for (int i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
                {
                    var page = pdfDoc.GetPage(i);
                    text.Append(PdfTextExtractor.GetTextFromPage(page));
                }
                return text.ToString();
            }
        }
    }
}