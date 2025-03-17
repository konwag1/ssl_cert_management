using System;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace Namespace.Services
{
    public class CertificateScanner
    {
        public X509Certificate2 GetCertificate(string hostname)
        {
            try
            {
                using (TcpClient client = new TcpClient(hostname, 443))
                using (SslStream sslStream = new SslStream(client.GetStream(), false,
                    new RemoteCertificateValidationCallback((sender, certificate, chain, sslPolicyErrors) => true)))
                {
                    // Inicjalizacja połączenia SSL
                    sslStream.AuthenticateAsClient(hostname);

                    // Pobieranie certyfikatu serwera
                    var cert = sslStream.RemoteCertificate;

                    if (cert != null)
                    {
                        return new X509Certificate2(cert);
                    }
                    else
                    {
                        throw new Exception("No certificate found.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching certificate: {ex.Message}");
                return null;
            }
        }
    }
}
