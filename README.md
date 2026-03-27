# MovieApp — WinUI 3 Desktop Application

A comprehensive movie catalog, review, battle, and discussion platform built with **WinUI 3**, **.NET 8**, and **ADO.NET** (raw SQL, no ORM).

## 📦 Project Structure

```
MovieApp.sln
├── MovieApp.Core   → Models, Services, Interfaces, Repositories
└── MovieApp.UI     → WinUI 3 XAML Views + MVVM ViewModels
```

## 🛠️ Prerequisites

- **Visual Studio 2022 (17.8+)** with:
  - .NET 8 SDK
  - Windows App SDK workload
- **SQL Server Express** installed and running

## 🚀 How to Run

### 1. Clone / Open the Solution
Open `MovieApp.sln` in Visual Studio 2022.

### 2. Restore NuGet Packages
```bash
dotnet restore
```

### 3. Database Setup
The app connects to SQL Server Express automatically. Tables and seed data are created on first run via a custom `DatabaseInitializer`. The connection string used is:
```
Server=.\SQLEXPRESS;Database=Movie_App2;Trusted_Connection=True;TrustServerCertificate=True;
```
If SQL Server Express is unavailable the app falls back to an in-memory mock data mode automatically.

### 4. Run the Application
Set **MovieApp.UI** as the startup project, then press **F5** or:
```bash
dotnet run --project MovieApp.UI
```

## 📋 Features

### 🎬 Movie Catalog
- Browse all movies with poster, title, year, genre, and rating
- Real-time search by title
- Filter by genre and minimum rating

### ✍️ Reviews
- Add star ratings (0–5, 0.5 increments)
- Submit extended reviews with 5 categories (Cinematography, Acting, CGI, Plot, Sound)
- One review per user per movie enforced
- Minimum 50 characters, maximum 2000 characters

### ⚔️ Battle Arena
- Weekly movie battles between similarly-rated films
- Place bets with earned points (one bet per battle)
- Winners determined by rating improvement over the week
- Battle and bet results visible even after the battle ends

### 💬 Forum
- Threaded comment discussions per movie
- Reply to existing comments
- Maximum 10,000 characters per comment

### 🏆 Points & Badges
- Earn points for reviewing movies
- Six achievement badges: The Snob, The Super Serious, The Joker, The Godfather I/II/III

### 🎭 External Reviews
- Critic reviews fetched from NYT, Guardian, and OMDb APIs
- Results cached locally to avoid redundant API calls
- Aggregate critic and audience scores per movie

## 🏗️ Architecture

- **MVVM Pattern** — Views, ViewModels, and Models cleanly separated
- **Dependency Injection** — All services registered in `App.xaml.cs`
- **ADO.NET Repositories** — Raw SQL queries, no ORM
- **Async/Await** — All database and API operations are async
- **Automatic DB Fallback** — Falls back to mock data if SQL Server Express is unreachable
- **Hard-coded User** — UserId = 1 (no authentication required)

## 📝 Database Schema

9 entities: Movie, User, Review, Comment, Battle, Bet, UserStats, Badge, UserBadge

Composite primary keys:
- `Bet` → (UserId, BattleId)
- `UserBadge` → (UserId, BadgeId)
