# Moony

EVE Online moon mining survey scan formatter. Paste raw moon survey scanner output and get structured, spreadsheet-friendly results with ore rarity ratings (R4–R64).

## Architecture

- **Frontend:** React SPA (Vite + TypeScript)
- **Backend:** .NET 10 AOT minimal API (C#)
- **Database:** PostgreSQL

## Project Structure

- `/api/` — .NET 10 AOT backend API
- `/web/` — React frontend (Vite + TypeScript)
- `/sql/` — Database schema scripts (source of truth)

## Building

### API
```
cd api
dotnet publish -c Release -r linux-x64 --self-contained /p:PublishAot=true
```

### Frontend
```
cd web
npm install && npm run build
```
