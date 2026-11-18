# SplitwiseClone

REST API for shared group expenses and member balances.

## Features

- Create groups and manage their members.
- Record group expenses with multiple payers.
- Split owed amounts equally, exactly, by percentage, or by weighted shares.
- View member balances and suggested transfers that simplify group debts.
- Authenticate users with ASP.NET Core Identity and JWT bearer tokens.

Every group and expense endpoint requires an authenticated member of the relevant group. Expense creators are the only users who can edit or delete their expenses.

## API

[Open the deployed Swagger UI](https://splitwiseclone-wfaq.onrender.com/swagger/index.html)

## Run locally

Create a PostgreSQL database, then configure the connection string and JWT signing key through user secrets or environment variables:

```sh
dotnet user-secrets --project Api set "ConnectionStrings:DefaultConnection" "Host=localhost;Database=splitwiseclone;Username=postgres;Password=..."
dotnet user-secrets --project Api set "JwtSettings:SecretKey" "a-random-secret-key-with-at-least-32-characters"
dotnet run --project Api
```

The API applies pending EF Core migrations on startup. Open `http://localhost:5223/swagger` for local API documentation. To allow a browser client, configure `Cors:AllowedOrigins` as an array of trusted origins.

## Run tests

```sh
dotnet test SplitwiseClone.slnx --configuration Release
```

See [DEPLOYMENT.md](DEPLOYMENT.md) for Render and Neon setup.
