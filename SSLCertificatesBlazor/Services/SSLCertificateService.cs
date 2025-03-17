using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using SSLCertificatesBlazor.Models;

namespace SSLCertificatesBlazor.Services
{
    public class SSLCertificateService
    {
        private readonly HttpClient _httpClient;

        public SSLCertificateService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<SSLCertificate>> GetCertificatesAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<SSLCertificate>>("api/SSLCertificates");
        }

        public async Task AddCertificateAsync(SSLCertificate certificate)
        {
            await _httpClient.PostAsJsonAsync("api/SSLCertificates", certificate);
        }

        public async Task UpdateCertificateAsync(int id, SSLCertificate certificate)
        {
            await _httpClient.PutAsJsonAsync($"api/SSLCertificates/{id}", certificate);
        }

        public async Task DeleteCertificateAsync(int id)
        {
            await _httpClient.DeleteAsync($"api/SSLCertificates/{id}");
        }
    }
}
