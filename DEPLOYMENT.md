# Deploy to Render and Neon

The API runs as a Docker web service on Render. PostgreSQL is hosted separately on Neon. EF Core applies pending migrations when the API starts.

## Configure Neon

1. Create a PostgreSQL project and database in Neon.
2. Copy its connection URI, including `sslmode=require` (the application also enforces TLS for PostgreSQL URIs).
3. Keep this URI private. Set it as Render's `ConnectionStrings__DefaultConnection` environment variable when creating the service from `render.yaml`.

The application accepts Neon's `postgresql://` URI directly and converts it to an Npgsql connection string.

## Configure Render

1. In Render, create a Blueprint instance from this repository and select `render.yaml`.
2. Set `ConnectionStrings__DefaultConnection` to the Neon connection URI when prompted. Render generates `JwtSettings__SecretKey` for the service.
3. In the service settings, create a deploy hook and copy its URL.
4. Confirm the service health check uses `/health`.

The service uses the `PORT` value supplied by Render. Its `/health` endpoint returns HTTP 200 when the API process is ready.

## Enable continuous deployment

Add a GitHub Actions repository secret named `RENDER_DEPLOY_HOOK_URL` containing the Render deploy hook URL. The workflow builds and tests every pull request to `main` and every push to `main`, including a Docker image build. After a successful push to `main`, it invokes the deploy hook. `autoDeploy: false` in `render.yaml` ensures Render deploys only after these checks pass.

## Run locally with Docker

Build from the repository root:

```sh
docker build -t splitwiseclone-api .
docker run --rm -p 8080:8080 \
  -e ConnectionStrings__DefaultConnection='postgresql://USER:PASSWORD@HOST/DATABASE?sslmode=require' \
  -e JwtSettings__SecretKey='a-long-random-secret-of-at-least-32-characters' \
  splitwiseclone-api
```
