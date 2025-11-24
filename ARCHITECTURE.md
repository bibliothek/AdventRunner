# AdventRunner Architecture Overview

## Project Description
AdventRunner is an Advent calendar application (December 1-24) that gamifies running activities by integrating with Strava. Users get randomized daily running distances based on the dates 1-24km, and the application tracks their progress through Strava activity verification.

## Technology Stack

### Backend (.NET 8.0 / F#)
- **Framework**: ASP.NET Core 8.0
- **Language**: F# (Functional-first)
- **Web Framework**: Saturn (F# web framework built on Giraffe)
- **HTTP Library**: Flurl.Http
- **Authentication**: JWT Bearer tokens via Auth0
- **Serialization**: Newtonsoft.Json
- **Build Tool**: FAKE (F# Make)
- **Package Manager**: Paket

### Frontend (Vue.js 3)
- **Framework**: Vue 3 with TypeScript
- **State Management**: Vuex 4
- **Routing**: Vue Router 4
- **Build Tool**: Vite
- **UI Framework**: DaisyUI (Tailwind CSS based)
- **Authentication**: Auth0 SPA SDK
- **HTTP Client**: Axios
- **Icons**: Font Awesome

### Infrastructure
- **Target Framework**: .NET 8.0
- **Containerization**: Docker (multi-stage builds)
- **Storage**: File-based JSON storage (configurable via AR_Storage_Path)
- **Deployment**: Azure-ready (references to Farmer for IaC)

## Solution Structure

```
AdventRunner/
├── src/
│   ├── Shared/           # Shared domain models (F#)
│   ├── Server/           # Backend web API (F#)
│   └── Client/           # Frontend SPA (Vue.js + TypeScript)
├── Build.fsproj          # FAKE build automation
├── Dockerfile            # Production container definition
├── paket.dependencies    # NuGet package definitions
└── AdventRunner.sln      # Visual Studio solution
```

## Core Domain Models (src/Shared/Shared.fs)

### Data Types
- **DoorState**: `Closed | Open | Done | Failed` - Calendar door status
- **CalendarDoor**: Represents one day's challenge (day, distance, state)
- **Calendar**: Collection of doors + settings + verified distance from Strava
- **UserData**: Complete user profile with multiple calendars (by year), display preferences
- **SharedLink**: Shareable calendar links with owner and period info
- **Settings**: User configuration (distance factor multiplier, shared link ID)

### Key Concepts
- **Period**: Year-based calendar periods (2024, 2025, etc.)
- **Verified Distance**: Actual distance run, synced from Strava
- **Distance Factor**: Multiplier for customizing challenge difficulty

## Backend Architecture (src/Server/)

### Entry Point: Runner.fs
- Configures Saturn application on port 8085
- Sets up dependency injection for storage services and sync queue
- Configures JWT authentication middleware
- Routes to three main endpoint modules

### Storage Layer (Storage.fs)
**File-based JSON persistence with two storage types:**

#### UserDataStorage
- Container: "users"
- Key: owner.name
- Operations: GetUserData, UserExists, UpdateUserData, AddNewUser

#### SharedLinksStorage
- Container: "shared-links"
- Key: sharedLink.id
- Operations: UpsertSharedLink, LinkExists, GetSharedLink, DeleteSharedLink

**Storage Location**: Configurable via `AR_Storage_Path` environment variable (default: ".data")

### API Endpoints

#### CalendarEndpoints.fs
**Routes**: `/api/calendars` (GET, PUT, POST)
- **GET**: Retrieves user calendar, handles migration between versions/periods
- **PUT**: Updates user calendar data
- **POST**: Resets calendar (deletes associated shared links)
- **Migration Logic**:
  - Version migration from pre-2.0 to 2.0
  - Auto-creates new calendar for new year periods
  - Preserves settings from previous period

#### SharedLinkEndpoints.fs
**Routes**: `/api/sharedCalendars/:id` (GET, DELETE), `/api/sharedCalendars` (POST)
- **GET**: Public endpoint - retrieves calendar via shared link ID
- **POST**: Creates shareable link (authenticated)
- **DELETE**: Removes shared link (authenticated)
- Uses GUID-based short IDs for sharing

#### WebhookEndpoints.fs
**Routes**: `/api/webhooks/strava` (GET, POST)
- **GET**: Handles Strava webhook verification (hub.challenge)
- **POST**: Receives Strava activity events
- Filters for "activity" object type only
- Enqueues sync jobs for activity updates

### Authentication (Auth/)

#### TokenAuthenticationExtensions.fs
- Configures JWT Bearer authentication with Auth0
- Authority: `adventrunner.eu.auth0.com`
- Audience: `AdventRunner`

#### EndpointsHelpers.fs
- `mustBeLoggedIn`: Authentication middleware
- `getUser`: Extracts Owner from ClaimTypes.NameIdentifier

### External API Integrations

#### Auth0Client.fs
**Purpose**: Manages Auth0 Management API access for user data
- Obtains S2S management tokens (cached with 5-min buffer)
- Retrieves Strava refresh tokens from user identities
- Functions:
  - `getManagementAccessTokenAsync`: Gets/caches Auth0 mgmt token
  - `getStravaRefreshToken`: Fetches refresh token for Strava users
  - `canVerifyDistance`: Checks if user has Strava connection

**Configuration**:
- `AR_Auth0_ClientId`
- `AR_Auth0_ClientSecret`

#### StravaClient.fs
**Purpose**: Strava API integration
- Exchanges refresh token for access token
- Fetches activities within date range
- Functions:
  - `getAccessToken`: OAuth token refresh
  - `getActivities`: Fetches activities with pagination (per_page=100)

**Configuration**:
- `AR_Strava_ClientId`
- `AR_Strava_ClientSecret`

### Background Processing

#### MsgProcessor.fs
**SyncQueue**: MailboxProcessor-based message queue
- Processes sync messages asynchronously
- One message at a time (sequential processing)
- Error handling with console logging

#### StravaSync.fs
**Core sync logic for Strava activity verification**

**Key Functions**:
- `sync`: Main entry point, dispatches sync for selected periods
- `syncVerifiedDistance`:
  1. Fetches activities from Strava for December of specified year
  2. Filters for accepted sport types (Run, Hike, TrailRun, VirtualRun, Walk)
  3. Filters for December activities (by local time)
  4. Calculates total distance
  5. Updates UserData if distance changed

**Period Handling**:
- `PeriodSelector`: `All | Period of int`
- December timestamps: Nov 30 00:00 - Jan 2 00:00 (UTC) with local filtering

**Performance**: Runs synchronously within async context (via `Async.RunSynchronously`)

## Frontend Architecture (src/Client/)

### Technology Choices
- **Vue 3 Composition API** with TypeScript
- **Vuex** for centralized state management
- **Vue Router** for SPA navigation
- **Vite** for fast development and optimized builds

### Key Components (assumed from file structure)
- `App.vue`: Root component
- `Shell.vue`: Layout/shell component
- `navbar.vue`: Navigation component
- **DoorCalendar/**: Door-based calendar view
  - `DoorCalendar.vue`: Container
  - `OpenDoor.vue`: Opened door state
  - `ClosedDoor.vue`: Closed door state
- `MonthlyCalendar.vue`: Month view of calendar
- `RunProgress.vue`: Progress tracking visualization

### Views
- `home.vue`: Landing/home page
- `calendar.vue`: Main calendar view
- `settings.vue`: User settings

### State Management (store/)
- `index.ts`: Vuex store configuration
- `action-types.ts`: Action constants
- `mutation-types.ts`: Mutation constants

### Models (models/)
- `calendar.ts`: TypeScript calendar models
- `fsharp-helpers.ts`: Utilities for F# interop

### Authentication (auth/)
- `index.ts`: Auth0 SPA integration

## Build System (Build.fs)

### FAKE Targets
- **Clean**: Removes deploy directory
- **InstallClient**: Runs `yarn install`
- **RunClient**: Runs `yarn dev` (development server)
- **Bundle**: Production build (publishes server + builds client)
- **Run**: Development mode (watches server + runs client in parallel)
- **RunTests**: Test execution

### Dependencies
```
Clean => InstallClient => Run
InstallClient => RunClient
InstallClient => RunTests
InstallClient => Bundle
```

### Entry Points
- `dotnet run -- bundle`: Production build
- `dotnet run -- run`: Development mode

## Deployment

### Docker Multi-Stage Build
**Stage 1 (build)**:
- Based on `mcr.microsoft.com/dotnet/sdk:8.0`
- Installs Node.js 18.x and yarn
- Restores dotnet tools
- Runs `dotnet run -- Bundle`

**Stage 2 (runtime)**:
- Based on `mcr.microsoft.com/dotnet/aspnet:8.0`
- Copies artifacts from /workspace/deploy
- Exposes port 8085
- Runs `Server.dll`

### Environment Variables
**Required**:
- `AR_Auth0_ClientId`: Auth0 application client ID
- `AR_Auth0_ClientSecret`: Auth0 application client secret
- `AR_Strava_ClientId`: Strava API client ID
- `AR_Strava_ClientSecret`: Strava API secret

**Optional**:
- `AR_Storage_Path`: Custom storage path (default: ".data")

## Data Flow

### User Calendar Flow
1. User authenticates via Auth0 (JWT)
2. Frontend requests `/api/calendars` (GET)
3. Backend checks if user exists in storage
4. If exists: loads data, runs migration if needed
5. If not: initializes new UserData with default settings
6. If Strava user: enqueues background sync for verified distance
7. Returns UserData with calendars for all periods

### Strava Sync Flow
1. Webhook receives activity event from Strava
2. Extracts owner_id and event_time
3. Creates sync message with owner and period
4. Enqueues to SyncQueue (MailboxProcessor)
5. Background processor:
   - Gets Auth0 management token
   - Retrieves Strava refresh token from Auth0
   - Gets Strava access token
   - Fetches December activities
   - Filters by sport type and local date
   - Calculates total distance
   - Updates UserData if changed

### Shared Link Flow
1. User creates shared link (POST `/api/sharedCalendars`)
2. System generates GUID-based short ID
3. Creates SharedLink record (owner, period, id)
4. Updates UserData calendar settings with sharedLinkId
5. Anyone with link ID can GET `/api/sharedCalendars/:id`
6. Returns displayName, calendar, period (no authentication required)

## Security Considerations

### Authentication
- JWT Bearer tokens from Auth0
- NameIdentifier claim used as user ID
- Public endpoints: Shared link retrieval
- Protected endpoints: All calendar mutations, shared link creation/deletion

### Authorization
- User isolation via owner.name from JWT claims
- Shared links: Read-only, no mutation allowed
- Webhook: Accepts all Strava events (validation via hub.challenge)

### Storage
- File-based (no built-in encryption)
- User data keyed by Auth0 user ID
- Pipe character replaced with dash in filenames

## Scaling Considerations

### Current Limitations
1. **File Storage**: Not suitable for high concurrency or distributed systems
2. **Sync Queue**: In-memory MailboxProcessor (lost on restart)
3. **Token Caching**: In-memory, single-instance only
4. **Synchronous Sync**: Blocking operations during sync

### Potential Improvements
1. Replace file storage with database (PostgreSQL, CosmosDB)
2. Use durable message queue (Azure Service Bus, RabbitMQ)
3. Use distributed cache for tokens (Redis)
4. Implement proper async/await throughout sync pipeline
5. Add retry policies for external API calls
6. Implement rate limiting for Strava/Auth0 APIs

## Development Workflow

### Prerequisites
- .NET 8.0 SDK
- Node.js and Yarn
- Run: `dotnet tool restore`

### Local Development
```bash
dotnet run -- run
```
- Server: watches on changes, hot reload
- Client: Vite dev server with HMR

### Production Build
```bash
dotnet run -- bundle
```
Output: `deploy/` directory

### Docker
```bash
docker build -t adventrunner .
docker run -p 8085:8085 \
  -e AR_Auth0_ClientId=xxx \
  -e AR_Auth0_ClientSecret=xxx \
  -e AR_Strava_ClientId=xxx \
  -e AR_Strava_ClientSecret=xxx \
  adventrunner
```

## Key Design Patterns

### Backend
- **Functional Core**: Immutable domain models, pure functions in Shared
- **Railway Oriented Programming**: Option types for nullable values
- **Computation Expressions**: task {} for async operations
- **Agent Pattern**: MailboxProcessor for message queue
- **Repository Pattern**: Storage abstractions for data access

### Frontend
- **Vuex Store Pattern**: Centralized state with actions/mutations
- **Component Composition**: Small, focused Vue components
- **Type Safety**: TypeScript for compile-time checks
- **Auth Guard**: Router-level authentication

## Testing Strategy
- Test infrastructure present (serverTestsPath, sharedTestsPath references)
- `dotnet run -- RunTests` target configured
- No test files currently in solution structure

## Migration & Versioning
- UserData.version field (current: "2.0")
- Auto-migration in CalendarEndpoints:
  - Pre-2.0 → 2.0: Full reset
  - New period: Creates new calendar, preserves settings
- Period-based: Each year gets separate calendar

## Notes for Future Development

### Adding New Features
1. **New Endpoints**: Add to respective endpoint module, register in Runner.fs
2. **New Domain Types**: Add to Shared.fs (visible to both client/server)
3. **New Storage**: Extend Storage.fs with new container type
4. **New External API**: Create new client module (similar to StravaClient)

### Common Patterns
- Use `task {}` for async operations
- Use `Option` types instead of null
- Pipe operators `|>` for data transformation
- Pattern matching for type discrimination
- Named parameters in F# records

### Configuration Management
- All config via environment variables
- No appsettings.json present
- Hard-coded values: Auth0 domain, audience, port 8085

### Frontend-Backend Contract
- Shared types must match between F# (Shared.fs) and TypeScript (models/)
- JSON serialization via Newtonsoft (server) and Axios (client)
- No code generation for type sync (manual maintenance)
