namespace MailGun.Services
{

    public class MailGunSmtpEmailSettings
    {

        public string SmtpHost { get; set; }

        public int SmtpPort { get; set; }

        public string SmtpLogin { get; set; }

        public string SmtpPassword { get; set; }

        public string SenderName { get; set; }

        public string From { get; set; }

    }

    public class MailGunApiEmailSettings
    {

        public string ApiKey { get; set; }

        public string BaseUri { get; set; }

        public string RequestUri { get; set; }

        public string From { get; set; }

    }

}

