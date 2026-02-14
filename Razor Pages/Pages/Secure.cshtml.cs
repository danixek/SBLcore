using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Razor_Pages.Pages
{
    public class SecureModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public SecureModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // Zpráva pro zobrazení chyb/potvrzení
        public required string Message { get; set; }

        // GET request: jen zobrazí stránku -> proto metoda OnGetAsync()
        // POST request: odesílá data z formuláře -> proto metoda OnPostAsync()
        public async Task OnGetAsync()
        {
            // Načte token ze server-side "cookie" session
            var token = HttpContext.Session.GetString("JWToken");

            if (string.IsNullOrEmpty(token))
            {
                Message = "You are not logged in.";
                return;
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // endpoint - Web UI volá Web API
            // Volání zabezpečeného endpointu Web API z Razor Page
            
            // ⚠️ Architektura je zde trochu duplicitní – URL API je hardcoded jak v Login.cs, tak v Secure.cs
            // Doporučení: extrahovat základní URL (https://localhost:7002) do Program.cs nebo konfigurační proměnné
            // aby se endpoint udržoval na jednom místě a bylo snazší změnit např. prostředí (Development/Production)
            var response = await client.GetAsync("https://localhost:7002/api/auth/secure");

            if (response.IsSuccessStatusCode)
            {
                // JSON se deserializuje do JsonElement,
                // aby s ním bylo možné pracovat jako s objektem nebo polem
                // a procházet ho např. pomocí result[i] ve for-cyklu
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<JsonElement>(json);

                Message = result.GetProperty("message").GetString() ?? "No message returned";
            }
            else { 
                Message = "Failed to load secure data.";
            }

        }
    }
}
