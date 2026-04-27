# Deployment Guide

FitPlan ships as two Docker images: an ASP.NET Core API and an nginx-served Vue 3 SPA. Nginx proxies `/api/` to the API and serves the SPA for all other routes.

## VPS Deployment (Docker Compose)

### Prerequisites

- VPS running Ubuntu 22.04 or Debian 12
- Docker Engine ≥ 24 and Docker Compose v2
- A domain name (recommended for SSL)
- Outbound SMTP access or a relay service (SendGrid, Postmark, etc.)
- Google OAuth credentials (optional — only required if Google login is enabled)

### Step 1: Install Docker

```bash
curl -fsSL https://get.docker.com | sh
sudo usermod -aG docker $USER
newgrp docker
```

### Step 2: Clone and Configure

```bash
git clone <repo-url>
cd fitplan
cp .env.example .env
```

Edit `.env` with production values:

```bash
# MongoDB
MONGO_PASSWORD=<strong-random-password>

# JWT — generate with: openssl rand -base64 32
JWT_SECRET=<64-char-random-string>

# SMTP
SMTP_HOST=smtp.sendgrid.net
SMTP_PORT=587
SMTP_USERNAME=apikey
SMTP_PASSWORD=<sendgrid-api-key>
SMTP_FROM=noreply@yourdomain.com

# Google OAuth (leave blank to disable)
GOOGLE_CLIENT_ID=
GOOGLE_CLIENT_SECRET=
GOOGLE_REDIRECT_URI=https://yourdomain.com/api/auth/google/callback

# App URLs
FRONTEND_URL=https://yourdomain.com
PORT=80
```

### Step 3: Build and Start

```bash
make build       # builds fitplan-api:latest and fitplan-frontend:latest
make prod-up     # starts the stack in background
```

Check logs:
```bash
make prod-logs
```

Verify the API is healthy:
```bash
curl http://localhost/api/health
# {"status":"healthy","timestamp":"..."}
```

### Step 4: SSL with Caddy (Recommended)

Install Caddy on the host:
```bash
sudo apt install -y debian-keyring debian-archive-keyring apt-transport-https
curl -1sLf 'https://dl.cloudsmith.io/public/caddy/stable/gpg.key' | sudo gpg --dearmor -o /usr/share/keyrings/caddy-stable-archive-keyring.gpg
curl -1sLf 'https://dl.cloudsmith.io/public/caddy/stable/debian.deb.txt' | sudo tee /etc/apt/sources.list.d/caddy-stable.list
sudo apt update && sudo apt install caddy
```

Create `/etc/caddy/Caddyfile`:
```
yourdomain.com {
    reverse_proxy localhost:80
}
```

Change `PORT=443` is not needed — Caddy terminates TLS and forwards to nginx on `:80`.

Restart Caddy:
```bash
sudo systemctl reload caddy
```

### Updating

```bash
git pull
make build
make prod-down
make prod-up
```

### Maintenance

```bash
make prod-logs       # tail all logs
make prod-restart    # restart all services
make prod-down       # stop stack (data preserved in volume)

# MongoDB backup
docker compose -f docker-compose.prod.yml exec mongodb \
  mongodump --username root --password $MONGO_PASSWORD \
  --authenticationDatabase admin --db fitplan_prod --archive \
  > backup-$(date +%Y%m%d).archive

# Restore
docker compose -f docker-compose.prod.yml exec -i mongodb \
  mongorestore --username root --password $MONGO_PASSWORD \
  --authenticationDatabase admin --archive \
  < backup-20240101.archive
```

## Environment Notes

### MongoDB

MongoDB data persists in a named Docker volume (`mongodb_data`). It is only reachable on the internal bridge network — not exposed to the host.

### JWT Secret Rotation

Rotating `JWT_SECRET` invalidates all existing access tokens. Refresh tokens stored in MongoDB remain valid but cannot generate new access tokens with the old key. After rotation:
1. Update `.env`
2. `make prod-restart`
3. Users will be logged out and need to re-authenticate

### Google OAuth Setup

1. Go to [Google Cloud Console](https://console.cloud.google.com) → APIs & Services → Credentials
2. Create an OAuth 2.0 Client ID (Web application)
3. Add Authorized redirect URI: `https://yourdomain.com/api/auth/google/callback`
4. Copy Client ID and Client Secret to `.env`

## CI/CD with GitHub Actions

`deploy.yml` builds and pushes Docker images to GitHub Container Registry on every push to `main`:

- `ghcr.io/<owner>/<repo>/api:latest` — API image
- `ghcr.io/<owner>/<repo>/frontend:latest` — Frontend image

To pull and deploy pre-built images instead of building on the server, update `docker-compose.prod.yml` to use `image:` instead of `build:`:

```yaml
services:
  api:
    image: ghcr.io/<owner>/<repo>/api:latest
  frontend:
    image: ghcr.io/<owner>/<repo>/frontend:latest
```

Then on the server:
```bash
echo $GITHUB_TOKEN | docker login ghcr.io -u $GITHUB_USER --password-stdin
docker compose -f docker-compose.prod.yml pull
make prod-down && make prod-up
```
