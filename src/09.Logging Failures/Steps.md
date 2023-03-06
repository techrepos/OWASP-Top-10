# Insufficient Logging and Monitoring

## 

**Vulnerable Code**

```c#
if (result.Succeeded)
{
   

    if (string.IsNullOrEmpty(HttpContext.Session.GetString(SessionKeyName)))
    {
        HttpContext.Session.SetString(SessionKeyName, Input.Email);
    }

    return Redirect(returnUrl);
}
if (result.RequiresTwoFactor)
{
    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
}
if (result.IsLockedOut)
{
    _logger.LogWarning("User account locked out.");
    return RedirectToPage("./Lockout");
}
else
{
    
    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
    return Page();
}
```

**Resolution**

```c#
builder.Logging.AddEventLog(opt =>{
    opt.SourceName = "ABCBankingWebApp";

});

```


```c#
if (result.Succeeded)
{
    _logger.LogInformation("User logged in.");

    if (string.IsNullOrEmpty(HttpContext.Session.GetString(SessionKeyName)))
    {
        HttpContext.Session.SetString(SessionKeyName, Input.Email);
    }

    return Redirect(returnUrl);
}
if (result.RequiresTwoFactor)
{
    _logger.LogWarning("User requires two factor auth, redirecting to MFA page");
    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
}
if (result.IsLockedOut)
{
    _logger.LogWarning("User account locked out.");
    return RedirectToPage("./Lockout");
}
else
{
    _logger.LogError("Invalid login attempt.");
    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
    return Page();
}
```

# Insufficient DB Logging
Basic DB transactions such as creating, reading, and deleting records are essential to have audit trails, especially when an error occurs as a DB function is performed.

**Vulnerable Code**

In our example, while editing the backup , DB operations such as performing a backup and updating its related records are not logged, we are only re-throwing the exception back or returning a  not found error

```c#
public async Task<IActionResult> Edit(int id, [Bind("ID,Name,BackupDate")] Backup backup)
{
    if (id != backup.ID)
    {
        return NotFound();
    }

    if (ModelState.IsValid)
    {
        try
        {
            _context.Update(backup);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!BackupExists(backup.ID))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }
        return RedirectToAction(nameof(Index));
    }
    return View(backup);
}

```


**Resolution**
To fix the missing logging of high-value DB transactions, let's add a logger using the `ILogger` interface through DI. Begin by defining a `_logger` member of type `ILogger` and inject the `ILogger` member into the controller constructor. Then modify the Edit method to add log statements to start writing to the logging providers configured in the startup

```C#
private readonly ILogger<BackupFileController> _logger;

//constructor
public BackupFileController(AppDbContext context, ILogger<BackupFileController> logger)
{
    _context = context;
    _logger = logger;
}

public async Task<IActionResult> Edit(int id, [Bind("ID,Name,BackupDate")] Backup backup)
{
    if (id != backup.ID)
    {
        return NotFound();
    }

    if (ModelState.IsValid)
    {
        try
        {
            _context.Update(backup);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            if (!BackupExists(backup.ID))
            {
                _logger.LogError("Backup not found");

                return NotFound();
            }
            else
            {
                _logger.LogError($"Exception Occurred : {ex.Message} {Environment.NewLine}{ex.ToString()} ");

                throw;
            }
        }
        return RedirectToAction(nameof(Index));
    }
    return View(backup);
}

````

During a sensitive DB operation such as a DB backup, an unexpected error can occur. Such exceptions may cause a faulty system or—worse—an attack in our applications. We mitigate the risk of losing data integrity by keeping track of these DB operations and knowing when an event happened


# Excessive information logging

Ensuring you prevent the exposure of personal details is the key to keeping your application secure, and the same goes for logging information. While logs are helpful, there is also a risk involved in logging excessive data.
Perpetrators will find ways to get useful information, and the log store is one source they will try to discover

**Vulnerable Code**
In the login method, too much sensitive information is written into the logs:

```c#
public async Task<IActionResult> OnPostAsync(string returnUrl = null)
{
    returnUrl ??= Url.Content("~/");

    ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

    if (ModelState.IsValid)
    {
        // This doesn't count login failures towards account lockout
        // To enable password failures to trigger account lockout, set lockoutOnFailure: true
        var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);
        if (result.Succeeded)
        {
            _logger.LogInformation($"Customer with email {Input.Email} and password {Input.Password} logged in");


            if (string.IsNullOrEmpty(HttpContext.Session.GetString(SessionKeyName)))
            {
                HttpContext.Session.SetString(SessionKeyName, Input.Email);
            }

            return Redirect(returnUrl);
        }
        if (result.RequiresTwoFactor)
        {
            _logger.LogWarning("User requires two factor auth, redirecting to MFA page");
            return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
        }
        if (result.IsLockedOut)
        {
            _logger.LogWarning("User account locked out.");
            return RedirectToPage("./Lockout");
        }
        else
        {
            _logger.LogInformation($"Customer tried with email {Input.Email} and password {Input.Password}");

            _logger.LogError("Invalid login attempt.");
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return Page();
        }
    }

    // If we got this far, something failed, redisplay form
    return Page();
}
```

**Resolution**

`Writing to File using Serilog`

Add the below NuGet package

`dotnet add package Serilog.AspNetCore`

Replace the logging entry in the `appsettings.json` file with the below one

```json
"Serilog": {
    "MinimumLevel": "Information",
    "Override": {
      "Microsoft.AspNetCore": "Error"
    },
    "WriteTo": [
      {
        "Name": "Console"
      },
      {
        "Name": "File",
        "Args": {
          "path": "Serilogs\\AppLogs.log"
        }
      }
    ]
  }
```

Modify the Program.cs file as below

```c#
Log.Logger = new LoggerConfiguration()
                 .ReadFrom.Configuration(builder.Configuration)
                 .Enrich.FromLogContext()
                 .CreateLogger();
builder.Host.UseSerilog(Log.Logger);

```


Replace the logging entries as shown below to remove sensitive data from the logs

```c#
public async Task<IActionResult> OnPostAsync(string returnUrl = null)
{
    returnUrl ??= Url.Content("~/");

    ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

    if (ModelState.IsValid)
    {
        // This doesn't count login failures towards account lockout
        // To enable password failures to trigger account lockout, set lockoutOnFailure: true
        var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);
        if (result.Succeeded)
        {
            _logger.LogInformation($"Customer logged in");


            if (string.IsNullOrEmpty(HttpContext.Session.GetString(SessionKeyName)))
            {
                HttpContext.Session.SetString(SessionKeyName, Input.Email);
            }

            return Redirect(returnUrl);
        }
        if (result.RequiresTwoFactor)
        {
            _logger.LogWarning("User requires two factor auth, redirecting to MFA page");
            return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
        }
        if (result.IsLockedOut)
        {
            _logger.LogWarning("User account locked out.");
            return RedirectToPage("./Lockout");
        }
        else
        {
            

            _logger.LogError("Invalid login attempt.");
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return Page();
        }
    }

    // If we got this far, something failed, redisplay form
    return Page();
}
}

```

# Adding App Insights

Add the nuget package

`install-package Microsoft.ApplicationInsights.AspNetCore`

SEtup middleware in Program.cs

`builder.Services.AddApplicationInsightsTelemetry();`


Setup connectionstring
```json
"ApplicationInsights": {
    "ConnectionString": "Copy connection string from Application Insights Resource Overview"
  }
```