using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Namespace.Models;

namespace Namespace.Services
{
    public class CertificateMonitor : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;
        private readonly string connectionString = "Server=KONRADACER\\SQLEXPRESS;Database=SslManagementDb;Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True;";

        public CertificateMonitor(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Od razu po uruchomieniu sprawdzenie certyfikatów
            await CheckCertificates();

            // Następnie sprawdzanie co 24 godziny
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
                await CheckCertificates();
            }
        }

        private async Task CheckCertificates()
        {
            var certificates = GetCertificatesFromDatabase();
            foreach (var cert in certificates)
            {
                if (cert.ExpiryDate <= DateTime.Now)
                {
                    continue; // Nie wysyłaj e-maila, jeśli certyfikat jest przeterminowany
                }

                if ((cert.ExpiryDate - DateTime.Now).TotalDays <= 30)
                {
                    SendNotification(cert.Domain, (int)(cert.ExpiryDate - DateTime.Now).TotalDays);
                }
            }
        }

        private List<SSLCertificate> GetCertificatesFromDatabase()
        {
            var certificates = new List<SSLCertificate>();

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    var command = new SqlCommand("SELECT Id, Domain, ExpiryDate FROM SSLCertificates", connection);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var certificate = new SSLCertificate
                            {
                                Id = reader.GetInt32(0),
                                Domain = reader.GetString(1),
                                ExpiryDate = reader.GetDateTime(2)
                            };
                            certificates.Add(certificate);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching certificates from database: {ex.Message}");
            }

            return certificates;
        }

        private void SendNotification(string domain, int daysRemaining)
        {
            var emailSettings = _configuration.GetSection("EmailSettings");
            var fromAddress = new MailAddress(emailSettings["FromAddress"], "Cert Notification");
            var toAddress = new MailAddress("our_mail", "Admin");
            string fromPassword = emailSettings["FromPassword"];
            string subject = $"SSL Certificate Expiry Warning for {domain}";
            string body = $"The SSL certificate for {domain} will expire in {daysRemaining} days.";

            try
            {
                Console.WriteLine("Starting email sending process...");

                var smtp = new SmtpClient
                {
                    Host = emailSettings["SmtpHost"],
                    Port = int.Parse(emailSettings["SmtpPort"]),
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
                };

                using var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = body
                };

                smtp.Send(message);
                Console.WriteLine("Email sent successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending email notification: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
}
