# Server Side Request Forgery

When we create web applications, we normally rely on a backend service to perform server side operations. This is either done in the form of a web service or a REST-based API hosted internally or externally from the system, and our code then calls the operations of these APIs and web services 

However, without proper filtering or being able to validate the data that's been sent to these services, the host could start executing unexpected actions. This vulnerability is otherwise known as **Server-Side Request Forgery(SSRF)**, with adversaries exploiting the lack of validation or
sanitization available.

### Vulnerable Code

In our demo app, we have an export feature which relies on another service to perform the extraction operation and returns the result to the calling method

```c#
public async Task<ContentResult> Export(String url)
{
    var response = string.Empty;

    Uri fullURL = new Uri($"https://localhost:5197/{url}");

    HttpResponseMessage result = await _httpClient.GetAsync(fullURL);
    if (result.IsSuccessStatusCode)
    {
        var content = await result.Content.ReadAsStringAsync();
            return  new ContentResult() { ContentType = "text/html", Content = content};

        
        
    }
    return new ContentResult() { Content = String.Empty };

}

```

Here we are accepting a part of the url which is constructed using a pre-defined logic. Since it is sanitized a malicious attacker can grab this and them modify it and in turn execute it using your application's context

### Resolution

To prevent this unwanted execution of an arbitrary command, we can use regular expressions to validate that the URL is correct according to the format that we expected or use a whitelist for domains

```c#

public async Task<IActionResult> Export(String url)
{
    var response = string.Empty;
    if(!url.StartsWith("DownloadAsExcel"))
    {
        return RedirectToAction("Index");
    }

    // or a whitelisting like this
    var allowedMethods = new List<String> { "DownloadAsExcel", "DownloadAsCSV", "DownloadAsPDF" };

    var parsedUrl = url.Split("?");

    if (!allowedMethods.Contains(parsedUrl[0]))
    {
        return RedirectToAction("Index");
    }

    Uri fullURL = new Uri($"https://localhost:5197/{url}");

    HttpResponseMessage result = await _httpClient.GetAsync(fullURL);
    if (result.IsSuccessStatusCode)
    {
        var content = await result.Content.ReadAsStringAsync();
            return  new ContentResult() { ContentType = "text/html", Content = content};

        
        
    }
    return new ContentResult() { Content = String.Empty };

}

```