namespace aspnetcoreapp.Services{
    public class EmailSettings
    {
        public required string SmtpServer { get; set; }
        public required int SmtpPort { get; set; }
        public required string SmtpUser { get; set; }
        public required string SmtpPass { get; set; }
    }
}