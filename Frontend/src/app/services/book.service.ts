import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Book, CreateBookDto, UpdateBookDto, BookDto } from '../models/book.model';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class BookService {
  private apiUrl = `${environment.apiUrl}/api/books`;

  constructor(private http: HttpClient) { }

  getAll(): Observable<BookDto[]> {
    return this.http.get<BookDto[]>(this.apiUrl);
  }

  getById(id: number): Observable<BookDto> {
    return this.http.get<BookDto>(`${this.apiUrl}/${id}`);
  }

  search(query: string): Observable<BookDto[]> {
    return this.http.get<BookDto[]>(`${this.apiUrl}/search?query=${encodeURIComponent(query)}`);
  }

  create(book: CreateBookDto): Observable<BookDto> {
    return this.http.post<BookDto>(this.apiUrl, book);
  }

  update(id: number, book: UpdateBookDto): Observable<BookDto> {
    return this.http.post<BookDto>(`${this.apiUrl}/${id}/update`, book);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
