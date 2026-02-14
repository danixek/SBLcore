# Popis projektu
Řešení obsahuje:

**Razor Pages** aplikaci s přihlašovacím formulářem (Username / Password).

**Web API** s autentizací přes JWT token a zabezpečeným endpointem.

**Komentáře v kódu**, které reflektují architektonické rozhodování, UX a bezpečnostní úvahy.

Projekt si při návrhu rozdělím na podproblémy a tyto poznatky využívám jako **vnitřní motivaci** k postupnému zlepšování produktu –  
ať už po stránce architektury, bezpečnosti, UX, obchodního modelu či celkové uživatelské zkušenosti.


# Spuštění projektu

Pro spuštění projektu doporučuji použít pokročilé editory jako Visual Studio Community nebo JetBrains Rider.
Alternativně lze použít i Visual Studio Code s doinstalovaným rozšířením C# Dev Kit, který nainstaluje .NET SDK včetně nástroje dotnet.

1. Naklonujte repozitář  
   `git clone https://github.com/danixek/SBLcore.git`  
   `cd SBLcore`
2. Sestavte projekt:  
   `dotnet build`  
   Spuštěním se zkontroluje struktura projektu a automaticky se stáhnou závislosti - NuGet balíčky.
3. Proveďte migraci databáze:
   ``` 
   dotnet ef database update
4. Spusťte projekt:  
   `dotnet run`
   
> 💡 **Poznámka:** Pokud se příkaz `dotnet ef` nezdaří, je pravděpodobně potřeba doinstalovat EF CLI nástroj:  
`dotnet tool install --global dotnet-ef`

# Přihlašovací údaje
```
username: admin  
password: 123456