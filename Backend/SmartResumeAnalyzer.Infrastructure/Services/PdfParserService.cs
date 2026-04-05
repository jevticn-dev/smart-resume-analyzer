using SmartResumeAnalyzer.Core.Exceptions;
using SmartResumeAnalyzer.Core.Interfaces;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace SmartResumeAnalyzer.Infrastructure.Services
{
    public class PdfParserService : IPdfParserService
    {
        public string ExtractText(Stream pdfStream)
        {
            try
            {
                using var memoryStream = new MemoryStream();
                pdfStream.CopyTo(memoryStream);
                memoryStream.Position = 0;

                using var document = PdfDocument.Open(memoryStream.ToArray());
                var textBuilder = new System.Text.StringBuilder();

                foreach (Page page in document.GetPages())
                    textBuilder.AppendLine(page.Text);

                return textBuilder.ToString().Trim();
            }
            catch (Exception ex) when (ex is not AppException)
            {
                throw new AppException("Failed to parse PDF file. Please ensure the file is a valid PDF.");
            }
        }
    }
}