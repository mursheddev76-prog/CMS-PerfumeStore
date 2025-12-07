# Perfume Store – ASP.NET 8 Commerce Stack

End-to-end perfume commerce experience with a premium landing page, dynamic admin console, PostgreSQL + Dapper data layer, checkout pipeline, and REST endpoints.

## Stack
- ASP.NET Core 8 MVC (controllers + Razor views)
- PostgreSQL with stored procedures (see `PerfumeStore.Web/Database`)
- Dapper micro ORM
- Bootstrap 5 + custom CSS
- Health checks (`/healthz`)

## Projects
| Project | Description |
| --- | --- |
| `PerfumeStore.Web` | Web + API project containing customer landing, catalog, checkout, admin area, services, repositories, and Dapper data access. |

## Getting Started
1. **Restore & build**
   ```bash
   dotnet restore
   dotnet build PerfumeStore.sln
   ```
2. **Database**
   - Install PostgreSQL.
   - Run the SQL in `PerfumeStore.Web/Database/perfume-store-schema.sql`.
   - Seed data as desired (samples in `Database/README.md`).
   - Update `appsettings*.json` connection string if needed.
3. **Run**
   ```bash
   dotnet run --project PerfumeStore.Web
   ```
4. **Navigate**
   - `/` landing page (customers)
   - `/catalog` advanced listing/search
   - `/checkout` payment + delivery pipeline
   - `/admin` command center (hero, products, payment methods, delivery options)
   - `/api/products`, `/api/configuration/*` for dynamic data (AJAX, mobile, etc.)

## Data & Dapper
- `Data/Repositories/CommerceRepository` encapsulates every stored procedure call.
- All writes go through proc wrappers (`sp_product_upsert`, `sp_order_create`, etc.).
- `Services/*` provide higher-level orchestration for controllers and Razor views.

## Frontend Experience
- Luxury hero section, featured/trending cards, fulfillment badges, CTA panel.
- Catalog filters & search hitting the API/service layer.
- Checkout page wired to admin-managed payment + delivery options.
- Admin dashboard with live stats, forms, and tables, all bound to the Postgres procs.

## Health & Observability
- `/healthz` leverages `AspNetCore.HealthChecks.NpgSql` against the configured DB.
- Response caching enabled for faster landing/catalog loads.

## Next Ideas
- Plug a payment gateway SDK for real captures.
- Add authentication for admins.
- Build a Blazor/MAUI client against the same API controllers.

