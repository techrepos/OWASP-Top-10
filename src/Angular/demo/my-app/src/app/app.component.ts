import { Component, OnInit } from '@angular/core';
import { DomSanitizer } from '@angular/platform-browser';
import { PostService } from './post.service';
import { Post } from './post';

@Component({
  selector: 'my-app',
  templateUrl: './app.component.html',
  styleUrls: [ './app.component.css' ],
  providers: [ PostService ]
})
export class AppComponent implements OnInit {
  name = 'Angular 5';

  constructor(private sanitizer: DomSanitizer,
    private postService: PostService) { }

  sanitizedHtml = "<h4>Some rogue html</h4>";
  trustedHtml = this.sanitizer.bypassSecurityTrustHtml(this.sanitizedHtml);

  sanitizedUrl = "javascript:alert('XSS Attack')";
  trustedUrl = this.sanitizer.bypassSecurityTrustUrl(this.sanitizedUrl);

  posts: Post[] = [];
  showPosts = false;
  ngOnInit() {
    this.posts = this.postService.getPosts();
  }

  togglePosts() {
    this.showPosts = !this.showPosts;
  }
}
