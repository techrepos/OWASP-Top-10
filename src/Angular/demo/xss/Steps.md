
## Preventing Cross Site Scripting(XSS)

To block XSS attacks, you must prevent malicious code from entering the Document Object Model (DOM)

To systematically block XSS bugs, Angular treats all values as untrusted by default. When a value is inserted into the DOM from a template binding, or interpolation, Angular sanitizes and escapes untrusted values

In `app.component.ts` define a variable with html content

```ts
   htmlSnippet = "<div>Hello Angular</div>";
```

And in the `app.component.html` file
```html
<div>Some snippet</div>
{{htmlSnippet}}
```
 
 The interpolation mechanism in Angular will escape your input. It won't interpret the input as HTML and will display the entire input as plain text on your webpage. This defense mechanism in Angular is known as "contextual escaping."

 Interpolated content is always escaped —the HTML isn't interpreted and the browser displays angle brackets in the element's text content.


 For the HTML to be interpreted, bind it to an HTML property such as `innerHTML`. Be aware that binding a value that an attacker might control into `innerHTML` normally causes an XSS vulnerability

 ```typescript
htmlTemplate= "Template <script>alert('XSS Alert')</script> <b>Syntax</b>";
 ```


 ```html
<h2>Inner Html </h2>
<p [innerHTML]="htmlTemplate"> </p>
 ```

Angular has automatically recognized the `<script>` tag as unsafe and removed it, but has kept the `<b>` tag as its potentially safe. This modification in Angular is called "sanitization." When you are running Angular in development mode, a warning appears to notify you if Angular has sanitized an input value.


In addition to the `[innerHtml]` property, Angular has a few other properties it uses to sanitize user inputs based on the context. The `[style]` property (which binds CSS attributes) and the `[href]` property (which binds URLs) perform similar context-based sanitization to remove possibly dangerous inputs from users

 ```typescript
dynamicCSS="color:red; javascript:alert('XSS Attack')";
 ```


 ```html
<p [style]="dynamicCSS" [innerHTML]="htmlTemplate"> </p>
 ```

 > The key concept behind Angular's out-of-the-box security model is that **Angular treats all input values as untrusted**. 

 So, if you need to use a validated user input within the application, you'll need to mark the input as trusted.

 For this, you can make use of the `DomSanitizer` and use the `byPassSecurityTrust..()` functions to tell Angular that you trust the input value.

 Angular has the following bypass functions in the DomSanitizer:

- byPassSecurityTrustHtml()
- byPassSecurityTrustStyle()
- byPassSecurityTrustScript()
- byPassSecurityTrustUrl()
- byPassSecurityTrustResourceUrl()


```typescript
<h4>An untrusted URL:</h4>
<p><a class="e2e-dangerous-url" [href]="dangerousUrl">Click me</a></p>
<h4>A trusted URL:</h4>
<p><a class="e2e-trusted-url" [href]="trustedUrl">Click me</a></p>
```

Normally, Angular automatically sanitizes the URL, disables the dangerous code, and in development mode, logs this action to the console. To prevent this, mark the URL value as a trusted URL using the `bypassSecurityTrustUrl` call:

```typescript
import { DomSanitizer, SafeUrl } from '@angular/platform-browser';

constructor(private sanitizer: DomSanitizer){
    this.dangerousUrl = 'javascript:alert("Hi there")';
    this.trustedUrl = sanitizer.bypassSecurityTrustUrl(this.dangerousUrl);
   }


```


If you need to convert user input into a trusted value, use a component method. The following template lets users enter a YouTube video ID and load the corresponding video in an `<iframe>`. The `<iframe src>` attribute is a resource URL security context, because an untrusted source can, for example, smuggle in file downloads that unsuspecting users could run. To prevent this, call a method on the component to construct a trusted video URL, which causes Angular to let binding into `<iframe src>`:



```typescript

   updateVideoUrl(id: string) {
  
    this.dangerousVideoUrl = 'https://www.youtube.com/embed/' + id;
    this.videoUrl =
        this.sanitizer.bypassSecurityTrustResourceUrl(this.dangerousVideoUrl);
  }
}
```