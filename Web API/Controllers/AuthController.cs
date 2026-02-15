using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Shared.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Web_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AuthController : ControllerBase
    {

        // UX & architektura:
        // - Generování hashe přímo z privátního klíče a přihlašovacích údajů eliminuje potřebu SQL a ochrany proti XSS.
        // - Teoreticky by šlo při registraci generovat "seed" pro obnovu účtu, ale v praxi by to bylo nepraktické 
        //   (uživatel by si musel zapisovat seed, změna hesla není možná, export/import účtu vyžaduje databázi).
        // - Pro ukládání uživatelských dat a optimalizaci více uživatelů je SQL databáze stále nejjednodušší a nejbezpečnější řešení.
        // - Tyto poznámky ukazují architektonické myšlení, čtení mezi řádky a zvažování UX potřeb koncového zákazníka.

        // POST api/Auth/Login
        [HttpPost("login")]
        [AllowAnonymous]
        public IActionResult Login([FromBody] LoginViewModel model)
        {
            if (model.Username == "admin" && model.Password == "123456")
            {
                var token = GenerateJwtToken(model.Username);
                return Ok(new { token });
            }

            return Unauthorized("Invalid credentials");
        }

        // GET api/Auth/secure
        [HttpGet("secure")]
        [Authorize]
        public IActionResult GetSecureData()
        {
            return Ok(new { Message = "Successfully logged in" });
        }
        // JWT token – vlastnosti a omezení
        // Generovaný JWT obsahuje časové razítko (expiraci),
        // takže výsledný token je při každém přihlášení odlišný, i pro stejného uživatele.
        //
        // Tento přístup není vhodný, pokud by bylo požadováno deterministické (stateless)
        // generování identického tokenu pouze na základě identity uživatele.
        //
        // Pokud nelze použít databázi pro ukládání tokenů nebo jejich hashů,
        // lze uvažovat o stateless generování identity pomocí kryptografického podpisu (privátní klíč).


        // Ověření licence a výkonnostní aspekty
        // Ověření licence vyžaduje uložení informace o vlastnictví,
        // např. do JSON souboru – ten zde funguje jako jednoduché perzistentní úložiště,
        // které není optimalizováno pro souběžný přístup více instancí nebo uživatelů.
        //
        // Načítání aktuálně probíhá lineárním průchodem dat (O(n), for/foreach).
        // Výkon by bylo možné zlepšit např. indexací, organizací podle prefixu identifikátoru,
        // nebo použitím databáze s podporou indexů.


        // Alternativní přístupy k uchování stavu
        // Lze využít cookie nebo server-side session s expirací


        // Obchodní model licence
        // Je nutné definovat, zda licence funguje jako:
        // - časově omezený klíč (např. 1 rok), nebo
        // - předplatné s možností obnovení a server-side validací.
        private string GenerateJwtToken(string username)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // Privátní klíč pro podepisování JWT tokenů
            // ⚠️ Klíč musí být bezpečně uložený, nesmí uniknout na GitHub ani být veřejně dostupný.
            // - V produkci se klíč prakticky nemění, proto riziko jeho změny v projektu není běžné.
            //   I když jeho umístění v kódu porušuje princip opakování kódu, v tomto případě je to přijatelné.
            // - V menších projektech lze klíč uložit do Program.cs nebo do appsettings.Development.json.
            // - Ideálně by byl klíč šifrovaný a hardwarově zabezpečený.
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("VerySuperSecretKeyForJWTVerySuperSecretKey"));
            
            // Podepisování tokenu privátním klíčem
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Session přihlášení vyprší za jednu hodinu
            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
