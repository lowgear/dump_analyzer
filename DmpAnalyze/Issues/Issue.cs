namespace DmpAnalyze.Issues
{
    public class Issue : IIssue
    {
        public string Title { get; }
        public string Message { get; }

        public Issue(string title, string message)
        {
            Title = title;
            Message = message;
        }
    }
}