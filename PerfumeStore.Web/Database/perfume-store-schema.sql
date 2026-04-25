-- PostgreSQL schema & stored procedures for Perfume Store

-- 1. Tables -------------------------------------------------------------------

create table if not exists categories (
    id              serial primary key,
    name            varchar(80) not null,
    description     varchar(255) not null default '',
    is_active       boolean not null default true,
    created_at      timestamptz not null default now()
);

create table if not exists products (
    id              serial primary key,
    name            varchar(120) not null,
    description     varchar(512) not null,
    price           numeric(10,2) not null,
    discount_price  numeric(10,2),
    image_url       varchar(512) not null,
    is_featured     boolean not null default false,
    is_trending     boolean not null default false,
    category_id     int not null references categories(id),
    stock_quantity  int not null default 0,
    created_at      timestamptz not null default now(),
    updated_at      timestamptz not null default now()
);

create table if not exists payment_methods (
    id                      serial primary key,
    name                    varchar(80) not null,
    provider                varchar(80) not null,
    payment_type            varchar(30) not null default 'manual',
    partner_name            varchar(80),
    processing_fee          numeric(10,2) not null default 0,
    supports_installments   boolean not null default false,
    account_title           varchar(120),
    account_number          varchar(80),
    bank_name               varchar(80),
    iban                    varchar(80),
    instructions            varchar(500),
    requires_receipt        boolean not null default true,
    is_active               boolean not null default true,
    created_at              timestamptz not null default now()
);

alter table payment_methods add column if not exists payment_type varchar(30) not null default 'manual';
alter table payment_methods add column if not exists partner_name varchar(80);
alter table payment_methods add column if not exists account_title varchar(120);
alter table payment_methods add column if not exists account_number varchar(80);
alter table payment_methods add column if not exists bank_name varchar(80);
alter table payment_methods add column if not exists iban varchar(80);
alter table payment_methods add column if not exists instructions varchar(500);
alter table payment_methods add column if not exists requires_receipt boolean not null default true;

create table if not exists delivery_options (
    id              serial primary key,
    name            varchar(80) not null,
    description     varchar(255) not null,
    fee             numeric(10,2) not null default 0,
    estimated_days  int not null,
    is_active       boolean not null default true,
    created_at      timestamptz not null default now()
);

create table if not exists hero_content (
    id                  int primary key default 1,
    title               varchar(120) not null,
    subtitle            varchar(255) not null,
    background_image    varchar(512) not null,
    primary_cta_text    varchar(60) not null,
    primary_cta_link    varchar(255) not null,
    secondary_cta_text  varchar(60),
    secondary_cta_link  varchar(255),
    updated_at          timestamptz not null default now()
);

create table if not exists orders (
    id                  serial primary key,
    order_number        varchar(32) not null unique,
    customer_name       varchar(120) not null,
    customer_email      varchar(120) not null,
    shipping_address    varchar(500) not null,
    payment_method_id   int not null references payment_methods(id),
    delivery_option_id  int not null references delivery_options(id),
    subtotal            numeric(10,2) not null,
    delivery_fee        numeric(10,2) not null,
    processing_fee      numeric(10,2) not null,
    total               numeric(10,2) not null,
    created_at          timestamptz not null default now()
);

alter table orders
    add column if not exists status varchar(30) not null default 'Processing';
alter table orders
    add column if not exists payment_status varchar(30) not null default 'Pending Review';
alter table orders
    add column if not exists payment_receipt_url varchar(512);
alter table orders
    add column if not exists payment_reference varchar(120);
alter table orders
    add column if not exists payment_review_notes varchar(300);

create table if not exists order_items (
    order_id    int not null references orders(id) on delete cascade,
    product_id  int not null references products(id),
    quantity    int not null,
    unit_price  numeric(10,2) not null
);


-- 2. Helper types -------------------------------------------------------------

drop type if exists order_item_type cascade;
create type order_item_type as (
    product_id int,
    quantity   int,
    unit_price numeric(10,2)
);


-- 3. Stored procedures --------------------------------------------------------

create or replace procedure sp_product_upsert(
    p_id int,
    p_name varchar,
    p_description varchar,
    p_price numeric,
    p_discount_price numeric,
    p_image_url varchar,
    p_is_featured boolean,
    p_is_trending boolean,
    p_category_id int,
    p_stock_quantity int
)
language plpgsql
as $$
begin
    if p_id is null or p_id = 0 then
        insert into products(name, description, price, discount_price, image_url, is_featured, is_trending, category_id, stock_quantity)
        values (p_name, p_description, p_price, p_discount_price, p_image_url, p_is_featured, p_is_trending, p_category_id, p_stock_quantity);
    else
        update products set
            name = p_name,
            description = p_description,
            price = p_price,
            discount_price = p_discount_price,
            image_url = p_image_url,
            is_featured = p_is_featured,
            is_trending = p_is_trending,
            category_id = p_category_id,
            stock_quantity = p_stock_quantity,
            updated_at = now()
        where id = p_id;
    end if;
end;
$$;

create or replace procedure sp_payment_method_upsert(
    p_id int,
    p_name varchar,
    p_provider varchar,
    p_payment_type varchar,
    p_partner_name varchar,
    p_processing_fee numeric,
    p_supports_installments boolean,
    p_is_active boolean,
    p_account_title varchar,
    p_account_number varchar,
    p_bank_name varchar,
    p_iban varchar,
    p_instructions varchar,
    p_requires_receipt boolean
)
language plpgsql
as $$
begin
    if p_id is null or p_id = 0 then
        insert into payment_methods (name, provider, payment_type, partner_name, processing_fee, supports_installments, is_active, account_title, account_number, bank_name, iban, instructions, requires_receipt)
        values (p_name, p_provider, p_payment_type, p_partner_name, p_processing_fee, p_supports_installments, p_is_active, p_account_title, p_account_number, p_bank_name, p_iban, p_instructions, p_requires_receipt);
    else
        update payment_methods set
            name = p_name,
            provider = p_provider,
            payment_type = p_payment_type,
            partner_name = p_partner_name,
            processing_fee = p_processing_fee,
            supports_installments = p_supports_installments,
            is_active = p_is_active,
            account_title = p_account_title,
            account_number = p_account_number,
            bank_name = p_bank_name,
            iban = p_iban,
            instructions = p_instructions,
            requires_receipt = p_requires_receipt
        where id = p_id;
    end if;
end;
$$;

create or replace procedure sp_delivery_option_upsert(
    p_id int,
    p_name varchar,
    p_description varchar,
    p_fee numeric,
    p_estimated_days int,
    p_is_active boolean
)
language plpgsql
as $$
begin
    if p_id is null or p_id = 0 then
        insert into delivery_options (name, description, fee, estimated_days, is_active)
        values (p_name, p_description, p_fee, p_estimated_days, p_is_active);
    else
        update delivery_options set
            name = p_name,
            description = p_description,
            fee = p_fee,
            estimated_days = p_estimated_days,
            is_active = p_is_active
        where id = p_id;
    end if;
end;
$$;

create or replace procedure sp_order_review_update(
    p_order_number varchar,
    p_status varchar,
    p_payment_status varchar,
    p_payment_review_notes varchar
)
language plpgsql
as $$
begin
    update orders
    set status = p_status,
        payment_status = p_payment_status,
        payment_review_notes = p_payment_review_notes
    where order_number = p_order_number;
end;
$$;

create or replace procedure sp_hero_content_upsert(
    p_title varchar,
    p_subtitle varchar,
    p_background_image_url varchar,
    p_primary_cta_text varchar,
    p_primary_cta_link varchar,
    p_secondary_cta_text varchar,
    p_secondary_cta_link varchar
)
language sql
as $$
    insert into hero_content(id, title, subtitle, background_image, primary_cta_text, primary_cta_link, secondary_cta_text, secondary_cta_link)
    values (1, p_title, p_subtitle, p_background_image_url, p_primary_cta_text, p_primary_cta_link, p_secondary_cta_text, p_secondary_cta_link)
    on conflict (id) do update set
        title = excluded.title,
        subtitle = excluded.subtitle,
        background_image = excluded.background_image,
        primary_cta_text = excluded.primary_cta_text,
        primary_cta_link = excluded.primary_cta_link,
        secondary_cta_text = excluded.secondary_cta_text,
        secondary_cta_link = excluded.secondary_cta_link,
        updated_at = now();
$$;

create or replace function f_products_featured() returns setof products
language sql
as $$ select * from products where is_featured order by updated_at desc limit 6 $$;


create or replace function f_products_trending() returns setof products
language sql
as $$ select * from products where is_trending order by updated_at desc limit 8 $$;

create or replace procedure sp_order_create(
    p_customer_name varchar,
    p_customer_email varchar,
    p_shipping_address varchar,
    p_payment_method_id int,
    p_delivery_option_id int,
    p_status varchar,
    p_payment_status varchar,
    p_payment_receipt_url varchar,
    p_payment_reference varchar,
    p_subtotal numeric,
    p_delivery_fee numeric,
    p_processing_fee numeric,
    p_total numeric,
    p_items order_item_type[],
    inout p_order_number varchar
)
language plpgsql
as $$
declare
    v_order_id int;
begin
    if exists (
        select 1
        from unnest(p_items) as item
        join products p on p.id = item.product_id
        where item.quantity > p.stock_quantity
    ) then
        raise exception 'Insufficient stock for one or more items.';
    end if;

    p_order_number := coalesce(p_order_number, concat('ORD-', to_char(clock_timestamp(), 'YYMMDDHH24MISSMS')));
    insert into orders(order_number, customer_name, customer_email, shipping_address, payment_method_id, delivery_option_id, status, payment_status, payment_receipt_url, payment_reference, subtotal, delivery_fee, processing_fee, total)
    values (p_order_number, p_customer_name, p_customer_email, p_shipping_address, p_payment_method_id, p_delivery_option_id, p_status, p_payment_status, p_payment_receipt_url, p_payment_reference, p_subtotal, p_delivery_fee, p_processing_fee, p_total)
    returning id into v_order_id;

    insert into order_items(order_id, product_id, quantity, unit_price)
    select v_order_id, item.product_id, item.quantity, item.unit_price
    from unnest(p_items) as item;

    update products p
    set stock_quantity = p.stock_quantity - item.quantity,
        updated_at = now()
    from unnest(p_items) as item
    where p.id = item.product_id;
end;
$$;

-- 4. Read-only stored procedures ---------------------------------------------


CREATE OR REPLACE FUNCTION sp_products_get_featured()
RETURNS TABLE (
    Id int,
    Name varchar,
    Description varchar,
    Price numeric,
    DiscountPrice numeric,
    ImageUrl varchar,
    IsFeatured boolean,
    IsTrending boolean,
    CategoryId int,
    CategoryName varchar,
    StockQuantity int
)
LANGUAGE sql
AS $$
    SELECT 
        p.id AS Id,
        p.name AS Name,
        p.description AS Description,
        p.price AS Price,
        p.discount_price AS DiscountPrice,
        p.image_url AS ImageUrl,
        p.is_featured AS IsFeatured,
        p.is_trending AS IsTrending,
        p.category_id AS CategoryId,
        c.name AS CategoryName,
        p.stock_quantity AS StockQuantity
    FROM products p
    JOIN categories c 
        ON c.id = p.category_id
		 where is_featured order by updated_at desc limit 6
$$;

CREATE OR REPLACE FUNCTION sp_products_get_trending()
RETURNS TABLE (
    Id int,
    Name varchar,
    Description varchar,
    Price numeric,
    DiscountPrice numeric,
    ImageUrl varchar,
    IsFeatured boolean,
    IsTrending boolean,
    CategoryId int,
    CategoryName varchar,
    StockQuantity int
)
LANGUAGE sql
AS $$
    SELECT 
        p.id AS Id,
        p.name AS Name,
        p.description AS Description,
        p.price AS Price,
        p.discount_price AS DiscountPrice,
        p.image_url AS ImageUrl,
        p.is_featured AS IsFeatured,
        p.is_trending AS IsTrending,
        p.category_id AS CategoryId,
        c.name AS CategoryName,
        p.stock_quantity AS StockQuantity
    FROM products p
    JOIN categories c 
        ON c.id = p.category_id
		 where is_trending order by updated_at desc limit 8
$$;


--create or replace function sp_products_get_featured()
--returns setof products
--language sql
--as $$ select * from f_products_featured(); $$;

--create or replace function sp_products_get_trending()
--returns setof products
--language sql
--as $$ select * from f_products_trending(); $$;

--create or replace function sp_products_get_all()
--returns table (
--    id int,
--    name varchar,
--    description varchar,
--    price numeric,
--    discount_price numeric,
--    image_url varchar,
--    is_featured boolean,
--    is_trending boolean,
--    category_id int,
--    category_name varchar,
--    stock_quantity int
--)
--language sql
--as $$
--    select p.id,
--           p.name,
--           p.description,
--           p.price,
--           p.discount_price,
--           p.image_url,
--           p.is_featured,
--           p.is_trending,
--           p.category_id,
--           c.name as category_name,
--           p.stock_quantity
--    from products p
--    join categories c on c.id = p.category_id;
--$$;

create or replace function sp_products_get_all()
returns table (
    Id int,
  Name varchar,
  Description varchar,
  Price numeric,
  DiscountPrice numeric,
  ImageUrl varchar,
  IsFeatured boolean,
  IsTrending boolean,
  CategoryId int,
  CategoryName varchar,
  StockQuantity int
)
language sql
as $$
    select   p.id AS Id,
  p.name AS Name,
  p.description AS Description,
  p.price AS Price,
  p.discount_price AS DiscountPrice,
  p.image_url AS ImageUrl,
  p.is_featured AS IsFeatured,
  p.is_trending AS IsTrending,
  p.category_id AS CategoryId,
  c.name AS CategoryName,
  p.stock_quantity AS StockQuantity
    from products p
    join categories c on c.id = p.category_id;
$$;




create or replace function sp_categories_get_all()
returns table (
	  Id int,
Name varchar,
Description varchar,
	IsActive boolean
)
language sql
as $$ select Id,Name,Description,
Is_Active
IsActive from categories order by name; $$;



CREATE OR REPLACE FUNCTION sp_payment_methods_get_all()
RETURNS TABLE (
    Id int,
    Name varchar,
    Provider varchar,
    PaymentType varchar,
    PartnerName varchar,
    ProcessingFee numeric,
    SupportsInstallments boolean,
    IsActive boolean,
    AccountTitle varchar,
    AccountNumber varchar,
    BankName varchar,
    Iban varchar,
    Instructions varchar,
    RequiresReceipt boolean
)
LANGUAGE sql
AS $$
    SELECT 
        pm.id AS Id,
        pm.name AS Name,
        pm.provider AS Provider,
        pm.payment_type AS PaymentType,
        pm.partner_name AS PartnerName,
        pm.processing_fee AS ProcessingFee,
        pm.supports_installments AS SupportsInstallments,
        pm.is_active AS IsActive,
        pm.account_title AS AccountTitle,
        pm.account_number AS AccountNumber,
        pm.bank_name AS BankName,
        pm.iban AS Iban,
        pm.instructions AS Instructions,
        pm.requires_receipt AS RequiresReceipt
    FROM payment_methods pm
    ORDER BY pm.provider;
$$;




CREATE OR REPLACE FUNCTION sp_delivery_options_get_all()
RETURNS TABLE (
    Id int,
    Name varchar,
    Description varchar,
    Fee numeric,
    EstimatedDays int,
    IsActive boolean
)
LANGUAGE sql
AS $$
    SELECT 
        d.id AS Id,
        d.name AS Name,
        d.description AS Description,
        d.fee AS Fee,
        d.estimated_days AS EstimatedDays,
        d.is_active AS IsActive
    FROM delivery_options d
    ORDER BY d.estimated_days;
$$;


--create or replace function sp_admin_dashboard_get_stats()
--returns table(
--    active_products int,
--    active_payment_methods int,
--    active_delivery_options int,
--    today_revenue numeric,
--    pending_orders int
--)
--language sql
--as $$
--    select
--        (select count(*) from products where is_featured) as active_products,
--        (select count(*) from payment_methods where is_active) as active_payment_methods,
--        (select count(*) from delivery_options where is_active) as active_delivery_options,
--        coalesce((select sum(total) from orders where created_at::date = now()::date), 0)::numeric as today_revenue,
--        (select count(*) from orders where total > 0) as pending_orders;
--$$;

create or replace function sp_admin_dashboard_get_stats()
returns table(
    ActiveProducts int,
    ActivePaymentMethods int,
    ActiveDeliveryOptions int,
    TodayRevenue numeric,
    PendingOrders int
)
language sql
as $$
    select
        (select count(*) from products where is_featured) as ActiveProducts,
        (select count(*) from payment_methods where is_active) as ActivePaymentMethods,
        (select count(*) from delivery_options where is_active) as ActiveDeliveryOptions,
        coalesce((select sum(total) from orders where created_at::date = now()::date), 0)::numeric as TodayRevenue,
        (select count(*) from orders where total > 0) as PendingOrders;
$$;

create or replace function sp_admin_orders_get_recent()
returns table(
    OrderNumber varchar,
    CustomerName varchar,
    CustomerEmail varchar,
    ShippingAddress varchar,
    Status varchar,
    PaymentStatus varchar,
    PaymentMethod varchar,
    PaymentMethodId int,
    PaymentType varchar,
    DeliveryOption varchar,
    ItemCount int,
    Total numeric,
    CreatedAt timestamptz,
    PaymentReceiptUrl varchar,
    PaymentReference varchar,
    PaymentReviewNotes varchar
)
language sql
as $$
    select
        o.order_number as OrderNumber,
        o.customer_name as CustomerName,
        o.customer_email as CustomerEmail,
        o.shipping_address as ShippingAddress,
        o.status as Status,
        o.payment_status as PaymentStatus,
        pm.name as PaymentMethod,
        pm.id as PaymentMethodId,
        pm.payment_type as PaymentType,
        d.name as DeliveryOption,
        coalesce(sum(oi.quantity), 0)::int as ItemCount,
        o.total as Total,
        o.created_at as CreatedAt,
        o.payment_receipt_url as PaymentReceiptUrl,
        o.payment_reference as PaymentReference,
        o.payment_review_notes as PaymentReviewNotes
    from orders o
    join payment_methods pm on pm.id = o.payment_method_id
    join delivery_options d on d.id = o.delivery_option_id
    left join order_items oi on oi.order_id = o.id
    group by o.order_number, o.customer_name, o.customer_email, o.shipping_address, o.status, o.payment_status, pm.name, pm.id, pm.payment_type, d.name, o.total, o.created_at, o.payment_receipt_url, o.payment_reference, o.payment_review_notes
    order by o.created_at desc
    limit 12;
$$;

create or replace function sp_admin_customers_get_top()
returns table(
    FullName varchar,
    Email varchar,
    OrderCount int,
    LifetimeValue numeric,
    WishlistCount int,
    LastOrderAt timestamptz
)
language sql
as $$
    select
        nullif(u.full_name, '') as FullName,
        u.username as Email,
        count(o.id)::int as OrderCount,
        coalesce(sum(o.total), 0)::numeric as LifetimeValue,
        (
            select count(*)
            from customer_wishlist cw
            where cw.user_id = u.id
        )::int as WishlistCount,
        max(o.created_at) as LastOrderAt
    from app_users u
    left join orders o on lower(o.customer_email) = lower(u.username)
    where u.role = 'customer'
    group by u.id, u.full_name, u.username
    order by coalesce(sum(o.total), 0) desc, max(o.created_at) desc nulls last
    limit 8;
$$;



--create or replace function sp_hero_content_get()
--returns table(
--    title varchar,
--    subtitle varchar,
--    background_image_url varchar,
--    primary_cta_text varchar,
--    primary_cta_link varchar,
--    secondary_cta_text varchar,
--    secondary_cta_link varchar
--)
--language sql
--as $$ select title, subtitle, background_image, primary_cta_text, primary_cta_link, secondary_cta_text, secondary_cta_link from hero_content limit 1; $$;

create or replace function sp_hero_content_get()
returns table(
    Title varchar,
    Subtitle varchar,
    BackgroundImageUrl varchar,
    PrimaryCtaText varchar,
    PrimaryCtaLink varchar,
    SecondaryCtaText varchar,
    SecondaryCtaLink varchar
)
language sql
as $$ select title, subtitle, background_image, primary_cta_text, primary_cta_link, secondary_cta_text, secondary_cta_link from hero_content limit 1; $$;


-- 5. User management ----------------------------------------------------------

CREATE EXTENSION IF NOT EXISTS pgcrypto;

create table if not exists app_users (
    id              serial primary key,
    full_name       varchar(120) not null default '',
    username        varchar(80) not null unique,
    password_hash   text not null,
    role            varchar(20) not null default 'customer',
    is_active       boolean not null default true,
    created_at      timestamptz not null default now()
);

alter table app_users
    add column if not exists full_name varchar(120) not null default '';

update app_users
set username = lower(trim(username))
where username <> lower(trim(username));

create unique index if not exists ix_app_users_username_lower
    on app_users (lower(username));

create table if not exists customer_wishlist (
    user_id      int not null references app_users(id) on delete cascade,
    product_id   int not null references products(id) on delete cascade,
    created_at   timestamptz not null default now(),
    primary key (user_id, product_id)
);

insert into app_users (username, password_hash, role)
values ('admin', crypt('password', gen_salt('bf')), 'admin')
on conflict (username) do nothing;

create or replace function sp_user_get_by_username(
    p_username varchar
)
returns table (
    Id int,
    FullName varchar,
    Username varchar,
    PasswordHash text,
    Role varchar,
    IsActive boolean,
    CreatedAt timestamptz
)
language sql
as $$
    select
        id,
        full_name,
        username,
        password_hash,
        role,
        is_active,
        created_at
    from app_users
    where lower(username) = lower(trim(p_username)) and is_active;
$$;

create or replace function sp_customer_orders_get_by_email(
    p_customer_email varchar
)
returns table (
    OrderNumber varchar,
    Status varchar,
    PaymentStatus varchar,
    PaymentMethod varchar,
    DeliveryOption varchar,
    EstimatedDays int,
    ItemCount int,
    Total numeric,
    CreatedAt timestamptz,
    PaymentReceiptUrl varchar,
    PaymentReference varchar
)
language sql
as $$
    select
        o.order_number as OrderNumber,
        o.status as Status,
        o.payment_status as PaymentStatus,
        pm.name as PaymentMethod,
        d.name as DeliveryOption,
        d.estimated_days as EstimatedDays,
        coalesce(sum(oi.quantity), 0)::int as ItemCount,
        o.total as Total,
        o.created_at as CreatedAt,
        o.payment_receipt_url as PaymentReceiptUrl,
        o.payment_reference as PaymentReference
    from orders o
    join payment_methods pm on pm.id = o.payment_method_id
    join delivery_options d on d.id = o.delivery_option_id
    left join order_items oi on oi.order_id = o.id
    where lower(o.customer_email) = lower(trim(p_customer_email))
    group by o.order_number, o.status, o.payment_status, pm.name, d.name, d.estimated_days, o.total, o.created_at, o.payment_receipt_url, o.payment_reference
    order by o.created_at desc;
$$;

create or replace function sp_customer_wishlist_get_by_username(
    p_username varchar
)
returns table (
    Id int,
    Name varchar,
    Description varchar,
    Price numeric,
    DiscountPrice numeric,
    ImageUrl varchar,
    IsFeatured boolean,
    IsTrending boolean,
    CategoryId int,
    CategoryName varchar,
    StockQuantity int
)
language sql
as $$
    select
        p.id as Id,
        p.name as Name,
        p.description as Description,
        p.price as Price,
        p.discount_price as DiscountPrice,
        p.image_url as ImageUrl,
        p.is_featured as IsFeatured,
        p.is_trending as IsTrending,
        p.category_id as CategoryId,
        c.name as CategoryName,
        p.stock_quantity as StockQuantity
    from customer_wishlist cw
    join app_users u on u.id = cw.user_id
    join products p on p.id = cw.product_id
    join categories c on c.id = p.category_id
    where lower(u.username) = lower(trim(p_username))
    order by cw.created_at desc;
$$;
