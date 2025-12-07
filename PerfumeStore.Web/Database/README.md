## Perfume Store Database Setup

PostgreSQL + Dapper + stored procedure stack.

### 1. Create database + role

```sql
create role perfume_admin with login password 'ChangeMe!';
create database perfume_store owner perfume_admin;
```

### 2. Apply schema & stored procedures

```bash
psql -U perfume_admin -d perfume_store -f perfume-store-schema.sql
```

### 3. Seed starter data

Add categories, products, payment methods, delivery options with SQL/psql. Samples:

```sql
insert into categories (name, description) values
('Citrus', 'Bright + effervescent'),
('Woody', 'Smoky amber accords'),
('Floral', 'Bouquet driven');

call sp_product_upsert(null, 'Golden Hour', 'Solar floral musk', 128, 110, '/images/golden-hour.jpg', true, true, 3, 50);
call sp_payment_method_upsert(null, 'Visa / Mastercard', 'Stripe', 1.90, false, true);
call sp_delivery_option_upsert(null, 'Express 48h', 'Bike courier in core metros', 9.00, 2, true);
```

### 4. Connection string

Update `appsettings.json`:

```json
"ConnectionStrings": {
  "PerfumeStore": "Host=localhost;Port=5432;Database=perfume_store;Username=perfume_admin;Password=ChangeMe!"
}
```

