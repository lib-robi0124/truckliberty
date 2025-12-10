# VozilaNajava Authentication System

## Overview

This application uses **custom session-based authentication**, NOT ASP.NET Core Identity. The authentication system is implemented in `HomeController` using custom services (`IUserService` and `ITansporterService`).

## Authentication Architecture

### Two Types of Users

1. **Admin/User** (stored in `Users` table)
   - Authenticates with: `FullName` + `Password`
   - Example: "System Admin" / "admin123"
   - Has roles: Admin, Transporter (role-based)
   - Managed by `IUserService.AuthenticateAsync()`

2. **Transporter** (stored in `Transporters` table)
   - Authenticates with: `Email` + `Password`
   - Example: "info@translogistika.mk" / "trans123"
   - Represents transport companies
   - Managed by `ITansporterService.LoginTransporterAsync()`

### Domain Models

```csharp
// User table (Admin users)
public class User
{
    public int Id { get; set; }
    public string FullName { get; set; }        // Used as login identifier
    public string Password { get; set; }        // Plain text for testing
    public int RoleId { get; set; }
    public Role Role { get; set; }              // Admin or Transporter role
    public int? TransporterId { get; set; }
    public Transporter? Transporter { get; set; }
}

// Transporter table (Company users)
public class Transporter
{
    public int Id { get; set; }
    public string CompanyName { get; set; }
    public string ContactPerson { get; set; }
    public string PhoneNumber { get; set; }
    public string Email { get; set; }           // Used as login identifier
    public string Password { get; set; }        // Plain text for testing
}
```

## Login Flow

### HomeController.Login (POST)

```csharp
public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
{
    // 1. Try Admin/User authentication (FullName-based)
    var adminUser = await _userService.AuthenticateAsync(model.Email, model.Password);
    if (adminUser != null)
    {
        // Store in session
        HttpContext.Session.SetInt32("UserId", adminUser.Id);
        HttpContext.Session.SetString("UserName", adminUser.FullName);
        HttpContext.Session.SetString("Role", adminUser.RoleName);
        HttpContext.Session.SetString("UserType", "Admin");
        
        // Redirect based on role
        if (adminUser.RoleName == "Admin")
            return RedirectToAction("Index", "Admin");
    }

    // 2. Try Transporter authentication (Email-based)
    var transporter = await _transporterService.LoginTransporterAsync(model.Email, model.Password);
    if (transporter != null)
    {
        // Store in session
        HttpContext.Session.SetInt32("TransporterId", transporter.Id);
        HttpContext.Session.SetString("UserName", transporter.CompanyName);
        HttpContext.Session.SetString("Role", "Transporter");
        HttpContext.Session.SetString("UserType", "Transporter");
        
        return RedirectToAction("Index", "Transporter");
    }

    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
    return View(model);
}
```

### Login View Input

The `LoginViewModel` uses a single `Email` field for both FullName and Email:

```csharp
public class LoginViewModel
{
    [Required]
    [EmailAddress]  // This validation may need to be removed for FullName-based login
    public string Email { get; set; }

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    public bool RememberMe { get; set; }
}
```

**Note**: The `[EmailAddress]` validation attribute may need to be changed to `[Required]` only, since Admin users login with FullName (not email format).

## Session Storage

After successful authentication, user information is stored in session:

### For Admin Users:
- `UserId` (int) - User.Id
- `UserName` (string) - User.FullName
- `Role` (string) - User.Role.Name ("Admin", etc.)
- `UserType` (string) - "Admin"

### For Transporters:
- `TransporterId` (int) - Transporter.Id
- `UserName` (string) - Transporter.CompanyName
- `Role` (string) - "Transporter"
- `UserType` (string) - "Transporter"

### Reading Session in Controllers

```csharp
// Check if user is logged in
var userId = HttpContext.Session.GetInt32("UserId");
var transporterId = HttpContext.Session.GetInt32("TransporterId");
var userType = HttpContext.Session.GetString("UserType");
var role = HttpContext.Session.GetString("Role");

if (userId == null && transporterId == null)
{
    return RedirectToAction("Login", "Home");
}
```

## Test Credentials

### Admin User (User table)
- **FullName**: System Admin
- **Password**: admin123
- **Role**: Admin
- **Redirects to**: /Admin/Index

### Transporter User #1 (Transporter table)
- **Email**: info@translogistika.mk
- **Password**: trans123
- **Company**: TransLogistika DOOEL
- **Redirects to**: /Transporter/Index

### Transporter User #2 (Transporter table)
- **Email**: office@balkantransport.rs
- **Password**: trans123
- **Company**: Balkan Transport Group

### Transporter User #3 (Transporter table)
- **Email**: contact@eurocargo.hr
- **Password**: trans123
- **Company**: EuroCargo Solutions

## Logout

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public IActionResult Logout()
{
    HttpContext.Session.Clear();
    return RedirectToAction("Login", "Home");
}
```

## Authorization in Controllers

To protect actions, check session manually:

```csharp
public class AdminController : Controller
{
    public IActionResult Index()
    {
        var role = HttpContext.Session.GetString("Role");
        if (role != "Admin")
        {
            return RedirectToAction("Login", "Home");
        }
        
        // Admin-only logic
        return View();
    }
}
```

### Recommended: Create Authorization Attribute

```csharp
public class SessionAuthorizeAttribute : ActionFilterAttribute
{
    public string RequiredRole { get; set; }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var session = context.HttpContext.Session;
        var userId = session.GetInt32("UserId");
        var transporterId = session.GetInt32("TransporterId");

        if (userId == null && transporterId == null)
        {
            context.Result = new RedirectToActionResult("Login", "Home", null);
            return;
        }

        if (!string.IsNullOrEmpty(RequiredRole))
        {
            var role = session.GetString("Role");
            if (role != RequiredRole)
            {
                context.Result = new ForbidResult();
            }
        }
    }
}

// Usage:
[SessionAuthorize(RequiredRole = "Admin")]
public IActionResult AdminOnlyAction() { ... }
```

## Security Considerations

### Current Implementation (DEVELOPMENT ONLY)

⚠️ **WARNING**: The current implementation stores passwords in plain text and has no security measures. This is ONLY suitable for development/testing.

### Production Requirements

Before deploying to production, implement:

1. **Password Hashing**
   - Use `BCrypt.Net-Next` or `Microsoft.AspNetCore.Identity.PasswordHasher<T>`
   - Hash passwords in seed data
   - Hash passwords when creating users/transporters
   - Compare hashed passwords during authentication

2. **HTTPS Enforcement**
   - Already configured in Program.cs with `app.UseHttpsRedirection()`
   - Ensure valid SSL certificate in production

3. **Session Security**
   - Already configured with `HttpOnly = true` and `IsEssential = true`
   - Consider adding `Secure = true` for production
   - Set appropriate `IdleTimeout` (currently 30 minutes)

4. **CSRF Protection**
   - Already using `[ValidateAntiForgeryToken]` on POST actions
   - Ensure all forms include `@Html.AntiForgeryToken()`

5. **SQL Injection Protection**
   - Entity Framework Core provides parameterized queries
   - Avoid raw SQL queries or use `FromSqlRaw` with parameters

6. **Input Validation**
   - Already using DataAnnotations (`[Required]`, `[EmailAddress]`)
   - Consider adding more validation rules

7. **Authorization Attributes**
   - Create custom `[SessionAuthorize]` attribute as shown above
   - Apply to all controllers/actions that require authentication

## Migration from Identity to Custom Auth

### What Was Changed

1. **Removed from Program.cs:**
   - `using Microsoft.AspNetCore.Identity;`
   - `AddIdentity<IdentityUser, IdentityRole>()` configuration
   - `AddEntityFrameworkStores<AppDbContext>()`
   - `app.UseAuthentication()` and `app.UseAuthorization()` middleware

2. **HomeController:**
   - Replaced `SignInManager<IdentityUser>` with `IUserService`
   - Replaced `UserManager<IdentityUser>` with `ITansporterService`
   - Replaced Identity's `PasswordSignInAsync()` with custom authentication
   - Replaced Identity's cookie-based auth with session-based auth

3. **Database:**
   - Removed dependency on AspNetUsers, AspNetRoles, etc. tables
   - Uses custom `Users` and `Transporters` tables
   - Added `Password` field to Transporters seed data

4. **Session Configuration:**
   - Kept session middleware (required for custom auth)
   - Session timeout: 30 minutes
   - Cookies: HttpOnly, Essential

## Troubleshooting

### "Cannot create a DbSet for 'IdentityUser'" Error
- **Fixed**: Removed all Identity references from Program.cs and HomeController

### "Invalid login attempt" Error
- Check that credentials match seed data EXACTLY (case-sensitive)
- Admin login: Use FullName, not email
- Transporter login: Use Email address
- Verify database has been updated with latest migration

### Session Not Persisting
- Ensure `app.UseSession()` is called BEFORE routing
- Check browser allows cookies
- Verify session timeout hasn't expired (30 min default)

### Unable to Access Protected Pages
- Check session is being set correctly after login
- Verify session keys match ("UserId", "TransporterId", "Role", "UserType")
- Add authorization checks to controller actions
