namespace DmpAnalyze.Issues
{
    public interface IIssue
    {
        string Title { get; }
        string Message { get; }
    }
}