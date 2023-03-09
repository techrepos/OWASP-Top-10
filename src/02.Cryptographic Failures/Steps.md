# Cryptographic Failures

## Weakened Ciphers

Password breakers and crackers are now more powerful than ever with advanced hardware and endless computing resources. Simply hashing the passwords is no longer enough, and it is now crucial to pick the right hashing function to protect the credentials from being exposed when a data breach happens

**Vulnerable Code**

In our sample application, we have defined our own hashing mechanism in the `PasswordHasher` class. It is derived from the `IPasswordHasher` interface and lets you define your own hashing mechanism. The algorithm used here is `MD5` which is known to have a weak hashing algorithm

```c#
 public string HashPassword(Customer customer, string password)
{ 
    using (var md5 = new MD5CryptoServiceProvider()) {

        var hashedBytes = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
        
        var hashedPassword = BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
        return hashedPassword;  
    }

}

```


**Resolution**

The `MD5` hashing algorithm is known to be a weak cipher. We can replace this vulnerable hash function by implementing the bcrypt algorithm.

`Bcrypt.Net` is a .NET library implementation of the Bcrypt hashing function based on the `Blowfish` cipher. The `BCrypt` hashing function implements a strong security measure as it adds `salt` to the hashing process. Bcrypt.Net lets developers define their own salt in the hash, but it is advisable to just let the library generate its own salt

Add the below package

```c#
dotnet add package BCrypt.Net-Next
```

Modify the `HashPassword` and `VerifyHashedPassword` methods in `PasswordHasher` class as shown below

```c#
using BC = BCrypt.Net.BCrypt;


 public string HashPassword(Customer customer, string password)
{
    return BC.HashPassword(password);

}

public PasswordVerificationResult VerifyHashedPassword(Customer customer,
    string hashedPassword, string password)
{
    if (BC.Verify(password, hashedPassword))
        return PasswordVerificationResult.Success;
    else
        return PasswordVerificationResult.Failed;
}
```

We installed the `BCrypt.NET` library implementation via the BCrypt.Net-Next NuGet package, adding a reference to the BCrypt.Net.BCrypt namespace and rewriting the entire
HashPassword function to make a call to BCrypt.Net's HashPassword method.

Bcrypt is a good option since its process is time-consuming, making it hard to break; however, it can be prone to GPU-based cracking

## Insufficient protection of data in transit

TLS is a network communication protocol that's used on the web to secure data and achieve privacy through cryptography. Missing or flawed implementations of this secure protocol brings an ASP.NET web application to a massive amount of risk when sensitive data being transmitted between the browser and the web server is unencrypted or potentially intercepted. Enabling TLS is the first step to adequately encrypting data in transit.

Not enabling TLS in your ASP.NET Core web application puts your confidential data in transit between the clients and servers at risk. You must ensure that HTTPS has been configured for the best protection

Setup `AddHttpsRedirection` middleware in `Program.cs` file. Here we determine if we must perform temporary redirection or permanent redirection using the standard HTTPS port 443 based on the environment

```c#
if(builder.Environment.IsDevelopment())
{
    builder.Services.AddHttpsRedirection(options =>
    {
        options.RedirectStatusCode
        = StatusCodes.Status307TemporaryRedirect;
        options.HttpsPort = 5001;
    });
}
else
{
    builder.Services.AddHttpsRedirection(options =>
    {
        options.RedirectStatusCode
        = StatusCodes.Status308PermanentRedirect;
        options.HttpsPort = 443;
    });
}

```


Add a call to the UseHttpsRedirection method just before the call to UseStaticFiles 

```c#

app.UseHttpsRedirection();

```


Open the `launchsettings.json` file and change the values in `applicationUrl` and `environmentVariables` . 
The launchsettings.json file is a configuration file that allows you to work on multiple environments during development. This file is not included when deploying to production. If
you have a setting for IISExpress, you may also want to change the ASPNETCORE_URLS environment variable and applicationURL

```json
"http": {
    "commandName": "Project",
    "launchBrowser": true,
    "environmentVariables": {
    "ASPNETCORE_ENVIRONMENT": "Development"
    },
    "dotnetRunMessages": true,
    "applicationUrl": "http://localhost:5000"
},
"https": {
    "commandName": "Project",
    "launchBrowser": true,
    "environmentVariables": {
    "ASPNETCORE_ENVIRONMENT": "Development"
    },
    "dotnetRunMessages": true,
    "applicationUrl": "https://localhost:5001;http://localhost:5000"
},

```


In the Startup class, we call the AddHttpsRedirection method to register UseHttpsRedirection to the service collection. We configure the middleware options by setting two properties:
RedirectStatusCode and HttpsPort. 

By default, RedirectStatusCode isStatus307TemporaryRedirect, but it should be changed to Status308PermanentRedirect in production environments to prevent user-agents (also known as browsers) from changing the HTTP methods from POST to GET

> The call to AddHttpsRedirection is optional unless you need a different redirect status code or a different port other than 443. 
> To redirect HTTP requests to HTTPS, we must add the HTTPS middleware in Configure with a call to the UseHttpsRedirection method

## Missing HSTS Headers

HTTP Strict Transport Security or HSTS is another web application security mechanism that helps prevent man-in-the-middle attacks. It allows web servers to send a special HTTP Response header that informs supporting browsers that the subsequent communication and transmission of data should only be done over HTTPS; otherwise, succeeding connections will  not be allowed. Failing to opt-in HSTS as an additional security policy does not eliminate the threat of sensitive data interception. Supplementing HTTPS with HSTS will thwart the risk of a user being exposed to an unencrypted channel

Setup `AddHsts` middlerware before the the call to `AddHttpsRedirection` in your `Program.cs` file

```c#

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
```
- **Preload**: Sends the preload flag as part of the Strict-Transport-Header. It directs the supporting browser to include your
ASP.NET Core web application's domain in the preloaded list, thus preventing users access over HTTP.

- **IncludeSubdomains**: Another directive that lets the subdomains have HSTS enabled, not just the top-level domain name.

- **MaxAge**: A property that determines the max-age directive of the Strict-Transport-Security header. This dictates how long the browser will remember to send requests for over HTTPS in seconds.

    > The call to the `Clear` method of `ExcludedHostsProperty` is used for the purposes of testing `HSTS` locally and in development. This method stops `localhost` from being excluded since it's a loopback host. This code must be removed in the production environment.

    > You can also add exclude options for localhost and/or development environment hosts as an alternative: `options.ExcludedHosts.Add("localhost");`

Then add a call to `UseHsts` method

```c#
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

```

## Weak Protocols

The cryptographic protocol known as TLS has evolved over the years and initially started as Secure Sockets Layer, most commonly known as SSL. This is now deprecated, and so its successors have been discovered to have  vulnerabilities in their design. The latest version of the Transport Layer Security protocol, TLS 1.3, was created to solve these problems.

Enabling HTTPS and using TLS is not enough to protect your ASP.NET Core web applications from accidental data exposure. An adversary can potentially exploit a weak version of TLS. To overcome this, you must employ the latest and greatest versions of cryptographic ciphers and protocols

**Vulnerable Code**

The `SslProtocols` property is being assigned with a lower version of `TLS`. The current value indicates that the TLS version being used is version `TLS 1.0`

```c#
builder.WebHost.ConfigureKestrel(opt =>
{
    opt.AddServerHeader = false;

    opt.ConfigureHttpsDefaults(https =>
    {
        https.SslProtocols
        = SslProtocols.Tls;
    });


});
```

The current version of the code is using a lower and vulnerable version of TLS. `SslProtocols` is assigned an enum value of `SslProtocols.Tls`, which is equivalent to TLS 1.0. TLS 1.0 is deprecated as of March 2020.


**Resolution**

```c#
builder.WebHost.ConfigureKestrel(opt =>
{
    opt.AddServerHeader = false;

    opt.ConfigureHttpsDefaults(https =>
    {
        https.SslProtocols
        = SslProtocols.Tls12 | SslProtocols.Tls13;

    });


});
```

The default web server being used in this sample ASP.NET Core web application is Kestrel.

Within the code, we configured the settings of Kestrel with UseKestrel, passing the property values as options. One of these properties is SslProtocols, which we should assign with the `SslProtocols.Tls12` and `SslProtocols.Tls13` piped enum values to specify that only `TLS 1.2` and `TLS 1.3` are allowed.


## HardCoded Cryptographic Keys

`Cryptographic keys` are an essential part of the whole ecosystem of cryptography. This string is
vital for encrypting and decrypting sensitive data. In particular, asymmetric cryptographic
algorithms have private keys (as part of the public-private key pair exchange) that are meant to
be kept secure from prying eyes. If these secret keys fall into the wrong hands or get leaked, an
attacker will be able to successfully perform sensitive data decryption


**Vulnerable Code**

```c#

private readonly ICryptoService _cryptoService;
private const string key = "TZw3HUkI5y";

var user = new Customer {
    FirstName = _cryptoService.Encrypt (Input.FirstName,key),
    MiddleName = _cryptoService.Encrypt (Input.MiddleName,key),
    LastName = _cryptoService.Encrypt (Input.LastName, key),
    DateOfBirth = Input.DateOfBirth,
    UserName = Input.Email,
    Email = Input.Email
}



```

Notice that the call to the `_cryptoService.Encrypt` method has a second argument that accepts a string. This parameter expects to receive a key, but the key that is being passed is hardcoded in code. The key class field is declared and assigned with a hardcoded value of `TZw3HUkI5y`

**Resolution**
Create an environment variable named `securekey`

From Powershell

```bash
 $Env:securekey="TZw3HUkI5y"
```



```c#
var user = new Customer
{
    FirstName = _cryptoService.Encrypt(Input.FirstName, Environment.GetEnvironmentVariable("securekey", EnvironmentVariableTarget.Machine)),
    MiddleName = _cryptoService.Encrypt(Input.MiddleName, Environment.GetEnvironmentVariable("securekey", EnvironmentVariableTarget.Machine)),
    LastName = _cryptoService.Encrypt(Input.LastName, Environment.GetEnvironmentVariable("securekey", EnvironmentVariableTarget.Machine)),
    DateOfBirth = Input.DateOfBirth,
    UserName = Input.Email,
    Email = Input.Email
};
```


In the event that an attacker can get hold of the code repository, there is a risk that the bad actor will be able to see the private key in the code. We must protect these secret keys and they must be stored externally from the code repository, thus limiting who can see them to anyone who has access to the system

This is a risk for accidental exposure through unauthorized access, but this can be remediated by
storing the key in an environment variable. Here, we created a system variable, making it accessible machine-wide, and retrieved the key by invoking the `Environment.GetEnvironmentVariable method`:

The first parameter is the name of the environment variable, while the second parameter specifies
the location where the key is stored. The possible enum values are
`EnvironmentVariableTarget.Machine`, 
`EnvironmentVariableTarget.Process`, and
`EnvironmentVariableTarget.User`.
We used `EnvironmentVariableTarget.Machine` to indicate that our variable is stored in the
Windows registry. Its exact location is the Windows registry is
`HKEY_LOCAL_MACHINE\System\CurrentControlSet\Control\SessionManager\Environment`

