import { Injectable } from "@angular/core";
import { Post } from './post';

export class PostService {
  public getPosts(): Post[] {
    let posts: Post[] = [{
        poster: 'RangerDanger94',
        likes: 2,
        content: 'Hello world!',
        comments: [{
          poster: 'World',
          likes: 2147483647,
          content: 'Hello to you!',
          comments: []
        }]
      },{
        poster: 'ShadyRussianHacker',
        likes: 0,
        content: '<script>alert("You\'ve been hacked!")</script>',
        comments: [{
          poster: 'JoeBloggs',
          likes: 4,
          content: 'I don\'t trust this guy',
          comments: []
        }]
      }]

    return posts;
  }
}