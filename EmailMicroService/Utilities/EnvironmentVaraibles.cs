namespace EmailMicroService.Utilities
{
    public static class EnvironmentVaraibles
    {
        public static string? SmtpHost => Environment.GetEnvironmentVariable("SMTP_HOST");

        public static int SmtpPort => int.Parse(Environment.GetEnvironmentVariable("SMTP_PORT") ?? "587");

        public static string? SmtpLogin => Environment.GetEnvironmentVariable("SMTP_LOGIN");

        public static string? SmtpKey => Environment.GetEnvironmentVariable("SMTP_KEY");
    }
}
