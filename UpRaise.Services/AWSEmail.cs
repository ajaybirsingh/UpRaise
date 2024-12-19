using UpRaise.Helpers;
using UpRaise.Models;
using Amazon.SimpleEmailV2;
using Amazon.SimpleEmailV2.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Linq;

namespace UpRaise.Services
{
    public class AWSEmail : IEmail
    {
        private readonly ILogger<AWSEmail> _logger = null;
        private readonly IOptions<SESSettings> _sesSettings = null;
        private readonly IConfiguration _configuration;

        public AWSEmail(ILogger<AWSEmail> logger, IOptions<SESSettings> sesSettings, IConfiguration configuration)
        {
            _logger = logger;
            _sesSettings = sesSettings;
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string email, string subject, string message, bool isHighPriority = false)
        {
            if (!string.IsNullOrWhiteSpace(email))
            {
                var emailList = new List<string>() { email };
                await SendEmailAsync(emailList, subject, message, isHighPriority);
            }
        }

        public async Task SendEmailAsync(List<string> emailList, string subject, string message, bool isHighPriority = false)
        {
            using (var wrappedLogger = new WrappedLogger(_logger))
            {
                try
                {
                    if (emailList != null && emailList.Any())
                    {

                        Destination destination = new Destination();

                        destination.ToAddresses = emailList;

                        // Create the subject and body of the message.
                        Content emailSubject = new Content();
                        emailSubject.Data = subject;

                        Content emailHTMLBody = new Content();
                        emailHTMLBody.Data = message;

                        Body emailBody = new Body();
                        emailBody.Html = emailHTMLBody;
                        //emailBody.Text = emailTextBody;


                        // Create a message with the specified subject and body.
                        var emailMessage = new Amazon.SimpleEmailV2.Model.Message();
                        emailMessage.Subject = emailSubject;
                        emailMessage.Body = emailBody;

                        var emailContent = new Amazon.SimpleEmailV2.Model.EmailContent();
                        emailContent.Simple = emailMessage;

                        // Assemble the email.
                        SendEmailRequest request = new SendEmailRequest();
                        request.FromEmailAddress = _sesSettings.Value.from_email;
                        request.Destination = destination;
                        request.Content = emailContent;


                        // Instantiate an Amazon SES client, which will make the service call.

                        var client = new AmazonSimpleEmailServiceV2Client(_configuration["AWS:AccessKeyId"], _configuration["AWS:AccessKeySecret"], Amazon.RegionEndpoint.USEast1);

                        if (_sesSettings.Value.send_email)
                        {
                            //#if !DEBUG
                            // Send the email
                            var response = await client.SendEmailAsync(request);
                            if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
                            {
                                wrappedLogger.LogWarning($"Error sending email - status code {response.HttpStatusCode}");
                            }
                            //#endif
                        }
                    }
                    
                }
                catch (Exception ex)
                {
                    wrappedLogger.LogError(ex);
                    throw;
                }
            }

        }

        /*
        private static MemoryStream ConvertMailMessageToMemoryStream(MailMessage message)
        {
            var stream = new MemoryStream();
            var mimeMessage = MimeMessage.CreateFromMailMessage(message);
            mimeMessage.Prepare(EncodingConstraint.None);
            mimeMessage.WriteTo(stream);
            return stream;
        }
        */

    }

}

