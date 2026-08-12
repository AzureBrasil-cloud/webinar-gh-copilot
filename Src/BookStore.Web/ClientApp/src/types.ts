export interface PagedResult<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
  hasPrevious: boolean;
  hasNext: boolean;
}

export interface BookDto {
  id: number;
  title: string;
  isbn: string;
  genre: string;
  price: number;
  stock: number;
  numberOfPages: number;
  isAvailable: boolean;
  publishedDate: string;
  authorId: number;
  authorName: string;
}

export interface AuthorDto {
  id: number;
  name: string;
  nationality: string;
  birthDate: string;
  age: number;
  booksCount: number;
}

export interface CustomerDto {
  id: number;
  fullName: string;
  email: string;
  phoneNumber: string | null;
  createdAt: string;
}
