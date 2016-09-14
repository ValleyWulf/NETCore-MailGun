using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using System.Security.Authentication;
using MailKit.Net.Smtp;
using MimeKit;


namespace MailGun.Services
{

    public class GmailSmtpEmailSender: IEmailSender
    {
        //
        // Identify the connection type to the Gmail server
        //
        public const string ConnectionType = "smtp";

        // 
        // Identify the external mail service provider
        // 
        public const string Provider = "Gmail";

        // 
        // Configuration setting for routing email through Gmail SMTP.
        //
        private readonly GmailSmtpEmailSettings _smtpEmailConfig;

        //
        // Accessor for the configuration to send mail via Gmail SMTP
        // 
        public GmailSmtpEmailSettings EmailSettings
        {

            get
            {

                return _smtpEmailConfig;

            }

        }

        // 
        // Default constructor with email configuration initialized via
        // IOption.
        // 
        public GmailSmtpEmailSender(
            IOptions<GmailSmtpEmailSettings> smtpEmailConfig
        )
        {

            _smtpEmailConfig = smtpEmailConfig.Value;

        }

        // 
        // Sends email via MailGun SMTP, given email recipient, email
        // subject, and email body.
        // 
        public async Task SendEmailAsync(
            string email,
            string subject,
            string message
        )
        {

            string host = EmailSettings.SmtpHost;

            int port = EmailSettings.SmtpPort;

            string login = EmailSettings.SmtpLogin;

            string code = EmailSettings.SmtpPassword;

            string senderName = EmailSettings.SenderName;

            string sender = EmailSettings.From;

            var emailMsg = new MimeMessage();

            emailMsg.From.Add(new MailboxAddress(senderName, sender));

            emailMsg.To.Add(new MailboxAddress("", email));

            emailMsg.Subject = subject;

            emailMsg.Body = new TextPart("plain") { Text = message };

            using (SmtpClient client = new SmtpClient()) 
            {

                try {

                    await client.ConnectAsync(host, port, false)
                        .ConfigureAwait(false);

                    client.AuthenticationMechanisms.Remove ("XOAUTH2");

                    await client.AuthenticateAsync(login, code);

                    await client.SendAsync(emailMsg).ConfigureAwait(false);;

                }
                catch (AuthenticationException ex)
                {
                    Console.WriteLine("Error: Authentication exception.");

                    Console.WriteLine("\tException message: {0}", ex.Message);

                    throw ex;

                }
                catch (SmtpCommandException ex)
                {

                    Console.WriteLine(
                        "Error: SMTP exception encountered via {0} {1}.",
                        Provider,
                        ConnectionType);

                    Console.WriteLine("\tException message: {0}", ex.Message);

                    Console.WriteLine ("\tStatus code: {0}", ex.StatusCode);

                    switch (ex.ErrorCode) 
                    {

                        case SmtpErrorCode.RecipientNotAccepted:

                            Console.WriteLine(
                                "Error: Recipient not accepted: {0}", 
                                ex.Mailbox
                            );

                            break;

                        case SmtpErrorCode.SenderNotAccepted:

                            Console.WriteLine(
                                "Error: Sender not accepted: {0}", 
                                ex.Mailbox
                            );

                            break;

                        case SmtpErrorCode.MessageNotAccepted:

                            Console.WriteLine("Error: Message not accepted.");

                            break;

                        default:
                            Console.WriteLine("Error: {0}.", ex.Message);

                            break;

                    }

                    throw ex;

                }
                catch (SmtpProtocolException ex)
                {

                    Console.WriteLine("Error: SMTP protocol exception.");

                    Console.WriteLine("\tException message: {0}", ex.Message);
                        
                    throw ex;

                }
                catch (Exception ex)
                {

                    Console.WriteLine("Error: Failed to send mail via {0} {1}.",
                        Provider,
                        ConnectionType
                    );

                    throw ex;


                }
                finally
                {

                    if (true == client.IsConnected)
                    {

                        await client.DisconnectAsync(true)
                            .ConfigureAwait(false);

                    }

                }



                if (true == client.IsConnected)
                {

                    await client.DisconnectAsync(true).ConfigureAwait(false);

                }

            }

        }

    }

}
