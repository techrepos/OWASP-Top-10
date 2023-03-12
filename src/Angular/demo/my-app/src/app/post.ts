export interface Post {
    poster: string;
    content: string;
    likes: number;
    comments: Post[]
  }