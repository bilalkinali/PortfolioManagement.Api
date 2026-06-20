# PortfolioManagement

A personal project for learning and building a portfolio management platform with **.NET** on the backend and **React** on the frontend.

The main purpose of the project is not just the product itself, but to improve how I work with:

* **React**
* **Vertical Slice Architecture**
* **ASP.NET Core Identity / Auth**
* **Domain modeling**
* **Clean and maintainable feature structure**
* **API + frontend separation**

## Project Structure

```text
PortfolioManagement.Api/
├── PortfolioManagement.Api.sln
├── PortfolioManagement.Api/        # .NET backend
└── portfoliomanagement.web/        # React + Vite frontend
```

## Purpose

This project is used to explore how to structure an application in a more feature-oriented way instead of grouping everything by technical layers only.

On the backend, the focus is on learning and applying **Vertical Slice Architecture**, where each feature keeps its own endpoint, request/response contracts, handler, validation, and related logic together.

On the frontend, the focus is on getting comfortable with **React**, component structure, and building a cleaner UI architecture over time.

## Tech Stack

### Backend

* .NET
* ASP.NET Core
* ASP.NET Core Identity
* Entity Framework Core
* PostgreSQL

### Frontend

* React
* Vite
* TypeScript
* Tailwind CSS
* shadcn/ui

## Setup

### 1. Clone the repository

```bash
git clone https://github.com/bilalkinali/PortfolioManagement.Api.git
cd PortfolioManagement.Api
```

### 2. Start the backend

```bash
dotnet restore
dotnet run --project PortfolioManagement.Api/PortfolioManagement.Api.csproj --launch-profile http
```

The backend listens at:

```text
http://localhost:5046
```

### 3. Start the frontend

```bash
cd portfoliomanagement.web
npm install
npm run dev
```

The frontend listens at:

```text
http://127.0.0.1:5174
```

The Vite dev server proxies `/api` and `/auth` to `http://localhost:5046`.

## Verification Notes For Code Agents

For browser acceptance checks, prefer the Codex in-app Browser when available. Use Playwright only if the in-app Browser is unavailable or a repo-owned Playwright setup exists for the requested check. This repo currently does not include a Playwright setup.

If the app is already running at `http://127.0.0.1:5174` and the API is available at `http://localhost:5046`, use those running processes. If it is not running and there is no safe repo-owned full-stack startup script, stop and ask the user to start the API and frontend manually with the commands above.

Do not invent Windows process-spawning workarounds, fight `PATH`/`Start-Process`/background shell jobs/JavaScript spawning/log redirection, change global Windows or package-manager settings, use elevated/admin permissions, or kill random processes.

Known browser-verification caveat: `AAPL` detail can show `Quote unavailable` when Finnhub quote credentials are unavailable. That alone is not a failed browser verification. If app-browser resource timing is unavailable, combine guarded runtime inspection with a frontend source scan for provider key/host leak checks.

## Important Note

The frontend dependencies inside `node_modules` are **not committed to Git**.

That means after cloning the project, you must run:

```bash
npm install
```

inside:

```text
portfoliomanagement.web
```

before the frontend will run correctly.

## Current Focus

Right now, the main focus is to build the foundation properly:

* authentication and registration
* frontend structure
* feature-based backend slices
* portfolio and trade flows later

## Long-Term Goal

The long-term goal is to create a solid full-stack project that demonstrates:

* structured backend design
* practical React usage
* clean feature organization
* realistic domain modeling
* growth from idea to working product

---

This project is also a learning space, so structure and implementation may evolve as I improve my understanding of React, auth, and Vertical Slice Architecture.
