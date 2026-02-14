using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using System.Text.Json;

namespace Razor_Pages.Pages
{
    // CSRF ochrana: ověřuje, že POST request pochází z naší aplikace.
    // Tímto se zabraňuje útokům, kdy by útočník mohl posílat požadavky z jiného webu
    // (Cross-Site Request Forgery). 
    [ValidateAntiForgeryToken]
    public class LoginModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public LoginModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        [BindProperty]
        // Povinné vlastnosti z formuláře:
        // budou automaticky naplněny z POST dat
        public required string Username { get; set; }
        [BindProperty]
        public required string Password { get; set; }

        // Zpráva pro zobrazení chyb/potvrzení
        public required string Message { get; set; }

        // GET request: jen zobrazí stránku -> proto metoda OnGetAsync()
        // POST request: odesílá data z formuláře -> proto metoda OnPostAsync()
        public async Task<IActionResult> OnPostAsync()
        {
            var client = _httpClientFactory.CreateClient();

            var content = new StringContent(
                JsonSerializer.Serialize(new { username = Username, password = Password }),
                Encoding.UTF8,
                "application/json"
            );

            // login endpoint - Web UI volá Web API
            // Volání zabezpečeného endpointu Web API z Razor Page
            // ⚠️ Architektura je zde trochu duplicitní – URL API je hardcoded jak v Login.cs, tak v Secure.cs

            // Doporučení: extrahovat základní URL (https://localhost:7002) do Program.cs nebo konfigurační proměnné
            // aby se endpoint udržoval na jednom místě a bylo snazší změnit např. prostředí (Development/Production)
            var response = await client.PostAsync("https://localhost:7002/api/auth/login", content);

            if (response.IsSuccessStatusCode)
            {
                // JSON se deserializuje do JsonElement,
                // aby s ním bylo možné pracovat jako s objektem nebo polem
                // a procházet ho např. pomocí result[i] ve for-cyklu
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<JsonElement>(json);

                // Získání tokenu z JSON
                var token = result.GetProperty("token").GetString();

                if (string.IsNullOrEmpty(token))
                {
                    Message = "Login failed - token is empty";
                    return Page();
                }

                // This saves token into server-side session "cookie"
                HttpContext.Session.SetString("JWToken", token);

                return RedirectToPage("Secure");
            }
            else {
                Message = "Login failed";
                return Page();
            }
        }
    }
}
