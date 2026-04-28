# RitmusShop Készletkezelő

Asztali alkalmazás a RitmusShop webáruház termékkészletének gyors, tömeges kezeléséhez.

---

## Tartalomjegyzék

1. [Projektleírás](#1-projektleírás)
2. [Funkciók](#2-funkciók)
3. [Technológiai stack és döntések indoklása](#3-technológiai-stack-és-döntések-indoklása)
4. [Architektúra](#4-architektúra)
5. [Projekt struktúra](#5-projekt-struktúra)
6. [Környezet beállítása](#6-környezet-beállítása)
7. [Konfiguráció](#7-konfiguráció)
8. [Használat](#8-használat)
9. [API integráció](#9-api-integráció)
10. [Adatmodellek](#10-adatmodellek)

---

## 1. Projektleírás

A **RitmusShop Készletkezelő** egy Windows asztali alkalmazás, amelyet kifejezetten a RitmusShop webáruház termékei készletszintjeinek hatékony, tömeges módosítására terveztünk. Az alkalmazás közvetlenül a Hotcakes e-kereskedelmi rendszer API-jával kommunikál, így valós idejű betekintést és szerkesztési lehetőséget biztosít a raktárkészletbe.

### Miért született ez az alkalmazás?

A webáruházi adminisztrációs felület termékenkénti készletmódosítása időigényes és körülményes, különösen akkor, ha sok terméket és méretet kell egyszerre frissíteni (pl. új áru érkezésekor vagy leltár után). Ez az alkalmazás lehetővé teszi, hogy egyetlen delta értékkel (pl. `+50`) egyszerre több száz termék vagy méretvariáns készletét frissítsük.

---

## 2. Funkciók

| Funkció | Leírás |
|---|---|
| **Kategória-navigáció** | Főkategóriák hierarchikus böngészése |
| **Terméklista** | Kategóriánkénti termékek megjelenítése lapozással |
| **Termék keresés** | Szűrés terméknév vagy cikkszám (SKU) alapján |
| **Variáns megjelenítés** | Termékek méretvariantjainak (pl. XS, S, M) külön listázása |
| **Tömeges kijelölés** | Termékek és variánsok checkbox-szal való tömeges kijelölése |
| **Delta alapú módosítás** | Egyetlen értékkel az összes kijelölt elem készlete módosítható egyszerre |
| **Eredményjelzés** | Sikeres és sikertelen frissítések száma visszajelzésként megjelenik |

---

## 3. Technológiai stack és döntések indoklása

### C# + .NET 8.0 (Windows Forms)

**Miért C# és .NET 8?**
- A RitmusShop infrastruktúrája Microsoft-alapú (Hotcakes Commerce, .NET), így a C# a természetes választás az integrációhoz.
- A .NET 8 LTS (Long-Term Support) verzió, amely 2026 novemberéig kap biztonsági frissítéseket.
- A `Hotcakes.CommerceDTO` könyvtár NuGet csomagként érhető el .NET-hez.

**Miért Windows Forms (WinForms)?**
- A WinForms gyorsan fejleszthető, a designer vizuálisan szerkeszthető, ami rövid fejlesztési időt tesz lehetővé.


### Newtonsoft.Json

**Miért Newtonsoft.Json**
- A `Hotcakes.CommerceDTO` modellek egy része olyan JSON-szerkezetet használ, amelyet a `Newtonsoft.Json` rugalmasan kezel (pl. null tolerancia, polymorphic deszerializáció).
- A Newtonsoft az iparban bevett, stabilan dokumentált könyvtár.

### Microsoft.Extensions.Configuration.Json

**Miért nem hardkódolt konfiguráció?**
- Az API kulcs és a szerver URL érzékeny adat. Konfigurációs fájlba szervezve könnyen cserélhető anélkül, hogy a forráskódot módosítani kellene.
- Az `appsettings.json` megközelítés ugyanaz a minta, amit az ASP.NET Core alkalmaz — ismerős, egységes, `.gitignore`-ral védhető.

---

## 4. Architektúra

Az alkalmazás egy háromrétegű, lazán MVVM-ihlette struktúrát követ:

```
                                                        ┌─────────────────────────────────────────────┐
                                                        │                  UI réteg                   │
                                                        │  MainForm  │  ProductListItem  │  VariantListItem  │
                                                        └────────────────────┬────────────────────────┘
                                                                             │ ViewModel-eken keresztül
                                                        ┌────────────────────▼────────────────────────┐
                                                        │               ViewModel réteg               │
                                                        │     InventoryItemViewModel, VariantViewModel│
                                                        └────────────────────┬────────────────────────┘
                                                                             │ IHotcakesApiService-en át
                                                        ┌────────────────────▼────────────────────────┐
                                                        │              Szolgáltatás réteg             │
                                                        │           HotcakesApiService                │
                                                        │         (HTTP kérések, JSON parse)          │
                                                        └────────────────────┬────────────────────────┘
                                                                             │ API
                                                        ┌────────────────────▼────────────────────────┐
                                                        │         Hotcakes Commerce API               │
                                                        │              (külső szerver)                │
                                                        └─────────────────────────────────────────────┘
```

### Kulcsdöntések az architektúrában

- **Interface-alapú szolgáltatás (`IHotcakesApiService`)**: Az interfész lehetővé teszi, hogy szükség esetén a valódi API-hívást tesztre vagy mock implementációra cseréljük.
- **Async/Await mindenhol**: Minden API-hívás aszinkron, hogy a UI ne fagyjon le adatlekérés közben.
- **`Task.WhenAll` párhuzamos betöltés**: Egy termék variánsait, készletét és opcióit egyszerre kéri le a rendszer, nem sorban — ez jelentősen csökkenti a betöltési időt sok terméknél.
- **Centralizált téma (`UiTheme`)**: Minden szín és betűtípus egyetlen osztályban van definiálva, így a vizuális megjelenés könnyen módosítható.

---

## 5. Projekt struktúra

```
                            RitmusShop_keszletkezelo/
                            │
                            ├── RitmusShop_keszletkezelo.sln          # Visual Studio solution fájl
                            │
                            └── RitmusShop_keszletkezelo/             # Fő projekt könyvtár
                                │
                                ├── Program.cs                        # Belépési pont, konfiguráció betöltése
                                ├── MainForm.cs                       # Fő ablak logikája
                                ├── MainForm.Designer.cs              # Fő ablak UI leíró (auto-generált)
                                │
                                ├── ProductListItem.cs                # Termék kártya UserControl
                                ├── ProductListItem.Designer.cs
                                │
                                ├── VariantListItem.cs                # Méret/variáns sor UserControl
                                ├── VariantListItem.Designer.cs
                                │
                                ├── UiTheme.cs                        # Centralizált színek és betűtípusok
                                │
                                ├── services/
                                │   ├── IHotcakesApiService.cs        # API szolgáltatás interfész
                                │   └── HotcakesApiService.cs         # API kliens implementáció
                                │
                                ├── ViewModels/
                                │   └── InventoryItemViewModel.cs     # Termék + variáns + készlet ViewModel
                                │
                                ├── appsettings.json                  # Konfiguráció (NEM kerül git-be)
                                ├── appsettings.example.json          # Konfiguráció sablon
                                └── RitmusShop_keszletkezelo.csproj   # .NET projekt fájl
```

---

## 6. Környezet beállítása

### Előfeltételek

| Eszköz | Minimális verzió | Letöltés |
|---|---|---|
| Windows | 10 (x64) | — |
| .NET SDK | 8.0 | https://dotnet.microsoft.com/download/dotnet/8.0 |
| Visual Studio | 2022 (v17.8+) | https://visualstudio.microsoft.com/ |
| Git | Bármely | https://git-scm.com/ |

> A Visual Studio telepítésekor a **".NET Desktop Development"** workload szükséges.

### Lépések

**1. Repository klónozása**

```bash
git clone https://github.com/szagi11/ritmusshop.git
cd RitmusShop_keszletkezelo
```

**2. Konfiguráció létrehozása**

Másold le a sablont, és töltsd ki az értékeket:

```bash
cp RitmusShop_keszletkezelo/appsettings.example.json RitmusShop_keszletkezelo/appsettings.json
```

Ezután nyisd meg az `appsettings.json` fájlt és add meg az adatokat (lásd a [Konfiguráció](#7-konfiguráció) fejezetet).


---

## 7. Konfiguráció

Az alkalmazás az `appsettings.json` fájlból olvassa be a beállításait. Ez a fájl **nem kerül verziókezelésbe** (`.gitignore`-ban szerepel), mert érzékeny adatokat tartalmaz.
Emiatt a már említett módon a verzókövetett appsettings.example.json fájlt át kell mósolni és átnevezni appsettings.json-re, majd meg kell adni a tényleges URL-t és API kulcsot!

#### Nagyon figyelni kell rá, hogy az appsettings.example.json-ben semmiképpen sem szabad az URL-t és az API kulcsot beírni hiszen ez verziőkövetve van. Emellett az appsettings.json-t sose commitold git-be!!!

```json
{
  "HotcakesApi": {
    "BaseUrl": "http://your-server-url/",
    "ApiKey": "YOUR-API-KEY-HERE"
  }
}
```

| Kulcs | Leírás | 
|---|---|
| `HotcakesApi:BaseUrl` | A Hotcakes Commerce szerver alap URL-je (záró perjellel) | 
| `HotcakesApi:ApiKey` | API hitelesítési kulcs | 

> **Figyelem**: Ha az `appsettings.json` hiányzik, vagy a `BaseUrl` / `ApiKey` üres, az alkalmazás hibaüzenettel kilép indításkor.

Az API kulcs minden kéréshez automatikusan hozzáfűződik query paraméterként:

```
GET http://your-server/DesktopModules/Hotcakes/API/rest/v1/categories/?key=YOUR-API-KEY
```

---

## 8. Használat

### Kategória-navigáció

1. Az alkalmazás indításakor a bal oldali panel betölti az összes főkategóriát.
2. Kattints egy kategória gombra — az alkategóriák megjelennek a lenyíló listában.
3. Válassz alkategóriát a termékek betöltéséhez, vagy maradj a szülőkategóriánál.

### Termékek böngészése

- Minden termék egy kártyán jelenik meg a nevével, cikkszámával és összesített készletével.
- A kártya kibontható a méretvariantok (pl. XS / S / M / L / XL, vagy 38, 39, stb.) megjelenítéséhez.

### Keresés

- A kategória panel mellett felül a keresőmezőbe írva a termékek azonnal szűrődnek név vagy SKU alapján.

### Tömeges készletmódosítás

1. Jelöld ki a módosítandó termékeket/variánsokat a checkboxok segítségével.
   - Termék szintű checkbox: az összes variánst kijelöli.
   - Variáns szintű checkbox: csak azt az egy méretet jelöli ki.
2. Az alsó vezérlősávban add meg a delta értéket (pl. `10` hozzáadáshoz, `-5` csökkentéshez).
3. Kattints az **"Alkalmaz"** gombra.
4. Az alkalmazás minden kijelölt elem készletét frissíti, és összesítő visszajelzést ad (pl. "15 sikeres, 0 sikertelen").

> **Megjegyzés**: A készlet nem csökkenthető 0 alá. A rendszer automatikusan megtagadja az olyan frissítést, amely negatív értéket eredményezne.

---

## 9. API integráció

Az alkalmazás a Hotcakes Commerce REST API v1-et használja. Az összes endpoint a `/DesktopModules/Hotcakes/API/rest/v1/` alap útvonalon érhető el.

### Végpontok

| Metódus | Útvonal | Funkció |
|---|---|---|
| `GET` | `/categories/` | Összes kategória lekérése |
| `GET` | `/products/?bycategory={bvin}&page={n}&pagesize={n}` | Kategóriához tartozó termékek (lapozva) |
| `GET` | `/productvariant/?productid={bvin}` | Termék variánsai |
| `GET` | `/productinventory/?byproduct={bvin}` | Termék készletei |
| `GET` | `/productoptions/?productbvin={bvin}` | Termék opciók (méretek nevei) |
| `POST` | `/productinventory/{bvin}` | Készlet frissítése |

### Válaszformátum

Minden válasz egy egységes borítékba van csomagolva:

```json
{
  "Content": { ... },
  "Errors": []
}
```

Az `ApiResponseEnvelope<T>` generikus osztály kezeli a deszerializációt és a hibák ellenőrzését.

---

## 10. Adatmodellek

### InventoryItemViewModel

Egy termék összes releváns adatát összefogó ViewModel:

```csharp
class InventoryItemViewModel
{
    string ProductBvin          // Egyedi azonosító (GUID)
    string ProductName          // Termék neve
    string Sku                  // Cikkszám
    ProductInventoryDTO MainInventory  // Fő készletrekord
    List<VariantViewModel> Variants    // Méretvariantok listája
    int TotalQuantityOnHand     // Összes variáns készletének összege
    bool IsSelected             // Tömeges műveletnél ki van-e jelölve
}
```

### VariantViewModel

Egyetlen méret/variáns adatai:

```csharp
class VariantViewModel
{
    string VariantBvin          // Egyedi azonosító
    string Sku                  // Variáns cikkszáma
    string DisplayName          // Megjelenítési név (pl. "Méret: XL")
    ProductInventoryDTO Inventory  // Készletrekord
    bool IsSelected
}
```

---

## Fejlesztői megjegyzések

- A `UiTheme.cs` az egyetlen forrása minden vizuális konstansnak — ha a kinézetet szeretnéd módosítani, ott kezd.
- Az `IHotcakesApiService` interfész lehetővé teszi a service kicserélését tesztelés vagy más API esetén.
- A `SafeGetOptionsAsync` és `SafeGetCategoriesForProductAsync` metódusok toleránsan kezelik az API hibákat: üres listával térnek vissza hiba esetén, így egy termék betöltési hibája nem akadályozza meg a többi termék megjelenítését.

## AI-asszisztált fejlesztés

A projekt fejlesztése során több AI-eszközt használtam **társfejlesztőként
(co-creation)**. Az idő szűke és a technológiában rejlő lehetőségek kiaknázás érdekében az AI használata elengedhetetlen volt. 

A munkafolyamat jellemzően így nézett ki:

1. **Specifikáció** — Megfogalmaztam mit szeretnék elérni, részletes kontextust és leírást adtam, így elérve a minél pontosabb választ
2. **Javaslat** — az AI konkrét kódot javasolt, gyakran több verziót, vagy alternatívát mérlegelve.
3. **Felülvizsgálat** — átolvastam a kódot, megértettem mit csinál és értékeltem, hogy illik-e a meglévő architektúrához.
4. **Adaptáció** — szükség esetén módosítottam, átszerveztem, vagy visszadobtam a javaslatot, és új irányt kértem.
5. **Integráció** — beillesztettem a saját kódbázisomba, teszteltem és iteráltam, ha hibára futottam.

Fontos kiemelni, hogy a kódolást és a tervezést megelőzően egy átfogó technikai elemzést végeztem a windows forms app-hez, amiben megvizsgáltam milyen lehetőségek rejlenek a technológiában, hogyan érdemes hozzákezdeni a tervezéshez, majd a kódoláshoz. Ezt követően elkezdtem az alábbi munkafolyamatot körkörösen alkalmazni egészen addig amíg el nem készültem!  

### Felhasznált eszközök

| Eszköz | Mire használtam | Felhasználás aránya |
|---|---|---|
| **Claude Opus 4.7 (web)** | Architekturális döntések, hosszabb refaktorok, dokumentáció, design-iterációk, hibák diagnosztizálása több fájlon átívelően | Domináns |
| **Claude Sonnet 4.6 (CLI)** | Egyedi fájlok átszervezése, kisebb módosítások a terminálból | Alkalmankénti |
| **Gemini 3** | Második vélemény nehezebb tervezési döntéseknél, alternatív megoldások felmérése | Alkalmankénti |
| **GPT-5.2-Codex** | Specifikus kódrészletek finomítása, kódstílus-ellenőrzés | Ritka |

#### Az eszközök használatba vétele

**Claude Opus 4.7 (web)**

A claude.ai webes felületen elérhető. Anthropic fiók szükséges hozzá
(ingyenes regisztrációval, vagy fizetős Pro előfizetéssel a magasabb
limitekért). A használat menete:

1. Megnyitom a [claude.ai](https://claude.ai) oldalt böngészőben.
2. Új beszélgetést indítok ("New chat").
3. A chat ablakba beírom a kérésemet — gyakran több bekezdéses
   leírással, és ha kell, fájlokat is csatolok (kódot, screenshotokat).
4. Claude válaszában a javasolt kódot, magyarázatokat, vagy diagnózist olvasom.
5. Iteratívan finomítom a kérést, ha szükséges.

A web verzió előnye, hogy hosszabb beszélgetéseket tud kezelni, és
fájlokat (képeket, szövegeket) is tud értelmezni.

**Claude Sonnet 4.6 (CLI — Claude Code)**

Az Anthropic által fejlesztett parancssori eszköz, ami a fejlesztői
környezetben (terminál) közvetlenül a kódbázisra tud dolgozni. A telepítés
és használat menete:

1. Telepítés: `irm https://claude.ai/install.ps1 | iex`
2. Hitelesítés a fiókkal: `claude login` parancs, ami böngészőben
   megnyitja az authentikációs oldalt.
3. A projekt mappájából indítva: `claude` parancs elindítja az
   interaktív munkamenetet.
4. A CLI látja a projekt fájljait, így konkrét fájlokra vagy
   módosításokra lehet kérni — pl. "olvasd el a `MainForm.cs`-t és
   javítsd a layout problémát". Ezzel a megoldással sokkal pontosabb kódot kapunk!

Előnye: közvetlenül a fájlokon dolgozik, nem kell másolgatni a kódot
oda-vissza. Hátránya: A CLI sokaknak kicsit nehezen olvasható, nem olyan szép a design. 

**Gemini 3**

A Google AI eszköze, a [gemini.google.com](https://gemini.google.com)
oldalon érhető el Google fiókkal. Hasonlóan a Claude web verzióhoz:

1. Megnyitom a Gemini oldalát.
2. Új beszélgetést indítok.
3. Beírom a kérdést vagy felteszem a problémát.
4. Általában a dokumentáláshoz és egyéb kisebb architekturális kérdésekben és dokumentum elemzéshez és a git megértéshez használtam

**GPT-5.2-Codex (Visual Studio chat módban)**

A Visual Studio 2022 IDE-be integrált AI asszisztens, ami a kódszerkesztő
oldalsó paneljéből érhető el. A használat menete:

1. A Visual Studio jobb felső sarkában a chat ikonra kattintok.
2. Beírom a kérdést, gyakran kiválasztott kódrészlettel együtt.
3. A válasz azonnal megjelenik az IDE-n belül, és néhány esetben
   közvetlenül beilleszthető a szerkesztett fájlba.

### Mire használtam az AI-t

- **Hotcakes API integráció**: a gyári DLL elemzése és megértéséhez!
- **GUID normalizálás megoldása**: a Hotcakes API kötőjelekkel és kötőjelek
  nélkül is visszaadja ugyanazt a GUID-ot. Ennek diagnosztizálása és kezelése
  iteratív munka volt az AI-val.
- **Variáns-méretek feloldása**: az `OptionDTO.Items` lookup logika kidolgozása,
  beleértve az `IsLabel=true` placeholder-ek kiszűrését.
- **WinForms layout-ok**: a kártyás design, a kibontható panelek, a kijelölési
  vizuális visszajelzés implementálása.
- **Refaktorálás tesztelhetőség érdekében**: a `HotcakesApiService` interface
  mögé húzása, a `HttpClient` kívülről történő átadása.
- **Dokumentáció**: a README és a kódkommentek megfogalmazásának finomítása.

### Mit NEM csinált az AI

- **Nem hozta meg a tervezési döntéseket** — minden architekturális
  döntésnél (pl. "kibontás kártyán belül vagy popupban?", "MVP refaktor vagy
  egyszerűbb kiszervezés?") én döntöttem, az AI csak az opciókat tárta fel
  és a következményekre figyelmeztetett. 
- **Nem futtatta a kódot** — minden tesztelést, hibadiagnózist, build-validációt
  én végeztem helyileg.
- **Nem ismerte a Hotcakes adatbázis valós tartalmát** — minden adat-specifikus
  problémát (pl. üres SKU-k, hiányzó méret-hozzárendelések) én tártam fel és
  értelmeztem, az AI csak a megoldási mintákat javasolta.
- **Nem írt teljes funkciókat felügyelet nélkül** — minden javaslatot
  átolvastam, mielőtt beillesztettem volna.

### Tanulságok

Az AI **gyorsít**, de **nem helyettesíti a programozói döntéseket**. A
projekt során több olyan helyzet volt, amikor az AI első javaslata hibás
vagy egyáltalán nem volt optimális. Nem ismerte minden esetben a windows forms app-ben rejlő lehetőségeket, így bizonyos design-al kapcsolatos problémák megoldásában egyáltalán nem tudott segíteni.

A leghatékonyabb használat az volt, amikor **konkrét, kontextusban gazdag
kéréseket** fogalmaztam meg: nem csak azt mondtam "javítsd ezt", hanem
elmondtam, mit látok, mit várnék, és milyen architekturális elvárásaim
vannak. Az AI ekkor tudott valóban hasznos, kivitelezhető javaslatokat adni.