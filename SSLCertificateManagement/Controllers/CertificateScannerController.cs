using Microsoft.AspNetCore.Mvc;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Net.Sockets;

namespace Namespace.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CertificateScannerController : ControllerBase
    {
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
                var certificateDetails = new CertificateDetails
                {
                    Subject = cert.Subject,
                    Issuer = cert.Issuer,
                    ExpiryDate = cert.NotAfter
                };

                return Ok(certificateDetails);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        private async Task<X509Certificate2> GetSslCertificateAsync(string url)
        {
            var uri = new Uri(url);
            using var client = new TcpClient();
            await client.ConnectAsync(uri.Host, 443);
            using var sslStream = new SslStream(client.GetStream(), false, (sender, certificate, chain, sslPolicyErrors) => true);
            await sslStream.AuthenticateAsClientAsync(uri.Host);
            var cert = new X509Certificate2(sslStream.RemoteCertificate);
            return cert;
        }
    }

    public class CertificateDetails
    {
        public string Subject { get; set; }
        public string Issuer { get; set; }
        public DateTime ExpiryDate { get; set; }
    }
}
