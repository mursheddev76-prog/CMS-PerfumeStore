# Quick Reference - Perfume Store UI/UX

## CSS Variables - Copy & Paste

```css
/* Color Palette */
--primary: #c68740;
--primary-dark: #a86e2d;
--secondary: #0b0b0f;
--accent: #d4a574;
--light-bg: #fdfcfb;
--white: #ffffff;
--text-dark: #0b0b0f;
--text-muted: #6b7280;

/* Borders & Shadows */
--border-light: rgba(0, 0, 0, 0.08);
--shadow-sm: 0 4px 12px rgba(0, 0, 0, 0.08);
--shadow-md: 0 10px 25px rgba(0, 0, 0, 0.1);
--shadow-lg: 0 20px 50px rgba(0, 0, 0, 0.12);
```

## Common Classes

### Cards
```html
<!-- Product Card -->
<article class="product-card">
  <div class="product-image" style="background-image:url(...)"></div>
  <div class="product-body">
    <p class="text-muted">CATEGORY</p>
    <h3>Product Name</h3>
    <p class="small text-muted">Description</p>
    <div class="pricing my-3">
      <span class="price-discount">$79.99</span>
      <span class="price-original">$99.99</span>
    </div>
  </div>
</article>

<!-- Stat Card -->
<div class="stat-card">
  <p>Label</p>
  <h3>123</h3>
  <small class="text-muted">Sub-text</small>
</div>

<!-- Feature Card -->
<div class="fulfillment-badge">
  <i class="bi bi-icon"></i>
  <h4>Title</h4>
  <p>Description</p>
</div>
```

### Forms
```html
<!-- Basic Input -->
<label class="form-label">Label</label>
<input class="form-control form-control-lg" />

<!-- Checkbox -->
<div class="form-check">
  <input class="form-check-input" type="checkbox" id="id" />
  <label class="form-check-label" for="id">Label</label>
</div>

<!-- Select -->
<select class="form-select">
  <option>Option</option>
</select>
```

### Buttons
```html
<!-- Primary -->
<button class="btn btn-dark">Button</button>

<!-- Outline -->
<button class="btn btn-outline-dark">Outline</button>

<!-- With Icon -->
<button class="btn btn-dark">
  <i class="bi bi-icon me-2"></i>Button
</button>

<!-- Large -->
<button class="btn btn-dark btn-lg">Large Button</button>

<!-- Rounded -->
<button class="btn btn-dark rounded-pill">Rounded</button>
```

### Sections
```html
<!-- Hero -->
<section class="hero" style="background-image:url(...)">
  <div class="hero-overlay"></div>
  <div class="hero-content container text-center text-white">
    <p class="eyebrow">EYEBROW TEXT</p>
    <h1>Heading</h1>
    <p class="lead">Subtitle</p>
    <div class="hero-ctas">
      <a class="btn btn-lg btn-primary">Primary</a>
      <a class="btn btn-lg btn-outline-light">Secondary</a>
    </div>
    <div class="hero-highlights">
      <span class="badge">Badge</span>
      <span class="badge">Badge</span>
    </div>
  </div>
</section>

<!-- Section Heading -->
<div class="section-heading">
  <p class="eyebrow">EYEBROW</p>
  <h2>Main Heading</h2>
  <p class="text-muted">Supporting text</p>
</div>

<!-- CTA Panel -->
<section class="cta-panel text-center text-white py-5">
  <div class="container">
    <p class="eyebrow">EYEBROW</p>
    <h2>Heading</h2>
    <p>Text</p>
    <a class="btn btn-outline-light btn-lg">Button</a>
  </div>
</section>
```

### Alerts
```html
<!-- Success -->
<div class="alert alert-success alert-dismissible fade show" role="alert">
  <i class="bi bi-check-circle me-2"></i>
  <strong>Success!</strong> Message here
  <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
</div>

<!-- Danger -->
<div class="alert alert-danger alert-dismissible fade show" role="alert">
  <i class="bi bi-exclamation-circle me-2"></i>
  <strong>Error!</strong> Message here
  <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
</div>
```

## Spacing Utilities

### Padding
- `p-3` = 1rem
- `p-4` = 1.5rem
- `p-5` = 3rem
- `py-5` = 3rem vertical
- `px-4` = 1.5rem horizontal

### Margin
- `my-3` = 1rem vertical
- `mb-4` = 1.5rem bottom
- `mt-5` = 3rem top
- `gap-3` = 1rem gap (flex/grid)
- `gap-4` = 1.5rem gap

## Icons (Bootstrap Icons)

### Common Icons
```html
<i class="bi bi-search"></i>          <!-- Search -->
<i class="bi bi-bag-check"></i>       <!-- Shopping bag -->
<i class="bi bi-heart"></i>           <!-- Heart -->
<i class="bi bi-eye"></i>             <!-- View -->
<i class="bi bi-arrow-right"></i>     <!-- Arrow -->
<i class="bi bi-check-circle"></i>    <!-- Check -->
<i class="bi bi-exclamation-circle"></i> <!-- Warning -->
<i class="bi bi-lock-fill"></i>       <!-- Lock -->
<i class="bi bi-credit-card"></i>     <!-- Credit card -->
<i class="bi bi-truck"></i>           <!-- Truck -->
<i class="bi bi-graph-up"></i>        <!-- Analytics -->
<i class="bi bi-box-seam"></i>        <!-- Package -->
<i class="bi bi-shield-check"></i>    <!-- Shield -->
<i class="bi bi-lightning-charge"></i> <!-- Fast -->
<i class="bi bi-percent"></i>         <!-- Percent -->
<i class="bi bi-image"></i>           <!-- Image -->
<i class="bi bi-save"></i>            <!-- Save -->
<i class="bi bi-filter"></i>          <!-- Filter -->
<i class="bi bi-fire"></i>            <!-- Trending -->
```

## Typography Classes

```html
<!-- Headings -->
<h1>Display heading</h1>
<h2>Large heading</h2>
<h3>Medium heading</h3>
<h4>Small heading</h4>
<h5>Smaller heading</h5>

<!-- Text Colors -->
<p class="text-muted">Muted text</p>
<p class="text-dark">Dark text</p>
<p class="text-danger">Error text</p>
<p class="text-success">Success text</p>

<!-- Text Styles -->
<p class="fw-bold">Bold text</p>
<p class="text-uppercase">UPPERCASE</p>
<p class="text-decoration-line-through">Strikethrough</p>
<p class="small">Small text</p>

<!-- Eyebrow -->
<p class="eyebrow">EYEBROW TEXT</p>

<!-- Lead -->
<p class="lead">Lead paragraph</p>
```

## Grid Layout

```html
<!-- 3-Column Grid -->
<div class="row g-4">
  <div class="col-lg-4 col-md-6">
    <!-- Content -->
  </div>
</div>

<!-- 2-Column Layout -->
<div class="row g-4">
  <div class="col-lg-8">
    <!-- Main content -->
  </div>
  <div class="col-lg-4">
    <!-- Sidebar -->
  </div>
</div>

<!-- Flex Layout -->
<div class="d-flex gap-3 align-items-center justify-content-between">
  <!-- Content -->
</div>
```

## Responsive Classes

```html
<!-- Hide on mobile -->
<div class="d-none d-lg-block">Desktop only</div>

<!-- Show on mobile only -->
<div class="d-lg-none">Mobile only</div>

<!-- Responsive text alignment -->
<div class="text-center text-lg-left">Text</div>

<!-- Responsive columns -->
<div class="col-12 col-md-6 col-lg-4">Content</div>
```

## Common Patterns

### Product Grid
```html
<div class="row g-4">
  @foreach (var product in products)
  {
    <div class="col-lg-4 col-md-6">
      <article class="product-card">
        <div class="product-image" style="background-image:url('@product.ImageUrl')"></div>
        <div class="product-body">
          <p class="text-muted">@product.Category</p>
          <h3>@product.Name</h3>
          <p class="small">@product.Description</p>
          <div class="pricing my-3">
            <span class="price">@product.Price.ToString("C")</span>
          </div>
          <a class="btn btn-dark w-100">Add to Cart</a>
        </div>
      </article>
    </div>
  }
</div>
```

### Stat Card Grid
```html
<div class="row g-3">
  <div class="col-sm-6 col-lg-2">
    <div class="stat-card">
      <p>Label</p>
      <h3>123</h3>
      <small class="text-muted">Sub-text</small>
    </div>
  </div>
</div>
```

### Form Section
```html
<div class="card">
  <div class="card-header">
    <h4><i class="bi bi-icon me-2"></i>Section Title</h4>
  </div>
  <div class="card-body">
    <div class="row g-3">
      <div class="col-md-6">
        <label class="form-label">Field Label</label>
        <input class="form-control" />
      </div>
    </div>
  </div>
</div>
```

## Common Snippets

### Empty State
```html
<div class="alert alert-info text-center py-5">
  <i class="bi bi-info-circle me-2"></i>
  <strong>No items found</strong>
  <p class="mb-0 mt-2">Try adjusting your search or filters</p>
</div>
```

### Loading
```html
<div class="text-center py-5">
  <div class="spinner-border" role="status">
    <span class="visually-hidden">Loading...</span>
  </div>
</div>
```

### Sticky Sidebar
```html
<div class="card sticky-top" style="top: 100px;">
  <!-- Sidebar content -->
</div>
```

## Text Truncation

```html
<!-- Single line truncate -->
<p class="text-truncate">Long text...</p>

<!-- Multi-line truncate (3 lines) -->
<p style="display: -webkit-box; -webkit-line-clamp: 3; -webkit-box-orient: vertical; overflow: hidden;">
  Long text...
</p>
```

## Hover Effects

```css
/* Hover text color change */
a:hover {
  color: var(--primary-dark);
}

/* Hover lift effect */
.product-card:hover {
  transform: translateY(-8px);
  box-shadow: var(--shadow-lg);
}

/* Hover background change */
.form-check:hover {
  background-color: rgba(198, 135, 64, 0.05);
}
```

## Colors Reference

### Usage
```css
/* Background colors */
background-color: var(--primary);
background-color: var(--light-bg);

/* Text colors */
color: var(--text-dark);
color: var(--text-muted);

/* Border colors */
border-color: var(--border-light);
border-color: var(--primary);

/* Shadow */
box-shadow: var(--shadow-md);
```

## Breakpoints

```css
/* Mobile first */
/* 0px - 576px */    Default
/* 576px - 768px */  sm
/* 768px - 992px */  md
/* 992px - 1200px */ lg
/* 1200px+ */        xl
```

---

**For complete documentation, see:**
- `UI_UX_IMPROVEMENTS.md` - Full overview
- `CSS_CUSTOMIZATION_GUIDE.md` - Detailed customization guide
- `TRANSFORMATION_SUMMARY.md` - Before/after summary
