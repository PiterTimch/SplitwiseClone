# SplitwiseClone

REST API for shared group expenses and member balances.

## Features

- Create groups and manage their members.
- Record group expenses with multiple payers and standalone expenses without a group.
- Split owed amounts equally, exactly, by percentage, or by weighted shares.
- View member balances and suggested transfers that simplify group debts.
- Authenticate users with ASP.NET Core Identity and JWT bearer tokens.

Group expense endpoints require an authenticated group member. Standalone expenses are visible only to their creator and listed participants. Expense creators are the only users who can edit or delete their expenses.

## API

[Open the deployed Swagger UI](https://splitwiseclone-wfaq.onrender.com/swagger/index.html)

## Run locally

Create a PostgreSQL database, then configure the connection string and JWT signing key through user secrets or environment variables:

```sh
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Database=splitwiseclone;Username=postgres;Password=..." --project Api/SplitwiseClone.Api.csproj
dotnet user-secrets set "JwtSettings:SecretKey" "a-random-secret-key-with-at-least-32-characters" --project Api/SplitwiseClone.Api.csproj
dotnet run --project Api
```

The API applies pending EF Core migrations on startup. Open `http://localhost:5223/swagger` for local API documentation. Local development allows `http://localhost:3000` and `http://localhost:5173`; production must set `Cors__AllowedOrigins__0` to the browser application's exact origin.

## Run tests

```sh
dotnet test SplitwiseClone.slnx --configuration Release
```

See [DEPLOYMENT.md](DEPLOYMENT.md) for Render and Neon setup.
