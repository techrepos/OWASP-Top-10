# Security Misconfiguration

## Disabling Debug features

ASP.NET Core enables developers to have easy access to a configuration that will quickly enable or disable debugging in a particular environment with configuration files or code.

Negligence or configuration mismanagement could cause debugging to be enabled in a non development environment such as staging or production

### Vulnerable code

In the `Program.cs`, developer exception page is turned on by default and will be shown to the users be it in development or production environment

```c#
//some code omitted for brevity

var app = builder.Build();

app.UseDeveloperExceptionPage();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();


//some code omitted for brevity

```

Resolution

Modify the code as below so that a custom error page will be shown instead of the detail error page. By doing the amount of debug information shown to the user can be controlled

```c#
if (!app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    
}
else
{
    app.UseExceptionHandler("/Home/Error");
}
```

Making a call to the UseExceptionHandler middleware will catch the exceptions and handle the exceptions to return a friendlier Error view defined in the home controller

# Disabled security features

Web application servers have built-in security features such as security headers configured to be sent as a part of the HTTP response back to the client, instructing browsers to
enable the security mechanism. Not all of these security headers are turned on or added by default, so enabling it in code is left in the web developers' hands


Vulnerable Code

```C#
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-XSS-Protection", "0" );
    await next();
});

```

Setting the `X-XSS-Protection` response header with a value of 0 will instruct the browser to disable its XSS filtering and its protection against `Cross-Site Scripting (XSS)`. XSS Filters is a browser security feature that defends users from Cross-Site Scripting attacks


Resolution

```c#
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block" );
    await next();
});

```

While X-XSS-Protection is already deprecated, it is still a useful HTTP header to enable in your web application if you
anticipate users will still be utilizing older browsers

When HTTP security headers are specified as part of the response that the web server or web
application sends, this instructs the web browser to enable protection from XSS, clickjacking, and
other types of web application-related vulnerabilities

There are other headers missing too

```c#
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    context.Response.Headers .Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers .Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("Referrer-Policy", "no-referrer");
    await next();
}

```

The `X-XSS-Protection` security header tells browsers to enable their XSS filter when the directive is set to 1. The XSS filter protects against XSS and stops a page from loading when XSS is detected


`X-Content-Type-Options` header, if assigned with a nosniff directive, prevents MIME snifing. MIME-type sniffing is a browser behavior where it guesses what the MIME-type is of a resource of a page, but this behavior can be tricked into executing malicious content. This response header tells the browser to believe the Content-Type header's value and not attempt to guess the page's mime type implicitly

Absence of the Content-Type response header is usually marked as a vulnerability
finding by most Dynamic Application Security Testing tools. Use X-Content-Type-Options:
nosniff in conjunction with an explicitly declared Content-Type response header. The X-Content-Type-Options header is supported in all major browsers, such as Firefox, Chrome,
and Edge.

X-Frame-Options is also an HTTP response header, which when set to DENY will tell the browser to not
allow the page to be rendered or embedded in any of the following HTML elements: `<iframe>, <frame>, <embed>, or <object>`. Malicious websites abuse these HTML elements to masquerade as authentic by embedding or framing the legitimate websites inside. Users are tricked in to clicking on the UI on what they presume rendered in front of the browser is a legit
website. This attack is called Click-Jacking

Referrer-Policy: Lastly, to keep sensitive information in your ASP.NET Core web application URLs from getting exposed during cross-site requests, set your Referrer-Policy to no-referrer. The Referrer header shows the URL where the user's request originated, and the no-referrer value explicitly instructs the browser to remove the Referrer from the HTTP header

## Disable Unnecessary Features

Most ASP.NET Core web application features are useful, but some can be unnecessary or
sometimes even harmful. Web developers must consider whether a web server or application
functionality needs to be enabled in code. We need to remove some features to keep our
ASP.NET Core web applications secure.

By default, Kestrel is the web server used and send this information in the header

Resolution

Disable sending this Server header back to the browser as a response, limiting
the malicious agent's information as to what type of platform the web application is running on. Knowing this detail gives the bad actor leverage on what specific exploits it could execute against the app.

Other unwanted HTTP headers include X-Powered-By and X-AspNet-Version. Both HTTP headers divulge software information about the web host that may benefit a threat actor.
If you're hosting your ASP.NET Core web application in IIS, these headers are typically present by default, and it best to remove them


## Remove passwords from your code


## Information exposure through logs

Logging in an ASP.NET Core web application is configured in the Logging section of
appsettings.json. In each of the log categories (Default, Microsoft, and
Microsoft.Hosting.Lifetime) defined in this file, their values are set to Trace, which is the lowest LogLevel with a value of 0. This LogLevel is not best for a production environment, which is why we modified the values and changed them to Warning.

Vulnerable Snippet

```json
  "Logging": {
    "LogLevel": {
      "Default": "Trace",
      "Microsoft": "Trace",
      "Microsoft.Hosting.Lifetime": "Trace"
    }
  },

```
The Trace value specifies that the web application will write trace logs into all log providers. This value is at its minimum level, which specifies that anything
else higher will also be logged, including sensitive debugging information

Solution

```json

  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Warning"
    }
  },
```
Logging in an ASP.NET Core web application is configured in the Logging section of
appsettings.json. In each of the log categories (Default, Microsoft, and
Microsoft.Hosting.Lifetime) defined in this file, their values are set to Trace, which is the lowest LogLevel with a value of 0. This LogLevel is not best for a production environment, which is why we modified the values and changed them to Warning.

## Insecure Cookies

Setting the SecurePolicy property of CookiePolicyOptions with an enum value of
CookieSecurePolicy.None, this configuration indicates that the session cookie in our sample
app will not have a Secure attribute.
Also, having the HttpOnly property of the session cookies assigned with a false value makes
the session cookies readable by a client-side script.

Vulnerable Code

```c#
builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".OnlineBanking.Session";
    options.IdleTimeout = TimeSpan.FromSeconds(10);
    options.Cookie.HttpOnly = false;
    options.Cookie.SecurePolicy = CookieSecurePolicy.None;
    options.Cookie.IsEssential = true;
});
```

Resolution

```c#
builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".OnlineBanking.Session";
    options.IdleTimeout = TimeSpan.FromSeconds(10);
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.IsEssential = true;
});
```

To prevent a malicious arbitrary JavaScript code from reading the values of our session cookies, we set the HttpOnly property of the session state service to true. Enabling the HttpOnly property will mark the session cookies with an HttpOnly attribute.


assign the SecurePolicy property of the CookiePolicyOptions with CookieSecurePolicy.Always to ensure  that the cookie policy middleware will mark the cookies with a Secure attribute


HttpOnly and Secure are two of the most important attributes in cookies yet are optional. These cookie attributes must be explicitly declared and configured as part of the cookie policy middleware. We append the HttpOnly attribute in session cookies by setting the HttpOnly
property to true:

Without the HttpOnly attribute, an arbitrary client-side script can read cookie values that
potentially could contain sensitive data. No JavaScript code will be able to retrieve values from the document.cookie property

> Cookies that we typically mark with an HttpOnly attribute contain values that may be at risk of exploitation from attacks such as Session Hijacking or Cross-Site Scripting (XSS).

CookieSecurePolicy.Always ensures that the cookie is only sent over HTTPS.
