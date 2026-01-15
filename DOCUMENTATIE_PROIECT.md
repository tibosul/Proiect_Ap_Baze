# VolunteerSystem - Documentație Completă a Proiectului

## Cuprins
1. [Prezentare Generală](#prezentare-generală)
2. [Arhitectura Aplicației](#arhitectura-aplicației)
3. [Baza de Date](#baza-de-date)
4. [Fluxul Aplicației](#fluxul-aplicației)
5. [Funcționalități pe Roluri](#funcționalități-pe-roluri)
6. [Cazuri de Utilizare Detaliate](#cazuri-de-utilizare-detaliate)
7. [Tehnologii Utilizate](#tehnologii-utilizate)

---

## Prezentare Generală

**VolunteerSystem** este o aplicație desktop complexă pentru gestionarea voluntarilor și oportunităților de voluntariat, construită cu **.NET 10.0**, **Avalonia UI** (framework cross-platform pentru interfață grafică) și **SQL Server** ca bază de date.

### Scopul Aplicației
Aplicația conectează trei tipuri de utilizatori:
- **Voluntarii** care caută oportunități de a ajuta
- **Organizatorii** care creează și gestionează evenimente de voluntariat
- **Administratorii** care supraveghează întregul sistem

### Caracteristici Principale
- Sistem complet de autentificare și înregistrare
- Gestionarea oportunităților și evenimentelor de voluntariat
- Sistem de aplicare și aprobare pentru evenimente
- Sistem de puncte și clasament pentru voluntari
- Chat direct între utilizatori
- Feedback pentru evenimente
- Rapoarte și statistici
- Recomandări personalizate de oportunități

---

## Arhitectura Aplicației

Proiectul urmează o arhitectură **în straturi (layered architecture)** cu separare clară a responsabilităților:

### 1. **VolunteerSystem.Core** (Stratul de Logică de Bază)
Conține definițiile fundamentale ale aplicației:

#### Entities (Entități)
- `User` (abstract) - Clasa de bază pentru toți utilizatorii
  - `Volunteer` - Voluntar cu nume, skills, puncte
  - `Organizer` - Organizator cu nume organizație, descriere
  - `Admin` - Administrator
- `Opportunity` - Oportunitate de voluntariat
- `Event` - Eveniment specific în cadrul unei oportunități
- `Application` - Aplicare a unui voluntar la un eveniment
- `Feedback` - Review după participarea la un eveniment
- `ChatMessage` - Mesaj între utilizatori
- `PointsTransaction` - Tranzacție de puncte (câștigate sau pierdute)
- `Report` - Raport/reclamație despre conținut problematic

#### Interfaces (Interfețe pentru Servicii)
- `IAuthenticationService` - Autentificare și înregistrare
- `IUserService` - Gestionarea profilurilor utilizatorilor
- `IOpportunityService` - Operații cu oportunități și aplicări
- `IChatService` - Mesagerie între utilizatori
- `IPointsService` - Gestionarea punctelor voluntarilor
- `IReportService` - Generare rapoarte și statistici

#### Helpers
- `PasswordHelper` - Hash și verificare parole (SHA256)

### 2. **VolunteerSystem.Data** (Stratul de Acces la Date)
Implementează accesul la baza de date folosind **Entity Framework Core**:

#### ApplicationDbContext
Contextul EF Core care mapează entitățile la tabelele din SQL Server, cu configurări pentru relațiile dintre entități.

#### Services (Implementări)
- `AuthenticationService` - Login, register pentru voluntari și organizatori
- `UserService` - Update profile, get users, delete users
- `OpportunityService` - CRUD oportunități, aplicare la evenimente, feedback
- `ChatService` - Trimitere și primire mesaje
- `PointsService` - Adăugare puncte, istoric tranzacții
- `ReportService` - Generare rapoarte pentru organizatori și admin

### 3. **VolunteerSystem.Avalonia** (Stratul de Prezentare - UI)
Interfața grafică construită cu Avalonia UI folosind pattern-ul **MVVM (Model-View-ViewModel)**:

#### ViewModels
Logica de prezentare și legătura cu serviciile:
- `MainViewModel` - Container principal care schimbă view-urile
- `LoginViewModel` - Autentificare
- `RegistrationViewModel` - Înregistrare utilizatori noi
- `VolunteerDashboardViewModel` - Panou de control voluntar
- `OrganizerDashboardViewModel` - Panou de control organizator
- `AdminDashboardViewModel` - Panou de control administrator
- `VolunteerProfileViewModel` / `OrganizerProfileViewModel` - Profiluri
- `ChatViewModel` - Interfață chat
- `FeedbackViewModel` - Trimitere feedback
- `LeaderboardViewModel` - Clasament voluntari
- `ReportViewModel` - Vizualizare rapoarte
- `EditOpportunityViewModel` - Creare/editare oportunități

#### Views (fișiere .axaml)
Interfața grafică XML-based pentru fiecare ViewModel.

#### Program.cs și App.axaml.cs
- Inițializare aplicație
- Configurare dependency injection (DI)
- Conectare la baza de date
- Rulare auto-migrații pentru schema DB

### 4. **VolunteerSystem.UI** (WPF - Alternativă)
O implementare alternativă folosind WPF (Windows Presentation Foundation), similar cu Avalonia dar specific pentru Windows.

---

## Baza de Date

Aplicația folosește **SQL Server** cu o schemă complexă și bine structurată.

### Structura Tabelelor Principale

#### 1. **Autentificare și Roluri**

**Users**
```sql
- Id (PK)
- Email (unique)
- PasswordHash
- Status (Active/Suspended/PendingApproval)
- IsEmailConfirmed
- CreatedAt
- LastLoginAt
```

**Roles**
```sql
- Id (PK)
- Name (Volunteer/Organizer/Admin)
```

**UserRoles** (Many-to-Many)
```sql
- UserId (FK)
- RoleId (FK)
```

#### 2. **Profile Separate**

**VolunteerProfiles**
```sql
- Id (FK to Users) (PK)
- FullName
- Phone
- Bio
- Skills (text)
- Points
- City, Lat, Lng (locație)
```

**OrganizerProfiles**
```sql
- Id (FK to Users) (PK)
- OrganizationName
- OrganizationDescription
- City, Lat, Lng
- IsVerified
```

**AdminProfiles**
```sql
- Id (FK to Users) (PK)
```

#### 3. **Oportunități și Evenimente**

**Opportunities**
```sql
- Id (PK)
- OrganizerId (FK)
- Title
- Description
- RequiredSkills
- Location
- Points (puncte acordate)
- City, Lat, Lng
- TotalSlots
- ApplicationDeadline
- Status (Draft/Published/Archived/Deleted)
- CreatedAt, UpdatedAt
```

**Events**
```sql
- Id (PK)
- OpportunityId (FK)
- StartTime
- EndTime
- MaxVolunteers
- IsCompleted
```

**Relația:** O oportunitate poate avea **multiple evenimente** (de exemplu: "Curățare parc" are evenimente în diferite zile).

#### 4. **Aplicări și Prezență**

**Applications**
```sql
- Id (PK)
- EventId (FK)
- VolunteerId (FK)
- Status (Pending/Approved/Rejected/Withdrawn)
- AppliedAt
- DecidedAt
- DecidedByOrganizerId (FK)
```

**EventAttendance**
```sql
- Id (PK)
- EventId (FK)
- VolunteerId (FK)
- Status (Present/Absent/Excused)
- MarkedAt
- MarkedByOrganizerId (FK)
```

#### 5. **Feedback și Puncte**

**Feedbacks**
```sql
- Id (PK)
- EventId (FK)
- VolunteerId (FK)
- Rating (1-5)
- Comment
- CreatedAt
```

**PointsTransactions**
```sql
- Id (PK)
- VolunteerId (FK)
- EventId (FK, nullable)
- Points (pozitiv sau negativ)
- Reason (CompletedEvent/Bonus/Penalty)
- CreatedAt
```

#### 6. **Skills și Interese (pentru Recomandări)**

**Skills**
```sql
- Id (PK)
- Name (unique)
```

**Interests**
```sql
- Id (PK)
- Name (unique)
```

**VolunteerSkills** (Many-to-Many)
```sql
- VolunteerId (FK)
- SkillId (FK)
```

**VolunteerInterests** (Many-to-Many)
```sql
- VolunteerId (FK)
- InterestId (FK)
```

**OpportunitySkills** / **OpportunityInterests**
Similar pentru a lega oportunități de skills/interese necesare.

#### 7. **Chat**

**ChatMessages**
```sql
- Id (PK)
- SenderId (FK to Users)
- ReceiverId (FK to Users)
- Content
- Timestamp
```

#### 8. **Rapoarte/Reclamații**

**Reports**
```sql
- Id (PK)
- ReporterId (FK to Users)
- TargetType (Opportunity/User/Message/Event)
- TargetId
- ReasonType (Fake/Abuse/Spam/Other)
- Description
- Status (Open/InReview/Resolved/Rejected)
- ResolvedByAdminId (FK)
- ResolvedAt
- CreatedAt
```

#### 9. **Echipe (per Event)**

**Teams**
```sql
- Id (PK)
- EventId (FK)
- Name
- Instructions
```

**TeamMembers** (Many-to-Many)
```sql
- TeamId (FK)
- VolunteerId (FK)
```

#### 10. **Security și Audit**

**PasswordResetTokens**
```sql
- Id (PK)
- UserId (FK)
- TokenHash
- ExpiresAt
- UsedAt
- CreatedAt
```

**AdminActions**
```sql
- Id (PK)
- AdminUserId (FK)
- TargetUserId (FK)
- ActionType (Approve/Suspend/Unsuspend/ResetPassword)
- Reason
- CreatedAt
```

### Date Inițiale (Seed Data)
La setup, se creează automat 3 utilizatori de test:
- **admin@test.com** (Administrator) - Password: `Password123!`
- **volunteer@test.com** (Voluntar) - Password: `Password123!`
- **organizer@test.com** (Organizator) - Password: `Password123!`

---

## Fluxul Aplicației

### 1. **Pornirea Aplicației**

```
Program.Main() 
  → App.Initialize()
  → App.OnFrameworkInitializationCompleted()
     → Citește appsettings.json pentru connection string
     → Creează DbContext cu SQL Server
     → Rulează migrații automate pentru schema DB
     → Inițializează serviciile (DI manual)
     → Creează LoginViewModel și îl setează ca view curent
     → Afișează MainWindow
```

### 2. **Autentificare**

```
Utilizator intră email/password în LoginView
  → LoginViewModel.LoginCommand
     → AuthenticationService.LoginAsync(email, password)
        → Query în Users table
        → Verifică hash-ul parolei (SHA256)
        → Dacă valid, returnează obiect User (Volunteer/Organizer/Admin)
     → Determină tipul utilizatorului (pattern matching)
     → Creează ViewModel-ul corespunzător dashboard-ului
     → Setează MainViewModel.CurrentView la noul dashboard
```

### 3. **Înregistrare**

```
Utilizator click "Register" în LoginView
  → LoginViewModel.GoToRegisterCommand
     → Navighează la RegistrationViewModel
        → Utilizator alege rol (Volunteer/Organizer)
        → Completează formular specific
        → RegistrationViewModel.RegisterCommand
           → AuthenticationService.RegisterVolunteerAsync() SAU
             AuthenticationService.RegisterOrganizerAsync()
              → Creează User în DB cu hash parolă
              → Creează profil corespunzător în VolunteerProfiles/OrganizerProfiles
              → Navighează înapoi la Login
```

### 4. **Navigare în Aplicație**

Aplicația folosește pattern-ul de **navigare prin view switching**:
- `MainViewModel` conține proprietatea `CurrentView`
- Schimbarea `CurrentView` declanșează actualizarea UI-ului
- Fiecare ViewModel primește referința la `MainViewModel` pentru a putea naviga

---

## Funcționalități pe Roluri

### **VOLUNTAR** (Volunteer)

#### Dashboard Principal
- **Oportunități disponibile**: Listă cu toate oportunitățile publicate
- **Oportunități recomandate**: Bazate pe skills-urile voluntarului
- **Puncte totale**: Afișare punctaj curent
- **Istoric tranzacții puncte**: Toate punctele câștigate/pierdute

#### Acțiuni Disponibile

1. **Căutare Oportunități**
   ```
   VolunteerDashboardViewModel.SearchCommand
     → OpportunityService.SearchOpportunitiesAsync(query)
     → Filtrează după titlu, descriere, skills
   ```

2. **Vizualizare Detalii Oportunitate**
   - Titlu, descriere, locație
   - Skills necesare
   - Puncte acordate
   - Lista de evenimente asociate cu date/ore

3. **Aplicare la Eveniment**
   ```
   Voluntar selectează un event din oportunitate
     → OpportunityService.ApplyToEventAsync(volunteerId, eventId)
        → Verifică dacă evenimentul nu e în trecut
        → Verifică dacă nu a aplicat deja
        → Verifică dacă sunt locuri disponibile
        → Creează Application cu status Pending
   ```

4. **Vizualizare Aplicări Proprii**
   ```
   GoToApplicationsCommand
     → VolunteerApplicationsViewModel
        → OpportunityService.GetVolunteerApplicationsAsync(volunteerId)
        → Afișează: Oportunitate, Event, Status (Pending/Approved/Rejected/Withdrawn)
   ```

5. **Retragere Aplicare**
   ```
   Doar dacă status = Pending
     → OpportunityService.WithdrawApplicationAsync(applicationId)
     → Setează status la Withdrawn
   ```

6. **Trimitere Feedback după Eveniment**
   ```
   După participarea la un event (IsPresent = true)
     → FeedbackViewModel
        → Selectează rating (1-5 stele)
        → Scrie comentariu
        → OpportunityService.SubmitFeedbackAsync(feedback)
   ```

7. **Vizualizare Clasament (Leaderboard)**
   ```
   LeaderboardViewModel
     → Afișează top voluntari sortați după puncte
     → Evidențiază poziția utilizatorului curent
   ```

8. **Editare Profil**
   ```
   VolunteerProfileViewModel
     → Modifică: FullName, Skills, Phone, Bio
     → UserService.UpdateVolunteerProfileAsync()
   ```

9. **Chat cu Organizatori**
   ```
   ChatViewModel
     → Selectează organizator din listă contacte
     → ChatService.GetConversationAsync(volunteerId, organizerId)
     → ChatService.SendMessageAsync(message)
     → Mesaje afișate în ordine cronologică
   ```

10. **Logout**
    ```
    Navighează înapoi la LoginViewModel
    ```

---

### **ORGANIZATOR** (Organizer)

#### Dashboard Principal
- **Oportunități create**: Lista tuturor oportunităților proprii
- **Statistici**: Număr evenimente, aplicări, voluntari prezenți

#### Acțiuni Disponibile

1. **Creare Oportunitate Nouă**
   ```
   CreateOpportunityCommand
     → EditOpportunityViewModel (mode: Create)
        → Completează: Title, Description, RequiredSkills, Location, Points
        → Adaugă unul sau mai multe Events cu StartTime, EndTime, MaxVolunteers
        → OpportunityService.CreateOpportunityAsync(opportunity)
   ```

2. **Editare Oportunitate Existentă**
   ```
   EditOpportunityCommand(opportunity)
     → EditOpportunityViewModel (mode: Edit)
        → Modifică detalii
        → Adaugă/Șterge evenimente
        → OpportunityService.UpdateOpportunityAsync(opportunity)
   ```

3. **Ștergere Oportunitate**
   ```
   DeleteOpportunityCommand(opportunity)
     → OpportunityService.DeleteOpportunityAsync(opportunityId)
     → Cascade delete: toate Events, Applications asociate
   ```

4. **Gestionare Aplicări**
   ```
   Pentru fiecare event din oportunitate:
     → Vizualizează lista aplicanților (Applications cu status Pending)
     → Aproba Aplicare:
        → Schimbă status la Approved
        → Setează DecidedAt și DecidedByOrganizerId
     → Respinge Aplicare:
        → Schimbă status la Rejected
   ```

5. **Marcare Prezență la Eveniment**
   ```
   După ce evenimentul a avut loc:
     → Pentru fiecare aplicare aprobată
        → Marchează voluntarul ca Present/Absent
        → Creează EventAttendance record
        → Dacă Present:
           → PointsService.AwardPointsAsync(volunteerId, points, CompletedEvent, eventId)
           → Update Volunteer.Points în DB
           → Creează PointsTransaction record
   ```

6. **Vizualizare Feedback-uri**
   ```
   Pentru oportunități proprii:
     → Afișează toate feedback-urile primite de la voluntari
     → Rating mediu per oportunitate
     → Comentarii text
   ```

7. **Generare Rapoarte**
   ```
   ReportViewModel
     → ReportService.GenerateOrganizerReportAsync(organizerId)
     → Afișează:
        - Total oportunități create
        - Total evenimente
        - Total aplicări primite
        - Total voluntari prezenți
        - Total ore de voluntariat
   ```

8. **Editare Profil Organizație**
   ```
   OrganizerProfileViewModel
     → Modifică: OrganizationName, OrganizationDescription
     → UserService.UpdateOrganizerProfileAsync()
   ```

9. **Chat cu Voluntari**
   ```
   ChatViewModel
     → Similar cu voluntarul, dar poate inițiere conversații cu voluntarii care au aplicat
   ```

---

### **ADMINISTRATOR** (Admin)

#### Dashboard Principal
- **Raport sistem**: Statistici globale
  - Total utilizatori
  - Total voluntari / organizatori
  - Total oportunități
  - Total aplicări
- **Lista tuturor utilizatorilor**
- **Lista tuturor oportunităților**

#### Acțiuni Disponibile

1. **Vizualizare Utilizatori**
   ```
   AdminDashboardViewModel
     → UserService.GetAllUsersAsync()
     → Afișează listă cu Email, Tip (Volunteer/Organizer/Admin)
   ```

2. **Ștergere Utilizator**
   ```
   DeleteUserCommand(user)
     → UserService.DeleteUserAsync(userId)
     → Cascade delete: Profile, Applications, Messages, etc.
     → Verificare: nu poate șterge propriul cont
   ```

3. **Gestionare Rapoarte/Reclamații**
   ```
   (Dacă implementat complet)
     → Vizualizează toate Reports cu status Open
     → Investighează conținut raportat
     → Marchează ca Resolved/Rejected
     → Poate lua acțiuni: suspendare user, ștergere oportunitate
   ```

4. **Suspendare/Deblocare Utilizator**
   ```
   (Bazat pe schema DB, AdminActions table)
     → Schimbă User.Status la Suspended/Active
     → Înregistrează action în AdminActions pentru audit
   ```

5. **Vizualizare Audit Trail**
   ```
   Verifică AdminActions table
     → Istoricul tuturor acțiunilor administrative
     → Cine a făcut ce și când
   ```

6. **Generare Raport Sistem Global**
   ```
   ReportService.GenerateSystemReportAsync()
     → Statistici comprehensive despre întregul sistem
   ```

---

## Cazuri de Utilizare Detaliate

### Cazul 1: Voluntar Se Înscrie și Participă la un Eveniment

**Pași:**

1. **Înregistrare**
   ```
   User nou → Register → Selectează "Volunteer"
   → Completează: Email, Password, Full Name, Skills
   → Click Register
   → AuthenticationService.RegisterVolunteerAsync()
      → INSERT în Users (Email, PasswordHash)
      → INSERT în VolunteerProfiles (Id, FullName, Skills, Points=0)
      → INSERT în UserRoles (UserId, RoleId=1 pentru Volunteer)
   → Success → Redirect la Login
   ```

2. **Autentificare**
   ```
   → Login cu email/password
   → AuthenticationService.LoginAsync()
   → Query User + verificare hash
   → Navighează la VolunteerDashboardViewModel
   ```

3. **Căutare Oportunitate**
   ```
   → Dashboard afișează liste oportunități
   → Opțional: search bar pentru filtrare
   → Voluntar găsește "Curățare Parc" cu 15 puncte
   ```

4. **Vizualizare Detalii și Aplicare**
   ```
   → Click pe oportunitate
   → Vede: Descriere, Skills: "Munca fizică", Locație: "Parcul Central"
   → Vede Event: Sâmbătă 20 Ian 2026, 10:00-14:00, Max 20 voluntari
   → Click "Apply"
   → OpportunityService.ApplyToEventAsync(volunteerId, eventId)
      → Verificări (nu e trecut, nu a aplicat deja, sunt locuri)
      → INSERT în Applications (EventId, VolunteerId, Status=Pending, AppliedAt=now)
   → Mesaj success: "Aplicare trimisă!"
   ```

5. **Organizator Aprobă Aplicarea**
   ```
   → Organizer login
   → Dashboard → Vede "Curățare Parc" cu 1 aplicare nouă (Pending)
   → Click "Approve"
      → UPDATE Applications SET Status=Approved, DecidedAt=now, DecidedByOrganizerId=orgId
   → Voluntar vede status Approved în secțiunea "My Applications"
   ```

6. **Participare la Eveniment**
   ```
   → Sâmbătă, voluntarul merge fizic la eveniment
   → După eveniment, organizatorul marchează prezența:
      → Mark Attendance → IsPresent = TRUE pentru aplicare
      → OpportunityService.MarkAttendanceAsync(applicationId, true)
         → UPDATE Applications SET IsPresent=1
         → INSERT în EventAttendance (EventId, VolunteerId, Status=Present, MarkedAt=now)
         → PointsService.AwardPointsAsync(volunteerId, 15, CompletedEvent, eventId)
            → INSERT în PointsTransactions (VolunteerId, EventId, Points=15, Reason=CompletedEvent, CreatedAt=now)
            → UPDATE VolunteerProfiles SET Points = Points + 15 WHERE Id=volunteerId
   → Voluntarul vede punctele actualizate: 0 → 15 puncte
   ```

7. **Trimitere Feedback**
   ```
   → Voluntar navighează la secțiunea Feedback pentru eventul respectiv
   → Rating: 5 stele
   → Comment: "Experiență minunată, organizare perfectă!"
   → OpportunityService.SubmitFeedbackAsync(feedback)
      → INSERT în Feedbacks (EventId, VolunteerId, Rating=5, Comment="...", CreatedAt=now)
   → Organizatorul vede feedback-ul în dashboard-ul său
   ```

---

### Cazul 2: Organizator Creează Oportunitate cu Multiple Evenimente

**Pași:**

1. **Login ca Organizator**
   ```
   → organizer@test.com / Password123!
   → Navighează la OrganizerDashboardViewModel
   ```

2. **Creare Oportunitate**
   ```
   → Click "Create Opportunity"
   → EditOpportunityViewModel
      → Title: "Distribuire Masă Caldă"
      → Description: "Pregătim și distribuim mâncare pentru persoane defavorizate"
      → RequiredSkills: "Cooking, Communication"
      → Location: "Centrul Comunitar, Str. Libertății 10"
      → Points: 20 (puncte acordate per participare)
   ```

3. **Adăugare Evenimente Multiple**
   ```
   → Buton "Add Event"
      → Event 1: Vineri 18 Ian, 17:00-20:00, Max 10 voluntari
      → Event 2: Sâmbătă 19 Ian, 17:00-20:00, Max 10 voluntari
      → Event 3: Duminică 20 Ian, 17:00-20:00, Max 10 voluntari
   → Click "Save"
   → OpportunityService.CreateOpportunityAsync(opportunity)
      → INSERT în Opportunities (OrganizerId, Title, Description, RequiredSkills, Location, Points, Status=Published, CreatedAt=now)
      → Pentru fiecare event:
         → INSERT în Events (OpportunityId, StartTime, EndTime, MaxVolunteers, IsCompleted=false)
   ```

4. **Voluntari Aplică la Evenimente Diferite**
   ```
   → Voluntar A aplică la Event 1 (Vineri)
   → Voluntar B aplică la Event 2 (Sâmbătă)
   → Voluntar C aplică la Event 1 și Event 3
   → Fiecare aplicare creează un record separat în Applications
   ```

5. **Gestionare Aplicări per Eveniment**
   ```
   → Organizatorul vede aplicările grupate per eveniment
   → Event 1: 2 aplicări (A, C) → Aprobă pe ambii
   → Event 2: 1 aplicare (B) → Aprobă
   → Event 3: 1 aplicare (C) → Aprobă
   ```

6. **După Evenimente - Marcare Prezență**
   ```
   → După fiecare eveniment:
      → Event 1: A și C prezenți → +20 puncte fiecare
      → Event 2: B prezent → +20 puncte
      → Event 3: C absent (s-a îmbolnăvit) → 0 puncte
   → C primește puncte doar pentru Event 1, nu pentru Event 3
   ```

---

### Cazul 3: Administrator Monitorizează Sistem și Șterge Utilizator Problematic

**Pași:**

1. **Login ca Admin**
   ```
   → admin@test.com / Password123!
   → AdminDashboardViewModel
   ```

2. **Vizualizare Dashboard**
   ```
   → SystemReport afișat automat:
      - Total Users: 25
      - Total Volunteers: 18
      - Total Organizers: 6
      - Total Opportunities: 12
      - Total Applications: 87
   ```

3. **Primire Raport despre Utilizator**
   ```
   → Un organizator a creat un raport:
      → "User X creează oportunități false pentru puncte"
      → TargetType: User, TargetId: 15, ReasonType: Fake
   → Admin vede raportul în secțiunea Reports (dacă vizibilă)
   ```

4. **Investigare**
   ```
   → Admin verifică lista utilizatorilor
   → Găsește User X (Id=15)
   → Verifică oportunități create de acest user
   → Confirmă că sunt suspecte
   ```

5. **Ștergere Utilizator**
   ```
   → Click "Delete User" pe User X
   → UserService.DeleteUserAsync(15)
      → DELETE CASCADE în:
         - UserRoles
         - OrganizerProfiles (dacă organizator)
         - Opportunities create (și Events, Applications asociate)
         - ChatMessages (ca sender/receiver)
         - PointsTransactions (dacă volunteer)
         - Reports (ca reporter)
      → DELETE din Users WHERE Id=15
   → User X este complet eliminat din sistem
   → Admin Action înregistrată automat în AdminActions pentru audit
   ```

---

## Tehnologii Utilizate

### Backend
- **.NET 10.0** - Framework principal
- **C# 13** - Limbaj de programare
- **Entity Framework Core** - ORM pentru acces la date
- **SQL Server** - Bază de date relațională

### Frontend
- **Avalonia UI** - Framework cross-platform pentru desktop (alternativă la WPF)
- **MVVM Pattern** - Model-View-ViewModel pentru separarea logicii de UI
- **CommunityToolkit.Mvvm** - Library pentru simplificare MVVM (ObservableProperty, RelayCommand)
- **AXAML** - XML markup pentru UI Avalonia

### Securitate
- **SHA256 Hashing** - Pentru parole (PasswordHelper)
- **Prepared Statements** - Prin EF Core (previne SQL injection)

### DevOps și Deployment
- **Docker** - Container pentru SQL Server
- **Bash Scripts** - Automatizare setup (setup_env.sh)
- **Makefile** - Comenzi convenabile (make setup, make run)

### Configuration
- **appsettings.json** - Configurare connection strings
- **Environment Variables** - SA_PASSWORD pentru Docker SQL Server

---

## Fluxul Complet de Date - Exemplu Integrat

```
┌─────────────────────────────────────────────────────────────────┐
│                         PORNIRE APLICAȚIE                        │
└─────────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────────┐
│  1. Program.Main() → App.Initialize()                           │
│  2. Citește appsettings.json                                    │
│  3. Creează DbContext cu SQL Server                             │
│  4. Rulează auto-migrații (adaugă coloane dacă lipsesc)         │
│  5. Inițializează Services (DI)                                 │
│  6. Creează LoginViewModel → Afișează LoginView                 │
└─────────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────────┐
│                        AUTENTIFICARE                             │
│                                                                  │
│  User: volunteer@test.com / Password123!                        │
│     ↓                                                            │
│  LoginViewModel.LoginCommand                                    │
│     ↓                                                            │
│  AuthenticationService.LoginAsync(email, password)              │
│     ↓                                                            │
│  Query: SELECT * FROM Users WHERE Email = 'volunteer@...'       │
│  Verificare: PasswordHelper.VerifyPassword(input, storedHash)   │
│     ↓                                                            │
│  Return: Volunteer object (Email, FullName, Skills, Points)     │
│     ↓                                                            │
│  Navigare: MainViewModel.CurrentView = VolunteerDashboardVM     │
└─────────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────────┐
│                    VOLUNTEER DASHBOARD                           │
│                                                                  │
│  1. LoadOpportunitiesAsync()                                    │
│     → Query: SELECT * FROM Opportunities                        │
│                   INNER JOIN Users ON OrganizerId               │
│                   WHERE Status = Published                      │
│     → Afișează listă oportunități                               │
│                                                                  │
│  2. LoadRecommendedOpportunitiesAsync(volunteerId)              │
│     → Algoritm: Match Skills voluntar cu RequiredSkills         │
│     → Afișează top 5 cele mai potrivite                         │
│                                                                  │
│  3. LoadPointsDataAsync()                                       │
│     → Query: SELECT SUM(Points) FROM PointsTransactions         │
│                WHERE VolunteerId = volunteerId                  │
│     → Afișează total puncte                                     │
└─────────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────────┐
│                    APLICARE LA EVENIMENT                         │
│                                                                  │
│  User click "Apply" pe Event ID 5                               │
│     ↓                                                            │
│  OpportunityService.ApplyToEventAsync(volunteerId, 5)           │
│     ↓                                                            │
│  Verificări:                                                    │
│    - Event.StartTime >= DateTime.Now? ✓                         │
│    - Aplicare existentă? (Query Applications) ✗                 │
│    - Locuri disponibile? (Count Applications Approved < Max) ✓  │
│     ↓                                                            │
│  INSERT INTO Applications                                       │
│    (EventId=5, VolunteerId=volunteerId, Status=Pending,        │
│     AppliedAt=NOW())                                            │
│     ↓                                                            │
│  Success message în UI                                          │
└─────────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────────┐
│                  ORGANIZATOR APROBĂ APLICARE                     │
│                                                                  │
│  Organizer login → OrganizerDashboardViewModel                  │
│     ↓                                                            │
│  LoadOpportunitiesAsync(organizerId)                            │
│     → Query cu Include: Opportunities → Events → Applications   │
│     → Afișează oportunități cu aplicări Pending                 │
│     ↓                                                            │
│  Click "Approve" pe Application ID 123                          │
│     ↓                                                            │
│  UPDATE Applications                                            │
│    SET Status = Approved,                                       │
│        DecidedAt = NOW(),                                       │
│        DecidedByOrganizerId = organizerId                       │
│    WHERE Id = 123                                               │
└─────────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────────┐
│              DUPĂ EVENIMENT - MARCARE PREZENȚĂ                   │
│                                                                  │
│  Organizator → Mark Attendance pentru Application 123           │
│     ↓                                                            │
│  OpportunityService.MarkAttendanceAsync(123, isPresent=true)    │
│     ↓                                                            │
│  UPDATE Applications SET IsPresent = 1 WHERE Id = 123           │
│     ↓                                                            │
│  INSERT INTO EventAttendance                                    │
│    (EventId, VolunteerId, Status=Present, MarkedAt=NOW(),      │
│     MarkedByOrganizerId)                                        │
│     ↓                                                            │
│  PointsService.AwardPointsAsync(volunteerId, 20,                │
│                                 CompletedEvent, eventId)        │
│     ↓                                                            │
│  INSERT INTO PointsTransactions                                 │
│    (VolunteerId, EventId, Points=20, Reason=CompletedEvent,    │
│     CreatedAt=NOW())                                            │
│     ↓                                                            │
│  UPDATE VolunteerProfiles                                       │
│    SET Points = Points + 20                                     │
│    WHERE Id = volunteerId                                       │
│     ↓                                                            │
│  Voluntar vede puncte actualizate în dashboard                  │
└─────────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────────┐
│                      TRIMITERE FEEDBACK                          │
│                                                                  │
│  Voluntar → Feedback pentru Event ID 5                          │
│     ↓                                                            │
│  FeedbackViewModel                                              │
│    - Rating: 5 stars                                            │
│    - Comment: "Excelent!"                                       │
│     ↓                                                            │
│  OpportunityService.SubmitFeedbackAsync(feedback)               │
│     ↓                                                            │
│  INSERT INTO Feedbacks                                          │
│    (EventId=5, VolunteerId, Rating=5, Comment="Excelent!",     │
│     CreatedAt=NOW())                                            │
│     ↓                                                            │
│  Organizatorul vede feedback-ul în dashboard                    │
└─────────────────────────────────────────────────────────────────┘
```

---

## Caracteristici Avansate

### 1. **Sistem de Recomandări**
- Algoritmul compară `Volunteer.Skills` cu `Opportunity.RequiredSkills`
- Calculează match score bazat pe overlapping skills
- Sortează oportunități după relevanță
- Afișează top recomandări în dashboard

### 2. **Chat Real-Time (Simulat)**
- Mesagerie directă între utilizatori
- Istoricul conversațiilor persistă în DB
- Lista contactelor afișează utilizatorii cu care ai comunicat
- Posibilitate trimitere mesaje noi către orice utilizator (dacă ID cunoscut)

### 3. **Gamification prin Puncte**
- Voluntarii câștigă puncte pentru fiecare participare
- Puncte bonus pot fi acordate manual de organizatori
- Penalități pot fi aplicate pentru comportament inadecvat
- Leaderboard motivează competiție sănătoasă

### 4. **Raportare și Audit**
- Organizatorii pot genera rapoarte despre activitatea lor
- Administratorii văd statistici globale ale sistemului
- AdminActions table păstrează audit trail complet
- Reports system permite sesizarea conținutului problematic

### 5. **Gestionare Evenimente Multiple**
- O oportunitate poate avea mai multe time slots (evenimente)
- Voluntarii aleg evenimentul specific la care vor participa
- Fiecare eveniment are capacitate maximă proprie
- Status tracking separat per eveniment

### 6. **Securitate**
- Parole hash-uite cu SHA256 (nu se stochează în clar)
- Verificare roluri la fiecare acțiune sensibilă
- Cascade delete pentru integritate referențială
- Validare input la nivel de entitate (DataAnnotations)

---

## Limitări Actuale și Îmbunătățiri Posibile

### Limitări
1. **Autentificare simplă** - Nu există 2FA, email confirmation nu e implementată complet
2. **Chat nu e real-time** - Trebuie refresh manual, nu WebSocket/SignalR
3. **Skills ca string** - Skills stocate ca text, nu relațieMany-to-Many completă în cod (schema DB o are)
4. **Fără geolocalizare activă** - Lat/Lng există dar nu se folosesc pentru căutare proximitate
5. **Fără notificări** - Nu există sistem de notificări (email, push)

### Îmbunătățiri Posibile
1. **Email Service** - Trimitere email-uri pentru confirmare, reset parolă, notificări
2. **SignalR pentru Chat** - Chat real-time cu WebSocket
3. **Google Maps Integration** - Afișare oportunități pe hartă, filtrare după distanță
4. **Photo Upload** - Profile pictures, event photos
5. **Advanced Search** - Filtre complexe (dată, locație, skills, organizator)
6. **Calendar View** - Vizualizare evenimente în format calendar
7. **Teams Management** - Organizare voluntari în echipe cu task-uri
8. **Multi-language** - Suport pentru multiple limbi (i18n)
9. **Mobile App** - Versiune iOS/Android (Avalonia suportă)
10. **API REST** - Expunere servicii ca REST API pentru integrări

---

## Concluzie

**VolunteerSystem** este o aplicație completă și funcțională pentru gestionarea voluntarilor, demonstrând:

✅ **Arhitectură în straturi** bine separată
✅ **Bază de date complexă** cu relații multi-table
✅ **Patternuri moderne** (MVVM, Dependency Injection, Repository-like Services)
✅ **Funcționalități complete** pentru trei roluri distincte
✅ **Sistem de puncte gamificat**
✅ **Chat și feedback** pentru interacțiune utilizatori
✅ **Administrare și raportare** comprehensivă

Proiectul demonstrează cunoștințe solide de:
- **Programare orientată pe obiecte** (C#)
- **Design de baze de date relaționale** (SQL Server)
- **Dezvoltare interfețe grafice** (Avalonia MVVM)
- **Entity Framework și ORM**
- **Securitate aplicații** (password hashing, data validation)

Este un sistem scalabil care poate fi extins cu funcționalitățile menționate mai sus și poate deservi ca fundație pentru o aplicație reală de voluntariat.
