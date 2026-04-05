namespace SmartResumeAnalyzer.Core.Interfaces
{
    public interface IPdfParserService
    {
        string ExtractText(Stream pdfStream);
    }
}