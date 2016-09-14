using System;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Net.Http.Headers;

namespace MailGun.Services
{

     public class MailGunApiEmailSender: IEmailSender
     {

         // 
         // Identify the connection type to the MailGun server
         // 
         public const string ConnectionType = "api";

         // 
         // Identify the external mail service provider.
         // 
         public const string Provider = "MailGun";

         // 
         // Configuration setting for routing email through MailGun API.
         // 
         private readonly MailGunApiEmailSettings _apiEmailConfig;

         // 
         // Accessor for the configuration to send mail via MailGun API.
         // 
         public MailGunApiEmailSettings EmailSettings
         {

             get 
             {

                 return _apiEmailConfig;

             }

         }

         // 
         // Default constructor with email configuration initialized via
         // option configuration.
         // 
         public MailGunApiEmailSender(
             IOptions<MailGunApiEmailSettings> apiEmailConfig)
         {

             _apiEmailConfig = apiEmailConfig.Value;

         }

         //
         // Sends email via MailGun REST api, given email recipient, email
         // subject, and email body.
         // 
         public async Task SendEmailAsync(
             string email, 
             string subject, 
             string message
         ) 
         {

             string apiKey = EmailSettings.ApiKey;

             string baseUri = EmailSettings.BaseUri;

             string requestUri = EmailSettings.RequestUri;

             string token = HttpBasicAuthHeader("api", apiKey);

             FormUrlEncodedContent emailContent = HttpContent(email,
                 subject,
                 message
             );

             using (var client = new HttpClient())
             {

                 client.BaseAddress = new Uri(baseUri);

                 client.DefaultRequestHeaders.Authorization = 
                     new AuthenticationHeaderValue("Basic", token);

                 HttpResponseMessage response = await client
                     .PostAsync(requestUri, emailContent)
                     .ConfigureAwait(false);

                 if (false == response.IsSuccessStatusCode)
                 {

                     string errMsg = String.Format(
                         "Failed to send mail to {0} via {1} {2} due to\n{3}.",
                         email,
                         Provider,
                         ConnectionType,
                         response.ToString()
                     );

                     throw new HttpRequestException(errMsg);

                 }

             }

         }

         // 
         // Put together the basic authentication header for MailGun 
         // Rest API (see https://documentation.mailgun.com/quickstart-sending.html#send-via-api).
         // 
         protected string HttpBasicAuthHeader(
             string tokenName, 
             string tokenValue
         )
         {

             string tokenString = string.Format(
                 "{0}:{1}",
                 tokenName,
                 tokenValue
             );

             byte[] bytes = Encoding.UTF8.GetBytes(tokenString);

             string authHeader = Convert.ToBase64String(bytes);

             return authHeader;

         }

         // 
         // Put together the core elements for the email for MailGun.
         // 
         // For a list of valid fields, please refer to MailGun document 
         // (see https://documentation.mailgun.com/api-sending.html#sending).
         //
         protected FormUrlEncodedContent HttpContent(
             string recipient,
             string subject,
             string message
         )
         {

             string sender = EmailSettings.From;

             var emailSender = new KeyValuePair<string, string>("from", sender);

             var emailRecipient = 
                 new KeyValuePair<string, string>("to", recipient);

             var emailSubject = 
                 new KeyValuePair<string, string>("subject", subject);

             var emailBody = 
                 new KeyValuePair<string, string>("text", message);

             List<KeyValuePair<string, string>> content = 
                 new List<KeyValuePair<string, string>>();

             content.Add(emailSender);

             content.Add(emailRecipient);

             content.Add(emailSubject);

             content.Add(emailBody);

             FormUrlEncodedContent urlEncodedContent = 
                 new FormUrlEncodedContent(content);

             return urlEncodedContent;

         }

     }

}
