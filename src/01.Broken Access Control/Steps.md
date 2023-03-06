# 01. Broken Access Control

## Insecure Direct Object Reference


- Login using `https://localhost:7294/FundTransfer/Details/1` using the below credentials
> Username : testuser01@kb.ca
>
> Password : Test123456!!

Then logout and relogin using the below the credentials

> Username : UserEmp01@kk.com
>
> Password : Test45678!!

Then use the same url in the step 1 to access the details page

So the user `UserEmp01@kk.com` was able to reach the transfer details page of another user even if he was not authorized to access that page

## Vulnerable Code

Here since the parameter was an integer value , the attacker was able to try out different combinations easily

```c#
public class FundTransfer : IValidatableObject
{
    public int ID { get; set; }

    [ForeignKey("Customer")]
    public string CustomerID { get; set; }

    //omitted rest of the code
}
```

## Remediation

- Change the ID property from int to a Guid type. Globally unique identifiers (GUIDs) are
unique identifiers and are harder to guess

- Annotating the ID property with the Key attribute makes this property the primary key for
Entity Framework to identify

```c#

public class FundTransfer : IValidatableObject
{
    [Key]
    public Guid ID { get; set; }

    //omitted rest of the code
}
```

**Using Authorization Policies**

- A authorization policy handler is defined to determine the user authorization



## improper auth

Incorrectly using ASP.NET Core's authorization components could lead to insecure code. The authorization feature offers a simple and declarative way to impose authorization, but mistakes can occur in implementing this

Right now , any user who is authenticated can access the fund transfer page, but we haven't implemented RBAC(Role based access control) yet

Modify the `[Authorize]` attribute to specify the roles for accessing this endpoint

```c#
[Authorize(Roles = "Customer, ActiveCustomer")]
public class FundTransferController : Controller
{
```

The Authorize annotation appears to have been used properly, but not quite. This combination would only be open to customers who have a Customer OR an ActiveCustomer role. Setting the Authorize annotation in this format means customers with either role can send money, which is not what we expect based on our business rule, allowing only active customers to make fund transfers

Inorder to implement the rule correctly, we need to have an AND condition, which can be implemented by specifying like  below

```c#


```

Declarative role checks enable web developers to add authorization in a page model easily, but there is a big difference between the annotations. For example,

`[Authorize(Roles = "Customer,ActiveCustomer")]`

Now, contrast it with these annotations:

`[Authorize(Roles = "Customer")]
[Authorize(Roles = "ActiveCustomer")]
`

The first one indicates that an authenticated user with either a Customer or an ActiveCustomer role can access the fund transfer page. The latter specifies that a customer needs both roles to have the authority to send money.


## missing  access control

The Authorize attribute in the CreateModel class provides the most basic authorization indicating that this  page requires authorization. However, a lack of defined roles as to which types of customers can make a fund transfer opens up an opportunity for an adversary to abuse this

We will implement policy-based authorization with criteria defined based on the current roles that our customer has

```c#
public static class PrincipalPermission{
    public static List<Func<AuthorizationHandlerContext, bool>> Criteria = new List<Func<AuthorizationHandlerContext, bool>>
    {
        CanCreateFundTransfer,
        CanViewFundTransfer
    };

    public static bool CanCreateFundTransfer(this AuthorizationHandlerContext ctx)
    {
        return ctx.User.IsInRole(Role.ActiveCustomer.ToString());
    }
    public static bool CanViewFundTransfer(this AuthorizationHandlerContext ctx)
    {
        return ctx.User.IsInRole(Role.ActiveCustomer.ToString()) ||
                ctx.User.IsInRole(Role.SuspendedCustomer.ToString());
    }

}

```


In the Program.cs class , modify the Authorization middleware configuration. 

Here we loop into each of the criteria lists we defined and create an authorization policy for each.


```c#
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();

    foreach (var criterion in PrincipalPermission.Criteria)
    {
        options.AddPolicy(criterion.Method.Name,
                    policy => policy.RequireAssertion(criterion));
    }
});    

```

Modify the FundTransfer Controller and modify the `Authorize` attribute to apply the authorization policy that we added
to the authorization service

```c#
[Authorize(Policy = nameof(PrincipalPermission.CanCreateFundTransfer))]
public class FundTransferController : Controller

```


The policy-based approach gives ASP.NET Core web developers the granularity needed to define
authorization matrices


## Open Redirect Vulnerability

Open the url `https://localhost:5001/Identity/Account/Login?ReturnUrl=https://www.packtpub.com`

You will redirected to the third party website by the app itself. An attacker can distribute this manipulated url and redirect the user to clone of the orignial website and extract sensitive information

In the login page , we are using the Redirect method to redirect the user 

The Redirect method, when invoked, sends a temporary redirect response to the browser.
With no URL validation in place, the URL redirection can be abused and sent to a website
controlled by an attacker whenever a tricked customer clicks a malicious URL


```
public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        
            if (ModelState.IsValid)
            {
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                var signInResult = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);
                if (signInResult.Succeeded)
                {
                    _log.LogInformation("User logged in.");

                    if (string.IsNullOrEmpty(HttpContext.Session.GetString(SessionKey)))
                    {
                        HttpContext.Session.SetString(SessionKey, Input.Email);
                    }

                    return Redirect(returnUrl);

``

The LocalRedirect method performs the same redirection, except it will throw an
InvalidOperationException exception when the URL is trying to redirect to a website that is
not local.


Another way to fix this security bug is to use the Url.IsLocalUrl method.

```c#
public async Task<IActionResult> OnPost(string returnUrl = null)
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            if (returnUrl != null)
            {
                if (Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);
                else
                    return RedirectToPage();
            }
            else
            {
                return RedirectToPage();
            }
        }

```