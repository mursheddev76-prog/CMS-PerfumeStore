# CSS Customization Guide - Perfume Store

## CSS Variables Reference

All colors and design tokens are now controlled by CSS variables at the root level.

### Color System

```css
:root {
  --primary: #c68740;           /* Main brand color (gold) */
  --primary-dark: #a86e2d;      /* Darker shade for interactions */
  --secondary: #0b0b0f;         /* Dark background/text */
  --accent: #d4a574;            /* Light gold accent */
  --light-bg: #fdfcfb;          /* Off-white background */
  --white: #ffffff;             /* Pure white */
  --text-dark: #0b0b0f;         /* Primary text */
  --text-muted: #6b7280;        /* Secondary text */
  --border-light: rgba(0, 0, 0, 0.08);  /* Light borders */
}
```

### Shadow System

```css
--shadow-sm: 0 4px 12px rgba(0, 0, 0, 0.08);     /* Subtle elevation */
--shadow-md: 0 10px 25px rgba(0, 0, 0, 0.1);     /* Medium elevation */
--shadow-lg: 0 20px 50px rgba(0, 0, 0, 0.12);    /* Strong elevation */
```

## Changing Brand Colors

To change the brand color throughout the entire application, simply modify the CSS variable:

```css
:root {
  --primary: #YOUR_COLOR;
  --primary-dark: #YOUR_DARKER_COLOR;
  --accent: #YOUR_ACCENT;
}
```

**Example - Changing to Purple:**
```css
:root {
  --primary: #9333ea;        /* Purple */
  --primary-dark: #7e22ce;   /* Dark purple */
  --accent: #c084fc;         /* Light purple */
}
```

## Component Classes Reference

### Product Cards
```html
<article class="product-card">
  <div class="product-image"></div>
  <div class="product-body">
    <p class="text-muted"></p>
    <h3></h3>
    <div class="pricing">
      <span class="price-discount"></span>
      <span class="price-original"></span>
    </div>
  </div>
</article>
```

**Key features:**
- Hover effect with lift animation
- Responsive image aspect ratio
- Better card shadow on hover

### Stat Cards
```html
<div class="stat-card">
  <p>Label</p>
  <h3>Value</h3>
  <small class="text-muted">Sub-text</small>
</div>
```

**Key features:**
- Uppercase label with letter-spacing
- Large number display
- Hover effect with shadow

### Form Elements
```html
<input class="form-control form-control-lg" />
<select class="form-select"></select>
<div class="form-check">
  <input class="form-check-input" type="checkbox" />
  <label class="form-check-label"></label>
</div>
```

**Key features:**
- Consistent border styling
- Focus state with colored border
- Rounded corners
- Better hover feedback

### Buttons
```html
<button class="btn btn-dark">Button</button>
<button class="btn btn-outline-dark">Outline Button</button>
```

**Key features:**
- Rounded corners (999px)
- Hover lift effect
- Better focus states
- Responsive padding

### Section Headings
```html
<div class="section-heading">
  <p class="eyebrow">Eyebrow text</p>
  <h2>Main Heading</h2>
  <p class="text-muted">Supporting text</p>
</div>
```

**Key features:**
- Eyebrow text with primary color
- Large responsive heading
- Proper spacing

### Hero Section
```html
<section class="hero" style="background-image:url(...)">
  <div class="hero-overlay"></div>
  <div class="hero-content">
    <p class="eyebrow"></p>
    <h1></h1>
    <p class="lead"></p>
    <div class="hero-ctas">
      <a class="btn btn-primary"></a>
      <a class="btn btn-outline-light"></a>
    </div>
    <div class="hero-highlights">
      <span class="badge"></span>
    </div>
  </div>
</section>
```

**Key features:**
- Background image with overlay
- Responsive typography
- Smooth entrance animations
- Badge styling

### Fulfillment Badges
```html
<div class="fulfillment-badge">
  <i class="bi bi-icon"></i>
  <h4>Title</h4>
  <p>Description</p>
</div>
```

**Key features:**
- Icon support
- Hover lift effect
- Better spacing

### CTA Panel
```html
<section class="cta-panel text-center text-white">
  <div class="container">
    <p class="eyebrow"></p>
    <h2></h2>
    <a class="btn btn-outline-light btn-lg"></a>
  </div>
</section>
```

**Key features:**
- Gradient background
- White text
- Outline buttons

## Responsive Breakpoints

```css
@media (max-width: 1200px) {
  /* Large tablet & small desktop */
}

@media (max-width: 992px) {
  /* Tablet */
}

@media (max-width: 768px) {
  /* Mobile */
}
```

## Animation Classes

### Fade In Up
```css
@keyframes fadeInUp {
  from {
    opacity: 0;
    transform: translateY(30px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}
```

Usage:
```css
.element {
  animation: fadeInUp 0.8s ease;
}
```

### Slide Up
```css
@keyframes slideUp {
  from {
    opacity: 0;
    transform: translateY(30px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}
```

### Fade In
```css
@keyframes fadeIn {
  from {
    opacity: 0;
  }
  to {
    opacity: 1;
  }
}
```

## Hover States

Most interactive elements have enhanced hover states:

```css
/* Buttons */
.btn-dark:hover {
  background-color: var(--primary);
  border-color: var(--primary);
  transform: translateY(-2px);
}

/* Product Cards */
.product-card:hover {
  box-shadow: var(--shadow-lg);
  transform: translateY(-8px);
}

/* Links */
a:hover {
  color: var(--primary-dark);
}

/* Nav Links */
.navbar-nav .nav-link:hover::after {
  width: 100%;
}
```

## Common Customizations

### Change Primary Color
```css
:root {
  --primary: #YOUR_COLOR;
  --primary-dark: #YOUR_DARKER_COLOR;
  --accent: #YOUR_LIGHT_COLOR;
}
```

### Adjust Font Sizes
Look for `clamp()` values in typography rules:
```css
h1 {
  font-size: clamp(2.5rem, 6vw, 4.5rem);
  /* min-size, preferred-size, max-size */
}
```

### Modify Spacing
Most spacing uses Bootstrap's grid system (g-3, g-4, etc.)
- `g-3`: 1rem gap
- `g-4`: 1.5rem gap
- `g-5`: 3rem gap

### Change Border Radius
Most components use consistent border-radius:
- Buttons: `border-radius: 999px` (fully rounded)
- Cards: `border-radius: 1.5rem`
- Inputs: `border-radius: 0.75rem`

### Adjust Shadows
Modify the shadow variables for darker/lighter shadows:
```css
--shadow-md: 0 10px 25px rgba(0, 0, 0, 0.15); /* Stronger */
--shadow-md: 0 10px 25px rgba(0, 0, 0, 0.05); /* Lighter */
```

## Dark Mode Support (Optional)

To add dark mode, create a media query:

```css
@media (prefers-color-scheme: dark) {
  :root {
    --light-bg: #1f1f1f;
    --white: #2d2d2d;
    --text-dark: #f5f5f5;
    --text-muted: #a0a0a0;
    --border-light: rgba(255, 255, 255, 0.1);
  }
}
```

## Debugging Tips

1. **Check CSS Variables**: Open DevTools and check the Styles tab for variable values
2. **Inspect Shadows**: Use box-shadow in DevTools to see shadow effects
3. **Test Responsive**: Use DevTools device toolbar to test breakpoints
4. **Check Animations**: Use DevTools animations panel to debug transitions

## Performance Considerations

- All shadows use `rgba()` for better performance
- Animations use `transform` and `opacity` for GPU acceleration
- Media queries are mobile-first for better performance
- No expensive computed values in loops

## Browser Support

- **Chrome/Edge**: Full support
- **Firefox**: Full support
- **Safari**: Full support (iOS 13+)
- **IE**: Not supported (modern CSS variables not available)

---

For questions or issues, refer to the main UI/UX improvements document.
