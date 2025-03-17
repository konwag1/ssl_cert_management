namespace Namespace.Models
{
    public class CertificateRequestModel
    {
        public string CommonName { get; set; }
        public string Organization { get; set; }
        public string OrganizationalUnit { get; set; }
        public string Country { get; set; }
        public string StateOrProvince { get; set; }
        public string Locality { get; set; }
        public string Password { get; set; }
    }

    public class CertificateResponseModel
    {
        public string Message { get; set; }
        public string Path { get; set; }
    }
}
