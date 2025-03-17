using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Namespace.Models;

namespace Namespace.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SSLCertificatesController : ControllerBase
    {
        private readonly string connectionString = "Server=KONRADACER\\SQLEXPRESS;Database=SslManagementDb;Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True;";

        [HttpGet]
        public ActionResult<List<SSLCertificate>> GetCertificates()
        {
            var certificates = new List<SSLCertificate>();

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

            if (certificates.Count == 0)
            {
                return NotFound("No certificates found.");
            }
            return certificates;
        }

        [HttpPost]
        public IActionResult AddCertificate([FromBody] SSLCertificate sslCertificate)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var command = new SqlCommand("INSERT INTO SSLCertificates (Domain, ExpiryDate) VALUES (@Domain, @ExpiryDate)", connection);
                command.Parameters.AddWithValue("@Domain", sslCertificate.Domain);
                command.Parameters.AddWithValue("@ExpiryDate", sslCertificate.ExpiryDate);
                var rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    return Ok("Certificate added successfully.");
                }
                else
                {
                    return StatusCode(500, "Failed to add certificate to database.");
                }
            }
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteCertificate(int id)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var command = new SqlCommand("DELETE FROM SSLCertificates WHERE Id = @Id", connection);
                command.Parameters.AddWithValue("@Id", id);
                var rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    return Ok("Certificate deleted successfully.");
                }
                else
                {
                    return NotFound("Certificate not found.");
                }
            }
        }

        [HttpGet("scan")]
        public async Task<IActionResult> Scan([FromQuery] string url)
        {
            try
            {
                if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
                {
                    return BadRequest("The URL must be an absolute URI.");
                }

                var cert = await GetSslCertificateAsync(url);
                var certificateDetails = new SSLCertificate
                {
                    Domain = url,
                    ExpiryDate = cert.NotAfter
                };

                return Ok(certificateDetails);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        private async Task<X509Certificate2> GetSslCertificateAsync(string url)
        {
            var uri = new Uri(url);
            using var client = new System.Net.Sockets.TcpClient();
            await client.ConnectAsync(uri.Host, 443);
            using var sslStream = new System.Net.Security.SslStream(client.GetStream(), false, (sender, certificate, chain, sslPolicyErrors) => true);
            await sslStream.AuthenticateAsClientAsync(uri.Host);
            var cert = new X509Certificate2(sslStream.RemoteCertificate);
            return cert;
        }
    }
}
