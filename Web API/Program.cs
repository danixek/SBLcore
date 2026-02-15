using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

// ==================== Reflexe psaní a úpravy kódu ====================
// Psaní kódu a jeho následné upravování jsou dvě různé disciplíny.
// Psaní kódu pro trénink a práce s existujícím kódem
// (jeho úprava a pochopení) jsou dvě odlišné dovednosti.
// Aby se dovednost psát kód sladila s architekturou a pochopením,
// je dobré trénovat schopnost zapamatovat si celou funkci, zavřít zadání,
// a následně ji znovu přepsat do poznámkového bloku.

// ==================== Účel zpětných poznámek ====================
// Poznámky v kódu jsou psány zpětně a reflektivně.
// Nevypovídají o běžných pracovních návycích ani o stylu psaní autora.
// Slouží k vysvětlení architektonického rozhodování, schopnosti kód číst a myšlenkového postupu.
// Tyto reflexe následně fungují jako podklad pro refaktoring a další profesní rozvoj.
//
// Kombinace vlastního architektonického uvažování a cílené práce s dostupnými nástroji
// (např. AI asistenty) může výrazně urychlit růst,
// pokud si vývojář zachová kritické myšlení a odpovědnost za výsledné řešení.


var builder = WebApplication.CreateBuilder(args);

// Privátní klíč pro podepisování JWT tokenů.
// - Ideálně generovaný z náhodných znaků/slov, aby odolal slovníkovým a brute-force útokům.
// - Může být uložen přímo v Program.cs nebo v appsettings.Development.json (jen pro vývoj).
// - Nesmí uniknout na GitHub ani jinak být veřejně dostupný.
// - V produkci by měl být uložen bezpečně: ideálně šifrovaný a hardwarově chráněný.

var key = "VerySuperSecretKeyForJWTVerySuperSecretKey";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

}).AddJwtBearer(options =>
{
    // Parametry validace JWT tokenu
    options.TokenValidationParameters = new TokenValidationParameters
    {
        // Neověřujeme vydavatele tokenu (issuer) – v jednoduchém zadání není potřeba
        ValidateIssuer = false,

        // Neověřujeme cílovou aplikaci (audience) – token může být použit pouze v rámci tohoto API
        ValidateAudience = false,

        // Kontrola platnosti tokenu (expirace)
        ValidateLifetime = true,

        // Kontrola, zda token byl podepsán správným klíčem
        ValidateIssuerSigningKey = true,

        // Klíč pro ověření podpisu tokenu
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))

    };
});
// Nastavení CORS (Cross-Origin Resource Sharing)
// Umožňuje, aby UI aplikace (localhost:7001) mohlo volat API (localhost:7002)

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowRazor",
        policy =>
        {
            policy.WithOrigins("https://localhost:7001")
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

builder.Services.AddControllers();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
