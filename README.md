# Strux Relay

**Remote access to your ESP32 devices, without port forwarding or a VPN.**

Strux Relay connects devices built with [Strux](https://github.com/StruxHQ/Strux) to browsers and AI assistants outside your local network. Each device opens an **outbound WebSocket connection** to the relay, so you don't need to expose the ESP32 itself to the internet.

The relay is optional: Strux devices work locally without it. For example, [Lablr](https://github.com/StruxHQ/Lablr) can print labels over local Wi-Fi, while the relay lets you control it remotely, including through ChatGPT using MCP.

## Features

- **Device dashboard** — See connected devices and approve new connections
- **Remote web UI** — Open each device's own interface through the relay
- **MCP integration** — Let compatible AI clients discover devices and execute their commands
- **Access control** — Device pairing, per-device MCP exposure, bearer tokens and OAuth
- **Telemetry** — Receive live measurements and optionally forward them to InfluxDB
- **Frontend cache** — Cache device web assets to reduce traffic to the ESP32

## How it works

```text
ESP32 device ── outbound WebSocket ──► Strux Relay ◄── Browser
                                              ▲
                                              │
                                         MCP client
                                       (e.g. ChatGPT)
```

The relay forwards commands and device responses without needing to understand the device's application. A new Strux product can use the same relay without printer-, sensor-, or device-specific server code.

New devices must be approved in the dashboard. MCP access is separately enabled per device and is **off by default**.

## Getting started

The server uses **ASP.NET Core (.NET 10)** and includes a **React** dashboard.

To run it locally, install the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) and start the API:

```bash
cd api
dotnet run
```

The server listens on port `8080` and serves the built dashboard. For frontend development, install [Node.js](https://nodejs.org/) and [pnpm](https://pnpm.io/), then run `pnpm dev` from `frontend/`.

### Docker Compose

For a quick test on a trusted local network, create a `compose.yaml`:

```yaml
services:
  strux-relay:
    image: ghcr.io/vanbassum/strux-relay:main
    restart: unless-stopped
    ports:
      - "8080:8080"
    environment:
      Relay__Database__ConnectionString: "Data Source=/app/data/relay.sqlite"
    volumes:
      - ./data:/app/data
```

Run `docker compose up -d` and open `http://localhost:8080`. The `data/` directory keeps device approvals across container restarts. The image path above is the existing published namespace; it can change when GHCR publishing moves to StruxHQ.

**This is a local example, not a public deployment.** For internet access, put the relay behind HTTPS and protect the dashboard with authentication. Device, MCP and OAuth routes need different proxy rules; see [operations documentation](docs/operations.md).

On a Strux device, configure `relay.enabled` and point `relay.url` at `ws://<server-ip>:8080/device` (or `wss://` for a public HTTPS deployment). The device appears as pending in the dashboard; approve it to allow the connection.

For a public deployment, use HTTPS with a reverse proxy and authentication for the dashboard. See [operations documentation](docs/operations.md) for deployment details.

## MCP

The relay exposes a **streamable HTTP MCP endpoint** at `/mcp`. It provides three generic tools:

| Tool | Purpose |
| --- | --- |
| `devices` | List connected devices exposed to MCP |
| `describe` | Discover a device's commands and arguments |
| `execute` | Run a command on a device |

Commands are described by the device firmware, not hardcoded into the relay. This means the same MCP integration can control different Strux applications.

Create an MCP token in the dashboard for clients that support bearer authentication. Clients such as ChatGPT can connect using the relay's OAuth flow. A public deployment requires a reachable HTTPS endpoint.

## Project structure

```text
api/          ASP.NET Core relay, device connections, MCP and OAuth
frontend/     React dashboard
docs/         Deployment and operations documentation
```

## License

Licensed under the [MIT License](LICENSE).
