import { Component } from '@angular/core';
import { DomSanitizer, SafeResourceUrl, SafeUrl } from '@angular/platform-browser';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  title = 'xss-demo';
  dangerousUrl: string;
  trustedUrl: SafeUrl;
  dangerousVideoUrl: string;
  videoUrl: SafeResourceUrl;
 
   htmlSnippet = "<div>Hello Angular</div>";
   htmlTemplate= "Template <script>alert('XSS Alert')</script> <b>Syntax</b>";

   dynamicCSS="color:red; javascript:alert('XSS Attack')";

   constructor(private sanitizer: DomSanitizer){
    this.dangerousUrl = 'javascript:alert("Hi there")';
    this.trustedUrl = sanitizer.bypassSecurityTrustUrl(this.dangerousUrl);
    this.updateVideoUrl('PUBnlbjZFAI');
   }

   updateVideoUrl(id: string) {
  
    this.dangerousVideoUrl = 'https://www.youtube.com/embed/' + id;
    this.videoUrl =
        this.sanitizer.bypassSecurityTrustResourceUrl(this.dangerousVideoUrl);
  }
}
