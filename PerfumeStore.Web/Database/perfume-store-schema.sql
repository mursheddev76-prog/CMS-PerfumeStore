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
    processing_fee          numeric(10,2) not null default 0,
    supports_installments   boolean not null default false,
    is_active               boolean not null default true,
    created_at              timestamptz not null default now()
);

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
    p_processing_fee numeric,
    p_supports_installments boolean,
    p_is_active boolean
)
language sql
as $$
    insert into payment_methods (id, name, provider, processing_fee, supports_installments, is_active)
    values (p_id, p_name, p_provider, p_processing_fee, p_supports_installments, p_is_active)
    on conflict (id) do update set
        name = excluded.name,
        provider = excluded.provider,
        processing_fee = excluded.processing_fee,
        supports_installments = excluded.supports_installments,
        is_active = excluded.is_active;
$$;

create or replace procedure sp_delivery_option_upsert(
    p_id int,
    p_name varchar,
    p_description varchar,
    p_fee numeric,
    p_estimated_days int,
    p_is_active boolean
)
language sql
as $$
    insert into delivery_options (id, name, description, fee, estimated_days, is_active)
    values (p_id, p_name, p_description, p_fee, p_estimated_days, p_is_active)
    on conflict (id) do update set
        name = excluded.name,
        description = excluded.description,
        fee = excluded.fee,
        estimated_days = excluded.estimated_days,
        is_active = excluded.is_active;
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
    p_order_number := coalesce(p_order_number, concat('ORD-', to_char(clock_timestamp(), 'YYMMDDHH24MISSMS')));
    insert into orders(order_number, customer_name, customer_email, shipping_address, payment_method_id, delivery_option_id, subtotal, delivery_fee, processing_fee, total)
    values (p_order_number, p_customer_name, p_customer_email, p_shipping_address, p_payment_method_id, p_delivery_option_id, p_subtotal, p_delivery_fee, p_processing_fee, p_total)
    returning id into v_order_id;

    insert into order_items(order_id, product_id, quantity, unit_price)
    select v_order_id, item.product_id, item.quantity, item.unit_price
    from unnest(p_items) as item;
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
IsActive from categories where is_active order by name; $$;



CREATE OR REPLACE FUNCTION sp_payment_methods_get_all()
RETURNS TABLE (
    Id int,
    Name varchar,
    Provider varchar,
    ProcessingFee numeric,
    SupportsInstallments boolean,
    IsActive boolean
)
LANGUAGE sql
AS $$
    SELECT 
        pm.id AS Id,
        pm.name AS Name,
        pm.provider AS Provider,
        pm.processing_fee AS ProcessingFee,
        pm.supports_installments AS SupportsInstallments,
        pm.is_active AS IsActive
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

