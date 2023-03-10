## Safe Storage of Secrets

Normally we store connections strings to databases and all in the configuration files. This can be vulnerable if a malicious attacker gets through to our file system and access our files to grab this info

Production secrets shouldn't be used for development or test. Secrets shouldn't be deployed with the app. Instead, production secrets should be accessed through a controlled means like environment variables or Azure Key Vault.


### Environment Variables

Environment variables are used to avoid storage of app secrets in code or in local configuration files. Environment variables override configuration values for all previously specified configuration sources.

> Environment variables are generally stored in plain, unencrypted text. If the machine or process is compromised, environment variables can be accessed by untrusted parties. Additional measures to prevent disclosure of user secrets may be required.

### Secret Manager

The Secret Manager tool stores sensitive data during the development of an ASP.NET Core project. In this context, a piece of sensitive data is an app secret. App secrets are stored in a separate location from the project tree. The app secrets are associated with a specific project or shared across several projects. The app secrets aren't checked into source control

> The Secret Manager tool doesn't encrypt the stored secrets and shouldn't be treated as a trusted store. It's for development purposes only. The keys and values are stored in a JSON configuration file in the user profile directory.

The Secret Manager tool operates on project-specific configuration settings stored in your user profile.

`dotnet user-secrets init`

This command adds a `UserSecretsId` element within a `PropertyGroup` of the project file. By default, the inner text of `UserSecretsId` is a GUID. The inner text is arbitrary, but is unique to the project.


```xml
<PropertyGroup>
    <TargetFramework>net7.0</TargetFramework>
    <Nullable>disable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <EnableNETAnalyzers>true</EnableNETAnalyzers>
    <AnalysisLevel>latest</AnalysisLevel>
    <EnableUnsafeBinaryFormatterSerialization>true</EnableUnsafeBinaryFormatterSerialization>
    <UserSecretsId>cff64aea-d2f8-4336-909a-673e2dfa2e48</UserSecretsId>
</PropertyGroup>

```

**Set a Secret**

Use the below command to set a secret in the store

```bash
 dotnet user-secrets set "ConnectionStrings:ABCBankDB" "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=ABCBanking;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False"
```

**Access a secret**

Register the user secrets configuration source. The user secrets configuration provider registers the appropriate configuration source with the .NET Configuration API.

`WebApplication.CreateBuilder` initializes a new instance of the `WebApplicationBuilder` class with preconfigured defaults. The initialized `WebApplicationBuilder (builder)` provides default configuration and calls `AddUserSecrets` when the EnvironmentName is `Development`:

```c#
var builder = WebApplication.CreateBuilder(args);
```


## KeyVault


`Azure KeyVault` is a cloud service provided by Microsoft for securely storing and accessing not only secrets but also certificates, keys, passwords, etc.  Apart from storing it securely, KeyVault provides additional features such as access control, audit logging, versioning, validity, and much more. With the help of these features, we can make sure that only authorized personnel/app has access to the data with proper auditing and expiration controls.

**Creating KeyVault**

Just type in Key Vault in the search bar at the top and select Key Vaults from the results. From the next page, select the Create option and you will get a window like the one below. There, just select the Resource Group, specify a name for the key vault, region, and pricing tier and leave the rest with the default values

![Alt text](../../../../E:/Training/OWASP%20Top%2010/Misc/images/create-keyvault-1.png)

![Alt text](../../../../E:/Training/OWASP%20Top%2010/Misc/images/create-keyvault-2.png)

![Alt text](../../../../E:/Training/OWASP%20Top%2010/Misc/images/create-keyvault-3.png)

**Creating a secret**

Before you can add a secret in the key vault, you will need to give yourself access to either add or manage it. To do that, you can go to Access Policies from the left menu under your key vault and then select Add Access Policy. Since we are dealing only with secrets, we will only select the necessary permissions needed for the same and then the identity to give access to

![Alt text](../../../../E:/Training/OWASP%20Top%2010/Misc/images/add-access-policy-1.png)

![Alt text](../../../../E:/Training/OWASP%20Top%2010/Misc/images/add-kv-secret-1.png)

**CLI Command**

```bash
az keyvault secret set --vault-name "<keyvault name" --name "<key name>" --value "<your connection string>"

```

For example in our case , for database connection string

```bash
az keyvault secret set --vault-name "gab22demo-rg" --name "ConnectionStrings--ABCBankDB" --value "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=ABCBanking;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False" Failover=False"
```

**Connecting to Key Vault from your project**

Add the below entry in the `appsettings.json`

```json
"AzureKeyVault": {
    "keyvault-url": "https://gab22demo-rg.vault.azure.net/"
  },
```
Add the below packages

```bash
Azure.Identity
Azure.Extensions.AspNetCore.Configuration.Secrets
```

And modify your `Program.cs` like below

```c#
var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddAzureKeyVault(
    new Uri(builder.Configuration["AzureKeyVault:keyvault-url"]), new DefaultAzureCredential());

```


## IP Safelist

Add an entry in `appsettings.json`

```json
"AdminSafeList": "127.0.0.1;192.168.1.5;::1"
```

Create a custom middleware to intercept the request pipeline and validate the incoming ip address. Based on the entries in the safelist, it will either allow or block the incoming request

```c#
public class AdminSafeListMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AdminSafeListMiddleware> _logger;
    private readonly byte[][] _safelist;

    public AdminSafeListMiddleware(
        RequestDelegate next,
        ILogger<AdminSafeListMiddleware> logger,
        string safelist)
    {
        var ips = safelist.Split(';');
        _safelist = new byte[ips.Length][];
        for (var i = 0; i < ips.Length; i++)
        {
            _safelist[i] = IPAddress.Parse(ips[i]).GetAddressBytes();
        }

        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        if (context.Request.Method != HttpMethod.Get.Method)
        {
            var remoteIp = context.Connection.RemoteIpAddress;
            _logger.LogDebug("Request from Remote IP address: {RemoteIp}", remoteIp);

            var bytes = remoteIp.GetAddressBytes();
            var badIp = true;
            foreach (var address in _safelist)
            {
                if (address.SequenceEqual(bytes))
                {
                    badIp = false;
                    break;
                }
            }

            if (badIp)
            {
                _logger.LogWarning(
                    "Forbidden Request from Remote IP address: {RemoteIp}", remoteIp);
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return;
            }
        }

        await _next.Invoke(context);
    }
}
```

Then hook up the middleware during the bootstrapping process in `Program.cs` file

```c#
app.UseMiddleware<AdminSafeListMiddleware>(Configuration["AdminSafeList"]);
```

The middleware parses the string into an array and searches for the remote IP address in the array. If the remote IP address isn't found, the middleware returns HTTP 403 Forbidden. This validation process is bypassed for HTTP GET requests.


## Rate Limiter


The `Microsoft.AspNetCore.RateLimiting` middleware provides rate limiting middleware. Apps configure rate limiting policies and then attach the policies to endpoints.

Supported rate limiters


- Fixed window
- Sliding window
- Token bucket
- Concurrency

**Fixed window limiter**

Add the configuration options in the `appsettings.json` file

```json
  "RateLimitSetting": {
    "PermitLimit": 4,
    "Window": 12,
    "ReplenishmentPeriod": 1,
    "QueueLimit": 2,
    "SegmentsPerWindow": 4,
    "TokenLimit": 8,
    "TokenLimit2": 12,
    "TokensPerPeriod": 4,
    "AutoReplenishment": true
  }

```
Add a new class to bind the settings mentioned above

```c#
public class RateLimitOptions
{
    
    public const string RateLimitSetting = "RateLimitSetting";
    public int PermitLimit { get; set; } = 10;
    public int SlidingPermitLimit { get; set; } = 100;
    public int Window { get; set; } = 10;
    public int ReplenishmentPeriod { get; set; } = 2;
    public int QueueLimit { get; set; } = 100;
    public int SegmentsPerWindow { get; set; } = 8;
    public int TokenLimit { get; set; } = 10;
    public int TokenLimit2 { get; set; } = 20;
    public int TokensPerPeriod { get; set; } = 4;
    public bool AutoReplenishment { get; set; } = false;
}

```

Then populate the configuration collection in `Program.cs` file

```c#
builder.Services.Configure<RateLimitOptions>(
    builder.Configuration.GetSection(RateLimitOptions.RateLimitSetting));

var optRateLimit = new RateLimitSetting();
builder.Configuration.GetSection(RateLimitOptions.RateLimitSetting).Bind(optRateLimit);


```

Add and configure the rate limiter middleware in `Program.cs` file

```c#

var fixedPolicy = "fixed";

builder.Services.AddRateLimiter(_ => _
    .AddFixedWindowLimiter(policyName: fixedPolicy, options =>
    {
        options.PermitLimit = optRateLimit.PermitLimit;
        options.Window = TimeSpan.FromSeconds(optRateLimit.Window);
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = optRateLimit.QueueLimit;
    }));
```

And finally, invoke the RateLimiter middleware by adding the below code

```c#
app.UseRateLimiter();


```
The above code

- Calls `AddRateLimiter` to add a rate limiting service to the service collection.
- Calls `AddFixedWindowLimiter` to create a fixed window limiter with a policy name of "`fixed`" and sets:
- `PermitLimit` to 4 and the time `Window` to 12. A maximum of 4 requests per each 12-second window are allowed.
- `QueueProcessingOrder` to `OldestFirst`.
- `QueueLimit` to 2.
- Calls `UseRateLimiter` to enable rate limiting.


## Secure Cookie Policy

Cookies are an essential part of web application development. It is a means to maintain a state in a stateless HTTP protocol and carry the most vital information that's used in security-like tokens and session data

SameSite is a relatively new cookie attribute (at the time of writing) and is utilized to limit third-party websites from accessing a cookie marked with the context of a first party.


```c#
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.CookiePolicy;

//add the snippet in Program.cs

services.Configure<CookiePolicyOptions>(options =>
{
    options.MinimumSameSitePolicy = SameSiteMode.Strict;
    options.Secure = Environment.IsDevelopment() ? CookieSecurePolicy.None : CookieSecurePolicy.Always;
    options.HttpOnly = HttpOnlyPolicy.Always;
});


```

The `MinimumSameSitePolicy` property of the cookie policy's options is assigned with the `SameSiteMode.Strict` value, marking the `SameSite` cookie attribute with a `Strict` value

If session state cookies from the session middleware are a concern, you can limit the session state cookies to the first-party context by assigning the same SameSite property with `SameSiteMode.Strict`
```c#
services.AddSession(options =>
{
    options.Cookie.Name = ".OnlineBanking.Session";
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy =  CookieSecurePolicy.Always;
    options.IdleTimeout = TimeSpan.FromSeconds(10);
});

```
Execute the cookie policy middleware by making a call to the UseCookiePolicy method

```c#
app.UseRouting();
app.UseCookiePolicy();
app.UseAuthentication();
```

Setting the `SameSite` cookie attribute to `Strict` limits the context as to when the cookies are sent as part of a request. As the name implies, cookies are only strictly sent within `same-site` and `first-party` requests, thus preventing third-party websites or web applications from sending the cookie when initiating the requests. This can be configured with the Session and Antiforgery service options

> Using the SameSite attribute requires the Secure attribute. Modern browsers will automatically reject the cookie if the SameSite
attribute is not accompanied by a Secure attribute.
> Modern browsers, by default, will set your cookie's SameSite attribute to Lax if you don't explicitly specify a value. Lax allows
the cookie to be sent in requests when a user follows a link


Limiting your cookies to first-party requests also prevents your ASP.NET Core web application from cross-site attacks and vulnerabilities such as Cross-Site Request Forgery (CSRF)


 > If your ASP.NET Core web application integrates with third-party websites extensively, and these sites need to use your cookies, users might experience some malfunctioning because of the restrictive nature of the `Strict` SameSite attribute. Depending on the risk level of your web app, you may want to lower the restriction to `SameSiteMode.Lax`


 ## Content Security Policy

 Browsers have built-in security mechanisms to protect its users from attacks, making the overall user experience safe from web-based vulnerabilities. Additionally, how we write our code in our web apps is crucial to instructing the browser on how to enable these security features

 We can tell the browser which hosts are safe to pull resources from and where it is safe to execute the scripts. These whitelisting rules can be defined using a `CSP`.

 ```c#
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Add("X-Content-Type- Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    
    //CSP Header
    string scriptSrc = "script-src 'self' https://cdnjs.cloudflare.com;";
    string styleSrc = "style-src 'self' 'unsafe-inline';";
    string imgSrc = "img-src 'self'; ";
    string objSrc = "object-src 'none';";
    string defaultSrc = "default-src 'self';";
    string connectSrc = "connect-src https: wss: http://localhost:58082;";
    string csp = $"{defaultSrc}{scriptSrc}{styleSrc} {imgSrc} {objSrc}";
    //context.Response.Headers.Add($"Content-Security-Policy-Report-Only", csp);
    context.Response.Headers.Add($"Content-Security-Policy", csp);

    await next();
});

 ```

 Here, we have defined string variables to hold each of the most common CSP headers and their source locations. Then, we added them to the HTTP response headers collection as a `Content-Security-Policy` HTTP header

 Each of the string variables corresponds to a CSP header

 `scriptSrc` defines a list of trusted source locations to load JavaScript from .The `self` value pertains
to our web app itself and the host (https://cdnjs.cloudflare.com) where we   downloaded the underscore.js library:

`styleSrc` defines a list of trusted source locations for loading stylesheets. We are using self here to indicate that it is acceptable to load stylesheets local to our web app, but we are also using unsafe-inline to allow the use of inline `<style>` elements. It is generally not safe to use `unsafe-inline`, but since there is no user-controlled input that can influence the stylesheet of our  web app

`img-src`  defines the acceptable values for trusted source for our images

To have a safe CSP, `object-src` needs to be set to none. This CSP header will list the acceptable
sources for plugins and was kept for legacy applets, which is an obsolete technology

 `default-src `CSP header is the fallback for all other CSP headers that weren't defined.

 There are tools online that you can use to generate your own CSP
 -  https://report-uri.com/home/generate
 -  Google's CSP validator, available at https://csp-evaluator.withgoogle.com/


 ## Fixing leftover debug code

The .NET platform offers a wide array of libraries and components for debugging, including diagnostics and tracing effectively. However, it is a bad practice for developers to leave debugging code as-is and neglect to remove it from the repository or add conditional checks, thus leading to unnecessary code being deployed in production


The `IsDevelopment` method checks whether the code is running under the context of a development environment. This method is a useful test before we execute debugging methods
such as `UseStatusCodePages` and `AddDatabaseDeveloperPageExceptionFilter`. `AddDatabaseDeveloperPageExceptionFilter` could potentially expose sensitive information
when invoked by a database error. 

Performing secure code reviews regularly and executing static application security testing as part of the development cycle can help detect these problems early


`UseStatusCodePages` is a piece of middleware that provides status code as a response. This is useful for determining what status code was returned by our sapp, but these status codes are not useful to our users

`AddDatabaseDeveloperPageExceptionFilter` is an exception filter that provides exception details related to database operations. This method is useful for debugging and finding
resolutions to DB-related issues. However, it must not be called in a production environment.

```c#
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddHttpsRedirection(options =>
    {
        options.RedirectStatusCode
        = StatusCodes.Status307TemporaryRedirect;
        options.HttpsPort = 5001;
    });
    builder.Services.AddDatabaseDeveloperPageExceptionFilter();
}
else
{
    builder.Services.AddHsts(options =>
    {
        options.ExcludedHosts.Clear();
        options.Preload = true;
        options.IncludeSubDomains = true;
        options.MaxAge = TimeSpan.FromDays(60);
    });

    builder.Services.AddHttpsRedirection(options =>
    {
        options.RedirectStatusCode
        = StatusCodes.Status308PermanentRedirect;
        options.HttpsPort = 443;
    });

    
}

//code omitted for brevity

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseStatusCodePages("text/plain", "Status code page, status code: {0}");

}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();

}

```