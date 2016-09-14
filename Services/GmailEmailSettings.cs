namespace MailGun.Services
{

    public class GmailSmtpEmailSettings
    {

        public string SmtpHost { get; set; }

        public int SmtpPort { get; set; }

        public string SmtpLogin { get; set; }

        public string SmtpPassword { get; set; }

        public string SenderName { get; set; }

        public string From { get; set; }

    }

}
