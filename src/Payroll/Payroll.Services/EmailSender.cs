using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace Payroll.Services
{
    public interface IEmailSender
    {
        Task<bool> SendEmailConfirmationAsync(string email, string url, List<string> ccRecipients = null);
    }

    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration configuration;
        private readonly IWebHostEnvironment env;
        private readonly ILogger<EmailSender> logger;
        public string Error { get; private set; }

        public EmailSender(IConfiguration configuration, IWebHostEnvironment env,
            ILogger<EmailSender> logger)
        {
            this.env = env;
            this.configuration = configuration;
            this.logger = logger;
        }

        public readonly IDictionary<string, string> emailBodyDict = new Dictionary<string, string>{
            { nameof(SendEmailConfirmationAsync), "/email-confirm-template.html" }
        };

        public async Task<bool> SendEmailConfirmationAsync(string email, string url, List<string> ccRecipients = null)
        {

            try
            {
                // string _filePath = env.EnvironmentName System.IO.Path.GetDirectoryName(System.AppDomain.CurrentDomain.BaseDirectory);

                //var path = System.IO.Path.Combine(env.WebRootPath + emailBodyDict[nameof(this.SendEmailConfirmationAsync)]);
                string body = await GetBody(nameof(this.SendEmailConfirmationAsync));
                body = body.Replace("{p-confirm-url}", url);

                bool result = await TrySendingMailAsync(
                    subject: "Please confirm your email address",
                    body: body,
                    isBodyHtml: true,
                    toEmail: email,
                    ccRecipients: ccRecipients
                );

                if(result)
                    logger.LogInformation("Email sent successfully");
                else
                    logger.LogWarning("Email FAILED!");
                return result;
            }
            catch (System.Exception)
            {
                throw;
            }

        }

        private async Task<string> GetBody(string method)
        {
            var path = env.WebRootPath + emailBodyDict[method];

            logger.LogInformation("Building up email body and replacing placeholders");
            logger.LogInformation("found web root path: " + path);
            var body = await System.IO.File.ReadAllTextAsync(path, System.Text.Encoding.UTF8);
            return body;
        }

        private async  Task<bool> TrySendingMailAsync(string subject, string body, bool isBodyHtml, string toEmail, List<string> ccRecipients = null)
        {
            // Credentials:
            var username = "YOUR USERNAME";
            var password = "YOUR PASSWORD";
            var sentFrom = "YOUR EMAIL";
            var host = "YOUR HOST";
            var port = 587;
            var datetime = DateTime.Now;

            // Configure the client:
            System.Net.Mail.SmtpClient client = new System.Net.Mail.SmtpClient(host, port)
            {
                DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                EnableSsl = true,
                Credentials = new NetworkCredential(username, password)

            };

            try
            {
                // Create the message:
                var mail = new System.Net.Mail.MailMessage(sentFrom, toEmail);

                mail.To.Add(toEmail);
                mail.Subject = subject;
                mail.IsBodyHtml = true;
                mail.Body = body;


                if(ccRecipients != null)
                    ccRecipients.ForEach(cc => mail.CC.Add(cc));

                //mail.Bcc.Add(sentFrom);


                await client.SendMailAsync(mail);
            }
            catch (Exception ex)
            {
                Error = ex.Message;
                logger.LogError(ex, "Error while sending email: ");
                //error = ex.Message;
                //result = false;
                //await new ErrorLogger().LogErrorAsync(ex, null);
                return false;
            }

            finally
            {
                //var logger = new NTConsoleLogger(ConsoleLogTypes.Communication, false);
                //logger.Log(message, new BMCommunication
                //{
                //    BmCommunicationType = BMCommunicationType.Email,
                //    IsSuccessful = result,
                //    ErrorInformation = error,
                //    CommunicationMetaData = $"HostAddress: {host}, Username: {username}, Password: {password}, port: {port}, sender: {sender}"
                //});

            }
            return true;
            // new HttpStatusCodeResult(200);
        }
    }
}
