namespace AwfulForumsLibrary.Models.Web
{
    public class Error
    {
        public string Type { get; set; }
        public string Reason { get; set; }
        public string StackTrace { get; set; }
        public bool IsPaywall { get; set; }
    }
}
