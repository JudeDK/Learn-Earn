# Sprint 2 — Changes Added

Acest fișier listează modificările aduse proiectului pentru Sprintul 2 și starea curentă a cerințelor Sprint 1 și Sprint 2.

**Rezumat scurt**
- Sprint 1: implementat (autentificare / înregistrare / profil via Identity scaffold).
- Sprint 2: parțial implementat — funcționalitățile de creare, listare, filtrare și paginare a lecțiilor sunt adăugate; upload atașamente și unele îmbunătățiri UI rămân de implementat.

**Fișiere noi / modificate (Sprint 2)**
- `Models/Lesson.cs` : modelul `Lesson` (Titlu, Limbă, Dificultate, Durată, Conținut, CreatedByUserId, CreatedAt).
- `Data/ApplicationDbContext.cs` : adăugat `DbSet<Lesson> Lessons`.
- `Pages/Lectii/Index.cshtml` : pagină de listare a lecțiilor cu filtre (`Search`, `Language`, `Difficulty`) și control UI pentru paginare.
- `Pages/Lectii/Index.cshtml.cs` : logică server-side pentru listare, filtrare și paginare (`Page`, `PageSize`, `TotalPages`, Skip/Take).
- `Pages/Lectii/Create.cshtml` : pagină de creare lecție (formular cu titlu, limbă, dificultate, durată, conținut) — protejată la nivel de pagină cu `[Authorize(Roles = "Profesor,Admin")]`.
- `Pages/Lectii/Create.cshtml.cs` : handlerul POST pentru salvarea lecției în BD (setează `CreatedByUserId`).
- `Pages/Lectii/Details.cshtml` : vizualizare lecție individuală.
- `Pages/Lectii/Details.cshtml.cs` : handler pentru afișarea unei lecții după `id`.

**Modificări de configurare relevante**
- `Program.cs` : s-a păstrat configurația implicită Identity (`AddDefaultIdentity`) — nu se mai suprascrie hasher-ul. Am revenit la comportamentul original pentru `Register`/`Account`.
- `Learn&Earn.csproj` : am eliminat referința la `BCrypt.Net-Next` pentru a preveni erori de salt.

**Ce e complet (Sprint 1)**
- Înregistrare utilizator (scaffold Identity) — `Areas/Identity/Pages/Account/Register` (formular + validări).
- Autentificare / Login — `Areas/Identity/Pages/Account/Login`.
- Vizualizare profil / pagini Identity (scaffold default).
- Stocare parole folosind hasher-ul implicit Identity (PBKDF2 etc.) — nu se folosește BCrypt în runtime.

**Ce e implementat pentru Sprint 2 (done)**
- Model `Lesson` și `DbSet<Lesson>` în `ApplicationDbContext`.
- Pagina de listare a lecțiilor cu filtre simple (`Search`, `Language`, `Difficulty`).
- Paginare server-side (parametru `Page`, `PageSize` = 10, prev/next și numerotare în UI).
- Pagina de creare lecție (formular + validări DataAnnotations). Protejată la nivel de rol cu atribut `[Authorize(Roles = "Profesor,Admin")]`.
- Pagina de detalii lecție (vizualizare conținut, limbă, dificultate, durată).

**Ce lipsește / recomandări (Sprint 2 — de implementat)**
- Upload și gestionare atașamente (imagine, PDF) pentru lecții (`FileService`, stocare, link de descărcare) — corespunde `US 2.5` și `US 5.3`.
- Optimizări UI: navigație principală îmbunătățită, widget progres (nu fac parte din paginile Razor curente).
- Creare/assign rol `Profesor` în aplicație (dacă vrei roluri pre-seedate va trebui un script de seed sau interfață admin) — momentan Identity e la starea default fără seed automat.
- Testare: teste manuale pentru fluxul profesor (creare lecție) și testele unitare (ex: hashing, autentificare) nu au fost adăugate.

**Comenzi utile (Migrații & rulare)**
1. Generează migrații și aplică baza de date (în folderul proiectului):
```powershell
dotnet tool install --global dotnet-ef --version 8.0.11
dotnet ef migrations add AddLessons
dotnet ef database update
```

2. Rulează aplicația:
```powershell
dotnet run
```

3. Testări rapide în browser:
- `https://localhost:5051/Identity/Account/Register` — înregistrează un user nou.
- Loghează-te și accesează `https://localhost:5051/Lectii` pentru listare.
- Ca user cu rol `Profesor` sau `Admin` (dacă ai setat manual rolurile) poți accesa `https://localhost:5051/Lectii/Create`.

Dacă dorești, pot:
- implementa upload atașamente și stocare fișiere (Passenger: `wwwroot/uploads` sau stocare externă),
- adăuga seed pentru roluri și un cont `Profesor`/`Admin` (pe care le poți accepta),
- crea teste unitare xUnit pentru funcțiile critice,
- genera un `README.md` sumar cu instrucțiuni complete.

---
Fă-mi semn ce din lista "Ce lipsește" vrei să implementez acum și continui imediat.
