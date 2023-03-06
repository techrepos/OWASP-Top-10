# Broken Access Control

## Improper Authentication

1. Excessive Authentication Attempts

- After multiple failed attempts the account is not being locked. This vulnerability will expose your application to brute-force attacks

- Open the sample solution and try to login with a invalid password as much as you want


    **Vulnerable Code**

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
                        _logger.LogInformation("User logged in.");
                        return LocalRedirect(returnUrl);
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
                }

                // If we got this far, something failed, redisplay form
                return Page();
            }
    ```

    - In the `PasswordSignInAsync` method the parameter `lockoutOnFailure` is set to `false` which in turn allows multiple sign in attempts

    **Remediation**

    **Lockout**
    - Turning the `lockoutOnFailure` parameter to `true` will trigger a lockout on the user
    account when several failed password logins have been reached.

    ```c#
    var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: true);

    ```

    - We can also configure additional settings to configure the lockout behavior. This can be done in the `Program.cs` file while configuring the ASP.NET Core Identity package

    ```c#
    builder.Services.AddIdentity<Customer, IdentityRole>(
                options => 
                    {
                        options.SignIn.RequireConfirmedAccount = false;
                        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                        options.Lockout.MaxFailedAccessAttempts = 3;
                    })
        .AddRoles<IdentityRole>()
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

    ```

    - **DefaultLockoutTimeSpan**  - sets the time for how long a user is locked out in
    minutes. The default value of this setting is 5 minutes
    - **MaxFailedAccessAttempts** - is set to lock the user out after the value set. Default is 5 attempts. 

    > The values of these properties should be assigned with values as per your organization's lockout policy

    **CAPTCHA**
    
    Another way to prevent brute-force attacks is to implement a **CAPTCHA**.  A
CAPTCHA is a challenge-response test that helps determine if a human or a computer executed the action.

    There are various CAPTCHA systems available for web developers to use, and Google's reCAPTCHA is the most popular among them.

    **How to setup reCAPTCHA service**
    1. Create a developer account in [Google Developer Website](https://developers.google.com/recaptcha/intro) and create an App to get an API key pair.
    2. When registering the website, make sure you are selecting the reCAPTCHA v2 **I'm not a robot checkbox**.  The demo code is based on this option
    
    **Setup **reCAPTCHA** in ASP.NET Core Web app
    1. Add the Google reCAPTCHA ASP.NET Core library
        `dotnet add package reCAPTCHA.AspNetCore`
    2. Create a config entry in `appsettings.json` to store the API key value pair
        ```json
        "RecaptchaSettings": {
            "SecretKey": "secret key",
            "SiteKey": "site key"
        }
        ```
        3. In `Program.cs`, import the namespace and configure the service
            ```c#
            using reCAPTCHA.AspNetCore;

            // Add recaptcha and pass recaptcha configuration section
            builder.Services.AddRecaptcha(builder.Configuration.GetSection("RecaptchaSettings"));

            ```
        4. Then to show the reCAPTCHA in the login page, navigate to `Areas\Identity\Pages\Account\Login.cshtml` file and add the following code

            ```html
            @using reCAPTCHA.AspNetCore
            @using reCAPTCHA.AspNetCore.Versions;

            @using Microsoft.Extensions.Options;

            @inject IOptions<RecaptchaSettings> RecaptchaSettings;
            ```

            ```html
            <div>
                @(Html.Recaptcha<RecaptchaV2Checkbox>(RecaptchaSettings?.Value))
            </div>

            ```

        5. In the action methed located in file `Areas\Identity\Pages\Account\Login.cshtml.cs` 

        ```c#
        using reCAPTCHA.AspNetCore;

        private readonly IRecaptchaService _recaptcha;

        public LoginModel(SignInManager<Customer> signInManager, ILogger<LoginModel> logger,
             IRecaptchaService recaptcha)
        {
            _signInManager = signInManager;
            _logger = logger;
            _recaptcha = recaptcha;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            var recaptcha = await _recaptcha.Validate(this.HttpContext.Request);
            if (!recaptcha.success)
                ModelState.AddModelError("Recaptcha", "Error Validating Captcha");

        ```

        The Validate method call from the reCAPTCHA service will take the current HTTP context and check whether the user's CAPTCHA response is valid.

        Supplementing the ASP.NET Core web application with a reCAPTCHA service will now prevent
        automated attacks and brute-forcing

## Testing User Enumeration

- Password breakers and crackers are now more powerful than ever with advanced hardware and endless computing resources. Simply hashing the passwords is no longer enough, and it is now crucial to pick the right hashing function to protect the credentials from being exposed when a data breach happens.

- An adversary can gather information by analyzing an application's behavior, especially on the messages that your ASP.NET Core web application displays to its users. The message "Customer does not exist" indicates that an email address (used as a username, in this case) does not exist in the database. This malicious actor can then come up with a list of valid usernames and email addresses that they can use for other nefarious activities

**Vulnerable Code**

In file `\Areas\Identity\Pages\Account\Login.cshtml.cs`, the code on the else part have it

```c#
if (result.IsLockedOut)
{
    _logger.LogWarning("Customer account locked out.");
    return RedirectToPage("./Lockout");
}
else
{
    var user = await _userManager.FindByEmailAsync(Input.Email);
    if (user == null)
    {
        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        return Page();
    }
}

```

**Remediation**

- Modify the code to as shown below so that it returns a generic message

```c#
if (result.IsLockedOut)
{
    _logger.LogWarning("Customer account locked out.");
    return RedirectToPage("./Lockout");
}
else
{
    var user = await _userManager.FindByEmailAsync(Input.Email);
    if (user == null)
    {
        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        return Page();
    }
}
```

## Weak Password Policy

Not requiring lowercase, alphanumeric, and uppercase characters is no longer an acceptable password policy. A firm password policy is needed to stop successful credential-based brute-force attacks,

**Vulnerable Code**
ASP.NET Core's `IdentityOptions` is configured to have a weak password policy, thus overriding the default safe values for the `RequireLowercase`, `RequireNonAlphanumeric`, and `RequireUppercase` properties

```c#
builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 5;
    options.Password.RequiredUniqueChars = 0;
});

```

**Remediation**

```c#
builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 10;
    options.Password.RequiredUniqueChars = 1;
});
```