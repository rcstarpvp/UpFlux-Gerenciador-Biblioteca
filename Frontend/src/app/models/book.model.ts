export interface Book {
  id: number;
  title: string;
  author: string;
  createdAt: string;
  updatedAt?: string;
  loans?: Loan[];
  isAvailable: boolean;
}

export interface CreateBookDto {
  title: string;
  author: string;
}

export interface UpdateBookDto {
  title: string;
  author: string;
}

export interface BookDto {
  id: number;
  title: string;
  author: string;
  createdAt: string;
  updatedAt?: string;
  isAvailable: boolean;
}

export interface Loan {
  id: number;
  bookId: number;
  user: string;
  borrowed: string;
  returned?: string;
  createdAt: string;
  updatedAt?: string;
  isActive: boolean;
}
