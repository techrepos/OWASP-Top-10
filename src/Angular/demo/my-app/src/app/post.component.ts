import { Component, Input, ChangeDetectionStrategy, ViewChild, ElementRef,
    AfterViewInit } from '@angular/core';
  import { DomSanitizer } from '@angular/platform-browser';
  import { Post } from './post';
  
  @Component({
    changeDetection: ChangeDetectionStrategy.OnPush,
    selector: 'app-post',
    template: `
      <div class="card">
        <div class="card-body">
          <h5 class="card-title">{{post.poster}}</h5>
          
          <p #comment class="card-text">{{post.content}}</p>
          <div *ngFor="let comment of post.comments">
            <app-post [post]="comment"></app-post>
          </div>
          
          <a href="#" class="card-link">Comment</a>
          <a href="#" class="card-link">Like</a>
  
          <p *ngIf="post.likes == 0" class="card-subtitle">Be the first to like this comment<p>
          <p *ngIf="post.likes > 0" class="card-subtitle">{{post.likes}} likes!<p>
        </div>
      </div>`
  })
  
  export class PostComponent implements AfterViewInit {
    @Input() post: Post;
    @ViewChild('comment') comment: ElementRef;
  
    constructor(private sanitizer: DomSanitizer,
      private elementRef: ElementRef) { 
      
    }
  
    ngAfterViewInit() {
      // if(this.post.content.startsWith('<script>')) {
      //   var script = document.createElement('script');
      //   script.type = "text/javascript";
      //   script.innerHTML = 'alert("Gotcha")';
      //   this.comment.nativeElement.appendChild(script);
      // }
    }
  }