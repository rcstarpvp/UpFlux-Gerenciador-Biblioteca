export interface Loan {
  id: number;
  bookId: number;
  user: string;
  borrowed: string;
  returned?: string;
  createdAt: string;
  updatedAt?: string;
  book?: Book;
  isActive: boolean;
}

export interface CreateLoanDto {
  bookId: number;
  user: string;
}

export interface UpdateLoanDto {
  user: string;
  returned?: string;
}

export interface LoanDto {
  id: number;
  bookId: number;
  user: string;
  borrowed: string;
  returned?: string;
  createdAt: string;
  updatedAt?: string;
  isActive: boolean;
}

export interface Book {
  id: number;
  title: string;
  author: string;
  createdAt: string;
  updatedAt?: string;
  isAvailable: boolean;
}
