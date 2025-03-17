using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Namespace.Models;

namespace Namespace.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CertificateGeneratorController : ControllerBase
    {
        [HttpPost]
        public IActionResult CreateCertificate([FromBody] CertificateRequestModel request)
        {
            try
            {
                // Generowanie klucza prywatnego
                using var rsa = RSA.Create(2048);
                var requestData = new System.Security.Cryptography.X509Certificates.CertificateRequest(
                    $"CN={request.CommonName}, O={request.Organization}, OU={request.OrganizationalUnit}, C={request.Country}, ST={request.StateOrProvince}, L={request.Locality}",
                    rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1
                );

                // Samopodpisanie certyfikatu
                var certificate = requestData.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddYears(1));

                // Eksportowanie certyfikatu z hasłem od użytkownika
                var pfxBytes = certificate.Export(X509ContentType.Pfx, request.Password);

                // Zapisywanie certyfikatu do pliku 
                var path = Path.Combine("C:\\Users\\Konrad\\source\\repos\\SSLCertificateManagement\\Certificates", "my-certificate.pfx");
                System.IO.File.WriteAllBytes(path, pfxBytes);

                return Ok(new CertificateResponseModel { Message = "Certificate created successfully", Path = path });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
