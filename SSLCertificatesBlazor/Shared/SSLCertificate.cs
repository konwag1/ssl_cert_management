namespace SSLCertificatesBlazor.Shared
{
        public class SSLCertificate
        {
            public int Id { get; set; }
            public string Domain { get; set; }
            public DateTime ExpiryDate { get; set; }
        }
}
