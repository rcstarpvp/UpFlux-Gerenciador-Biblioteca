import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Loan, CreateLoanDto, UpdateLoanDto, LoanDto } from '../models/loan.model';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class LoanService {
  private apiUrl = `${environment.apiUrl}/api/loans`;

  constructor(private http: HttpClient) { }

  getAll(): Observable<LoanDto[]> {
    return this.http.get<LoanDto[]>(this.apiUrl);
  }

  getById(id: number): Observable<LoanDto> {
    return this.http.get<LoanDto>(`${this.apiUrl}/${id}`);
  }

  getByBookId(bookId: number): Observable<LoanDto[]> {
    return this.http.get<LoanDto[]>(`${this.apiUrl}/book/${bookId}`);
  }

  getActiveLoans(): Observable<LoanDto[]> {
    return this.http.get<LoanDto[]>(`${this.apiUrl}/active`);
  }

  create(loan: CreateLoanDto): Observable<LoanDto> {
    return this.http.post<LoanDto>(this.apiUrl, loan);
  }

  update(id: number, loan: UpdateLoanDto): Observable<LoanDto> {
    return this.http.post<LoanDto>(`${this.apiUrl}/${id}/update`, loan);
  }

  returnBook(id: number): Observable<LoanDto> {
    return this.http.post<LoanDto>(`${this.apiUrl}/${id}/return`, {});
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
