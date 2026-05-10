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

## Installing Dependencies

### .NET 10 SDK

<details>
<summary><b>Windows</b></summary>

Download and run the installer from the official page:
```
https://dotnet.microsoft.com/en-us/download/dotnet/10.0
```
Or install via winget:
```powershell
winget install Microsoft.DotNet.SDK.10
```
Verify:
```powershell
dotnet --version
```
</details>

<details>
<summary><b>macOS</b></summary>

```bash
# Homebrew
brew install --cask dotnet-sdk

# Or via the official installer script
curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 10.0
```
Add to your shell profile if using the script:
```bash
export PATH="$HOME/.dotnet:$PATH"
```
Verify:
```bash
dotnet --version
```
</details>

<details>
<summary><b>Linux (Ubuntu / Debian)</b></summary>

```bash
# Register Microsoft package feed
wget https://packages.microsoft.com/config/ubuntu/24.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

# Install SDK
sudo apt-get update && sudo apt-get install -y dotnet-sdk-10.0
```
Verify:
```bash
dotnet --version
```
</details>

<details>
<summary><b>Linux (Fedora / RHEL)</b></summary>

```bash
sudo dnf install dotnet-sdk-10.0
```
Verify:
```bash
dotnet --version
```
</details>

---

### MongoDB

<details>
<summary><b>Windows</b></summary>

Download the Community Server installer (.msi) from:
```
https://www.mongodb.com/try/download/community
```
During installation, check **"Install MongoDB as a Service"** so it starts automatically.

Or via winget:
```powershell
winget install MongoDB.Server
```
Start manually if needed:
```powershell
net start MongoDB
```
</details>

<details>
<summary><b>macOS</b></summary>

```bash
# Add MongoDB tap and install
brew tap mongodb/brew
brew install mongodb-community

# Start as a background service
brew services start mongodb-community
```
Verify it's running:
```bash
mongosh --eval "db.runCommand({ connectionStatus: 1 })"
```
</details>

<details>
<summary><b>Linux (Ubuntu / Debian)</b></summary>

```bash
# Import MongoDB public key
curl -fsSL https://www.mongodb.org/static/pgp/server-8.0.asc | \
  sudo gpg -o /usr/share/keyrings/mongodb-server-8.0.gpg --dearmor

# Add repository
echo "deb [ arch=amd64,arm64 signed-by=/usr/share/keyrings/mongodb-server-8.0.gpg ] \
  https://repo.mongodb.org/apt/ubuntu noble/mongodb-org/8.0 multiverse" | \
  sudo tee /etc/apt/sources.list.d/mongodb-org-8.0.list

# Install
sudo apt-get update && sudo apt-get install -y mongodb-org

# Start and enable on boot
sudo systemctl start mongod
sudo systemctl enable mongod
```
Verify:
```bash
mongosh --eval "db.runCommand({ connectionStatus: 1 })"
```
</details>

<details>
<summary><b>Linux (Fedora / RHEL)</b></summary>

```bash
# Add repo file
sudo tee /etc/yum.repos.d/mongodb-org-8.0.repo <<EOF
[mongodb-org-8.0]
name=MongoDB Repository
baseurl=https://repo.mongodb.org/yum/redhat/9/mongodb-org/8.0/x86_64/
gpgcheck=1
enabled=1
gpgkey=https://www.mongodb.org/static/pgp/server-8.0.asc
EOF

# Install
sudo dnf install -y mongodb-org

# Start and enable on boot
sudo systemctl start mongod
sudo systemctl enable mongod
```
</details>

---

## Running Locally

Once .NET 10 and MongoDB are installed and MongoDB is running on `localhost:27017`:

```bash
git clone https://github.com/radiano2/NightCityVTT.git
cd NightCityVTT
dotnet run --project NightCityVTT
```

The app starts at `https://localhost:7xxx` (exact port shown in terminal output).

**First launch:**
1. Navigate to the app — you'll be redirected to `/login`
2. No users exist yet, so you'll be prompted to create the first GM account
3. Go to `/setup` and click **SEED DATABASE** to populate all CP2020 core data
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
