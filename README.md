# NightCityVTT

A browser-based Virtual Tabletop (VTT) for running **Cyberpunk 2020** sessions. Built for local network play — host it on your machine, your players connect from their browsers. No account services, no subscriptions, no cloud dependency.

---

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Frontend | Blazor WebAssembly (.NET 10) |
| Backend | ASP.NET Core 10 (hosted server) |
| Database | MongoDB (via MongoDB.Driver) |
| UI Components | MudBlazor + custom CSS (cyberpunk theme) |
| Real-time | ASP.NET Core SignalR (`GameHub`) |
| Auth | Custom session auth — SHA-256 hashed passwords, localStorage session token |
| Shared logic | .NET shared class library (`NightCityVTT.Shared`) |

---

## Architecture

The solution is split into three projects:

```
NightCityVTT.sln
├── NightCityVTT/           # ASP.NET Core host — API controllers, SignalR hub, MongoDB services
├── NightCityVTT.Client/    # Blazor WebAssembly SPA — all UI pages and client-side logic
└── NightCityVTT.Shared/    # Shared models and logic (DTOs, DiceEngine, FnffEngine)
```

The server hosts the WASM client and exposes a REST API. Pages that need interactivity run as `InteractiveWebAssembly` components; the shell (nav, layout) is server-rendered.

---

## Features

### Authentication
- User accounts with **Nickname + Password** (SHA-256 hashed)
- Two roles: **Player** and **Game Master**
- Login screen with user picker — select your character, enter your code
- Session persisted in `localStorage`; floating switcher widget to change users without a full reload
- First launch automatically prompts to create the first GM account

### Character Management (`/characters`)
- Create and view CP2020 characters with stats (INT, REF, TECH, COOL, ATTR, LUCK, MA, BODY, EMP), skills, role, and special ability
- **Role-based visibility**: Players see only their own characters; the GM sees everyone's with owner attribution
- Characters are tied to their creator's user ID (`OwnerId`)
- Inventory panel showing purchased gear with cost

### Marketplace (`/marketplace`)
- Browse the full CP2020 gear catalogue (weapons, armor, cyberware, etc.)
- Purchase items directly into a character's inventory
- Gear data seeded from the CP2020 core rulebook

### FNFF Combat Engine (`/fnff`)
- Full implementation of the CP2020 Friday Night Firefight rules
- Covers hit rolls, cover, range modifiers, wound class, and damage resolution
- Step-by-step combat log

### Dice Terminal (`/dice`)
- Freeform dice roller: d6, d10, d100, custom pools
- Roll history with cyberpunk terminal aesthetic

### Story Editor (`/story`)
- GM tool for writing and organizing session notes and narrative beats

### GM Setup Console (`/setup`)
- One-click database seeding with all CP2020 core data (skills, roles, gear)
- Validation report showing record counts per collection

---

## Running Locally

**Prerequisites:** .NET 10 SDK, MongoDB running on `localhost:27017`

```bash
# Clone and run
git clone https://github.com/radiano2/NightCityVTT.git
cd NightCityVTT
dotnet run --project NightCityVTT
```

The app starts at `https://localhost:7xxx` (port shown in terminal).

**First launch:**
1. Navigate to the app — you'll be redirected to `/login`
2. No users exist yet, so you'll be prompted to create the first GM account
3. Go to `/setup` and click **SEED DATABASE** to populate CP2020 data
4. Start playing

---

## Project Structure

```
NightCityVTT/
├── Controllers/        # REST API (Characters, Gear, Users, Seed, Story, Settings)
├── Hubs/               # SignalR GameHub
├── Models/             # MongoDB documents (CharacterDocument, UserDocument, etc.)
├── Services/           # DatabaseSeeder
└── wwwroot/            # Static assets, global CSS

NightCityVTT.Client/
├── Auth/               # Blazor AuthenticationStateProvider
├── Components/         # Shared UI components (AuthGuard, UserSwitcherWidget, etc.)
├── Pages/              # Routable pages (CharacterCreation, Marketplace, FnffTest, etc.)
└── Services/           # API client services (CharacterApiService, UserApiService, etc.)

NightCityVTT.Shared/
├── Models/             # DTOs shared between server and client
└── Services/           # DiceEngine, FnffEngine (pure C# logic, runs on both sides)
```

---

## Configuration

`NightCityVTT/appsettings.json`:
```json
{
  "MongoDB": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "NightCityVTT"
  }
}
```

Change `ConnectionString` to point to a remote MongoDB instance if needed.
