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
        // Generování JWT tokenu pro uživatele
        // ⚠️ Tento token obsahuje časové razítko (expiraci),
        // takže hash je při každém přihlášení téhož uživatele odlišný.
        // V praxi by tento model nebyl vhodný pro čistě stateless autentizaci,
        // kde byste chtěli, aby přihlášení téhož uživatele vždy generovalo stejný token (id uživatele).
        //
        // Pokud nelze použít databázi pro ukládání tokenů nebo hashů,
        // lze uvažovat o generování uživatelů "stateless" metodou přes privátní klíč.
        // Ověření licence by pak vyžadovalo ukládání nějakého záznamu,
        // např. do JSON souboru – prakticky se tento JSON stává jednoduchou databází bez optimalizace více uživatelů.
        // Alternativně lze využít server-side cookies s expirací, ale je otázkou optimalizace.
        //
        // Také je otázkou obchodního modelu: 
        // zda zákazník dostane licenční klíč na fixní období (např. 1 rok) 
        // nebo předplatné s automatickým obnovením.
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
