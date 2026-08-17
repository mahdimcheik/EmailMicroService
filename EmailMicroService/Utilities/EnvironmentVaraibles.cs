namespace EmailMicroService.Utilities
{
    public static class EnvironmentVaraibles
    {
        public static string SmtpHost => GetRequired("SMTP_HOST");

        public static int SmtpPort => int.Parse(Environment.GetEnvironmentVariable("SMTP_PORT") ?? "587");

        public static string SmtpLogin => GetRequired("SMTP_LOGIN");

        public static string SmtpKey => GetRequired("SMTP_KEY");

        private static string GetRequired(string name) =>
            Environment.GetEnvironmentVariable(name)
            ?? throw new InvalidOperationException($"Missing required environment variable '{name}'.");
    }
}
