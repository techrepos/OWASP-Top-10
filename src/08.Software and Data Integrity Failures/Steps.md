# Cross-Site Scripting


Cross-site scripting is still one of the widespread vulnerabilities in web applications today. Also known as XSS, it is a security flaw that allows an attacker to insert malicious client-side code into an ASP.NET Core web page. The injected input is made possible because of the lack of sanitization and filtering, and the browser processes the unwanted arbitrary code

An unknowing user can view a vulnerable web page in an XSS attack where the malicious script
runs in the browser. Once the code executes, the attacker can potentially redirect the user to a rogue website, potentially steal its session cookies, or deface your ASP.NET Core web
application

Three Types

- Fixing reflected XSS
- Fixing stored/persistent XSS
- Fixing DOM XSS


## Reflected XSS

Reflected cross-site scripting is one type of XSS where a bad actor could inject code as part of the HTTP response. The reflected XSS is non-persistent and not stored in the database, but the attack payload is delivered back to the browser, reflecting the untrusted input

## Vulnerable Code

NAvigate to  loans listing page, and enter the following snippet in the search box

```javascript
<script>alert(document.cookie)</script>
```
Notice that an alert box was displayed indicating that the XSS injection was successful and proved that the Loans page is vulnerable to reflected XSS

The loan notes search term entered is displayed on the page using Html.Raw, allowing the unfiltered string rendered within the HTML markup. A bad actor can exploit the absence of output encoding by entering malicious cross-site scripting payload attacks into the search textbox

```c#

var searchString = ViewData["SearchString"];
var itemCount = Model.Count();
if (searchString is not null)
{
    <h2>Your search for @Html.Raw(searchString) returned @itemCount results</h2>
}

```
The Loans page offers a search feature to enter a keyword to locate and return the matching
records. This page was vulnerable when the search term is displayed back using the `Html.Raw`
method. `Html.Raw` is a method that returns an unencoded string, exposing this page to a
reflected XSS attack. `Html.Raw` should not be used to render user-controlled input


Solution

```c#

var searchString = ViewData["SearchString"];
var itemCount = Model.Count();
if (searchString is not null)
{
    <h2>Your search for @searchString returned @itemCount results</h2>
}


```

We mitigate this risk and prevent exploitation by removing the call to the `Html.Raw` method and use the built-in Razor syntax instead to render markup for SearchString. Razor syntax that
evaluates to a string type returns an escaped string that makes the SearchString output safe for rendering.

## Stored XSS


Vulnerable Code

Rendered in the cell is the Note data from the database using the HtmlString class, but, by
default, an instance of the HtmlString class is unencoded, and displaying the page's output
will lead to an XSS vulnerability

```c#
@foreach (var item in Model) {
        <tr>
            <td>
                @Html.DisplayFor(modelItem => item.Amount)
            </td>
            <td>
                @Html.DisplayFor(modelItem => item.PeriodInMonths)
            </td>
            <td>
                @Html.DisplayFor(modelItem => item.TransactionDate)
            </td>
            <td>
                @Html.DisplayFor(modelItem => item.Status)
            </td>
            <td>
               @{ new HtmlString(item.Note); }
                        
               </td>
            <td>
                <a asp-action="Edit" asp-route-id="@item.ID">Edit</a> |
                <a asp-action="Details" asp-route-id="@item.ID">Details</a> |
                <a asp-action="Delete" asp-route-id="@item.ID">Delete</a>
            </td>
        </tr>

```

Resolution

```c#
td>
    
    @{ new HtmlString(item.Note).Value; }
    
</td>

```

```c#

<td>
    
    @Html.DisplayFor(modelItem => item.Note)
    
</td>

```

## DOM XSS

The Document Object Model (DOM) is an object interface that represents an HTML page. The client-side script used in conjunction with the JavaScript programming language can be
written insecurely and opens up security vulnerabilities such as DOM-based XSS

DOM XSS, in contrast to reflected and stored XSS, is not a server-side exploit. The weakness is
in the client-side code when it attempts to modify the DOM to display data, but instead interprets the input into code due to a lack of encoding and proper escaping.

**Vulnerable Code**

```javascript
  <script>
        $(document).ready(function () {
            debugger;
            var param = new URLSearchParams(window.location.search);
            var searchString = param.get('SearchString');
            var message = '<br><h2> You searched for ' + searchString + '</h2>';
            $('#searchForm').append(message);
        });
    </script>

```

Dynamically appended to the page is the search term retrieved from the URL's query string. Without output escaping the untrusted message string added to the page's document object model, this insecure code could lead to a DOM-based XSS

**Resolution**

Solution is to make use of the encoding function in JS. One way is to make use of a third party library called `underscore.js` which provides an excellent encoding function

Refer the `underscore.js` in your HTML page, here a CDN  reference is used

```html
<script src="https://cdnjs.cloudflare.com/ajax/libs/underscore.js/1.13.6/underscore-min.js" integrity="sha512-2V49R8ndaagCOnwmj8QnbT1Gz/rie17UouD9Re5WxbzRVUGoftCu5IuqqtAM9+UC3fwfHCSJR1hkzNQh/2wdtg==" crossorigin="anonymous" referrerpolicy="no-referrer"></script>
```



```javascript

<script>
    $(document).ready(function () {
        debugger;
        var param = new URLSearchParams(window.location.search);
        var searchString = param.get('SearchString');
        var message = '<br><h2> You searched for ' + _.escape(searchString) + '</h2>';
        $('#searchForm').append(message);
    });
</script>
        
```


The `._escape` function available in `underscore.js` library replaces characters that could lead to DOM XSS vulnerabilities, with its HTML entity counterpart transforming searchString into a harmless string. As a general rule, avoid calls to JavaScript methods such as document.write that can render unfiltered and unencoded data. A bad actor can exploit this vector by dynamically manipulating the DOM and executing arbitrary client-side code



## Insecure Deserialization

.NET has full support for serialization and deserialization of data. This language feature allows ASP.NET Core web applications to convert in-memory objects into a stream of bytes (serialize) and rebuild these byte  streams back to an object (deserialize). 

In the process of deserialization, the data format can be either `JavaScript Object Notation(JSON)` or `Extensible Markup Language (XML)`, and it can also be in binary format. However, as with any input type, the data source can be untrustworthy or tampered with before it gets deserialized back into a web application as an in-memory object. This vulnerability is commonly known as `insecure deserialization`

There are mainly three types of vulnerability

1. Unsafe deserialization
2. Use use of insecure deserializers
3. Untrusted data deserialization


### Unsafe deserialization.

JSON.NET is one of the most widely used library in .NET Core for serializing JSON. Json.NET has a type-handling feature that can make your ASP.NET Core web application vulnerable to insecure deserialization if misused. The automatic type handling will allow the Json.NET stream deserializer to use the declared .NET type in an incoming request. Allowing your app to automatically deserialize objects based on the declared .NET type from an untrusted source can be harmful and may cause the instantiation of unexpected objects, causing arbitrary code execution in the host.


**Vulnerable Code**

```c#

// .NET Framework
 public class StartUp
{
    public void Configuration(IAppBuilder builder)
    {
        var config = new HttpConfiguration();
        config.Routes.MapHttpRoute(
            name: "DefaultApi",
            routeTemplate: "api/{controller}/{id}",
            defaults: new { id = RouteParameter.Optional }
        );
        config.MapHttpAttributeRoutes();
        config.Formatters.XmlFormatter.SupportedMediaTypes.Clear();
        config.Formatters.JsonFormatter.SerializerSettings = new
            JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            };

        builder.UseWebApi(config);
    }
}

```
JSON deserialization can get abused in **NewtonSoft JSON.net** when ``TypeNameHandling`` is different from ``None`` in the ``JsonSerializationSettings`` and by default it is set to ``None``

It can have the following options

 - None, Do not include the type name when serializing types Objects, Include the .NET type name when serializing into a JSON object structure.
 - Array, Include the .NET type name when serializing into a JSON array structure.
 - All, Always include the .NET type name when serializing.
 - Auto, Include the .NET type name when the type of the object being serialized is not the same as its declared type.

Not all objects can get deserialized with JSON .NET. The object needs to have either a empty constructor or one constructor with parameters. This is because JSON.NET internally uses reflection to initialize objects

**Unsecure Payload**

```json
{
    "$type": "System.Windows.Data.ObjectDataProvider, PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35",
    "MethodName": "Start",
    "MethodParameters": {
        "$type": "System.Collections.ArrayList, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
        "$values": [ "cmd", "/c calc" ]
    },
    "ObjectInstance": { "$type": "System.Diagnostics.Process, System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" }
}

```

This payload will work only in with .NET Framework, for .NET Core the below payload can be used

```json
{
   "$type": "System.IO.FileInfo, System.IO.FileSystem",
   "fileName": "test.txt",
   "isReadOnly": true
}

```

**Resolution**

The fix for this security flaw in the code  is by simply setting the `TypeNameHandling` property to `TypeNameHandling.None`, preventing automatic .NET type resolution.


```c#

 // .NET Framework
 config.Formatters.JsonFormatter.SerializerSettings = new
    JsonSerializerSettings
    {
        TypeNameHandling = TypeNameHandling.All
    };

```

Ensure that you are using the latest version of the serializer/deserializer libraries. Older versions may have a publicly known vulnerability that a threat actor may exploit.
It is also important to log deserialization exceptions and failures. Throw exceptions when the incoming type is not the expected type by using strongly typed objects

### Insecure deserializers

BinaryFormatter is one of the types that an ASP.NET developer can use to serialize and
deserialize data.BinaryFormatter is an insecure
type to utilize because this deserializer does not check the type that it deserializes.

**Vulnerable Code**
For instance, you can use the `BinarySerializer` to persist an object to a file

```c#

static void Main(string[] args)
{
    var o = new Sample { Path = "test" };

    using (var stream = File.OpenWrite("output.bin"))
    {
        var binaryFormatter = new BinaryFormatter();
        binaryFormatter.Serialize(stream, collection);
    }

    using (var stream = File.OpenRead("output.bin"))
    {
        var binaryFormatter = new BinaryFormatter();
        var deserialized = binaryFormatter.Deserialize(stream);
    }
}

[Serializable]
class Sample
{
    public string Path { get; set; }
}
```

But, the deserializer does not validate that the file has not been tampered. If you send a file to a server that contains a binary serialized data, you can create an instance of any type on the server. The deserializer will instantiate the object and assign the value of its properties

Deserialization can execute code from different locations:
- The constructor of each deserialized instance
- The getter/setter of each property
- The destructor when the garbage collection (GC) destroy the instance

In our sample demo project the vulnerable code is located in the action method `UploadFile` in `LoansController` class

```c#
 using (var fileStream = new FileStream(file, FileMode.Create))
{
    await Upload.CopyToAsync(fileStream);
    BinaryFormatter formatter = new BinaryFormatter();
    fileStream.Position = 0;
    emptyLoan = formatter.Deserialize(fileStream) as Loan;
}

```

When you build the project, you will saw a warning in the output window

```bash
SYSLIB0011	'BinaryFormatter.Deserialize(Stream)' is obsolete: 'BinaryFormatter serialization is obsolete and should not be used. See https://aka.ms/binaryformatter for more information.'	

```

More than being obsolete, BinaryFormatter is also unsafe. It is highly recommended to
avoid this class and to use a more secure serializer/deserializer

**Resolution**

You can prevent this by implementing a custom SerializationBinder to only allow a reduced set of type to be instantiated:

Add a new class named `LoanDeserializationBinder` and using this binder we can check the type that is being deserialized, preventing our  web app from getting exploited

```c#
public class LoanDeserializationBinder : SerializationBinder
{
    public override Type BindToType(string assemblyName, string typeName)
    {
        if (typeName.Equals("ABCBankingWebApp.Models.Loan")){
            return typeof(Loan);
        }
        return null;
    }
}

```

And then in our controller class specify our binder class in the formatter object

```c#
 using (var fileStream = new FileStream(file, FileMode.Create))
{
    await Upload.CopyToAsync(fileStream);
    BinaryFormatter formatter = new BinaryFormatter();
    formatter.Binder = new LoanDeserializationBinder();
    fileStream.Position = 0;
    emptyLoan = formatter.Deserialize(fileStream) as Loan;
}

```

BinaryFormatter takes the incoming type as it is, with no validation. To resolve our code security problem, we define a new class that inherits from the `SerializationBinder` class and assign this to the `Binder` property of the `BinaryFormatter`
instance:

> `formatter.Binder = new LoanDeserializationBinder();`

The `LoanDeserializationBinder` class checks the type and ensures that it returns the expected
`Loan` object
    
```c#
if (typeName.Equals("OnlineBankingApp.Models.Loan"))
{
    return typeof(Loan);
}
```