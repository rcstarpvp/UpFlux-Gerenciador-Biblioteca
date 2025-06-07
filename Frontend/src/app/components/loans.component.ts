import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { LoanDto, CreateLoanDto } from '../models/loan.model';
import { BookDto } from '../models/book.model';
import { LoanService } from '../services/loan.service';
import { BookService } from '../services/book.service';

@Component({
  selector: 'app-loans',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="loans-container">
      <h2>Gerenciamento de Empréstimos</h2>
      
      <!-- Create Loan Form -->
      <div class="create-loan-section">
        <h3>Novo Empréstimo</h3>
        <form (ngSubmit)="createLoan()" #loanForm="ngForm">
          <div class="form-row">
            <select 
              [(ngModel)]="newLoan.bookId" 
              name="bookId"
              required
              class="form-select"
            >
              <option value="">Selecione um livro</option>
              <option *ngFor="let book of availableBooks" [value]="book.id">
                {{ book.title }} - {{ book.author }}
              </option>
            </select>
          </div>
          <div class="form-row">
            <input 
              type="text"
              [(ngModel)]="newLoan.user" 
              name="user"
              placeholder="Nome do Usuário" 
              required
              class="form-input"
            >
            <button type="submit" [disabled]="!loanForm.form.valid" class="btn btn-success">
              Criar Empréstimo
            </button>
          </div>
        </form>
      </div>

      <!-- Filter Buttons -->
      <div class="filter-section">
        <button (click)="loadAllLoans()" class="btn btn-primary">Todos</button>
        <button (click)="loadActiveLoans()" class="btn btn-warning">Ativos</button>
      </div>

      <!-- Loans Lista -->
      <div class="loans-list">
        <h3>{{ showingActive ? 'Empréstimos Ativos' : 'Todos os Empréstimos' }}</h3>
        <div class="loans-grid">
          <div *ngFor="let loan of loans" class="loan-card">
            <div class="loan-header">
              <h4>Livro ID: {{ loan.bookId }}</h4>
              <span class="status" [class.active]="!loan.returned" [class.returned]="loan.returned">
                {{ loan.returned ? 'Devolvido' : 'Ativo' }}
              </span>
            </div>
            <p><strong>Usuário:</strong> {{ loan.user }}</p>
            <p><strong>Data do Empréstimo:</strong> {{ formatDate(loan.borrowed) }}</p>
            <p *ngIf="loan.returned"><strong>Data de Devolução:</strong> {{ formatDate(loan.returned) }}</p>
            
            <div class="loan-actions">
              <button 
                *ngIf="!loan.returned" 
                (click)="returnBook(loan.id)" 
                class="btn btn-primary btn-sm"
              >
                Devolver Livro
              </button>
              <button (click)="deleteLoan(loan.id)" class="btn btn-danger btn-sm">
                Deletar
              </button>
            </div>
          </div>
        </div>
        
        <div *ngIf="loans.length === 0" class="no-loans">
          Nenhum empréstimo encontrado.
        </div>
      </div>
    </div>
  `,
  styles: [`
    .loans-container {
      max-width: 1200px;
      margin: 0 auto;
      padding: 20px;
    }

    .create-loan-section {
      background: #f8f9fa;
      padding: 20px;
      border-radius: 8px;
      margin-bottom: 30px;
    }

    .form-row {
      display: flex;
      gap: 15px;
      margin-bottom: 15px;
      align-items: center;
    }

    .form-input, .form-select {
      flex: 1;
      padding: 10px;
      border: 1px solid #ddd;
      border-radius: 4px;
      font-size: 14px;
    }

    .btn {
      padding: 10px 20px;
      border: none;
      border-radius: 4px;
      cursor: pointer;
      font-size: 14px;
      font-weight: 500;
    }

    .btn-success {
      background-color: #28a745;
      color: white;
    }

    .btn-primary {
      background-color: #007bff;
      color: white;
    }

    .btn-warning {
      background-color: #ffc107;
      color: #212529;
    }

    .btn-danger {
      background-color: #dc3545;
      color: white;
    }

    .btn-sm {
      padding: 5px 10px;
      font-size: 12px;
    }

    .btn:disabled {
      opacity: 0.6;
      cursor: not-allowed;
    }

    .filter-section {
      margin-bottom: 20px;
    }

    .filter-section .btn {
      margin-right: 10px;
    }

    .loans-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
      gap: 20px;
    }

    .loan-card {
      background: white;
      border: 1px solid #ddd;
      border-radius: 8px;
      padding: 20px;
      box-shadow: 0 2px 4px rgba(0,0,0,0.1);
    }

    .loan-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 15px;
    }

    .loan-header h4 {
      margin: 0;
      color: #333;
    }

    .status {
      padding: 4px 8px;
      border-radius: 4px;
      font-size: 12px;
      font-weight: bold;
    }

    .status.active {
      background-color: #ffeaa7;
      color: #fdcb6e;
    }

    .status.returned {
      background-color: #d4edda;
      color: #28a745;
    }

    .loan-card p {
      margin: 5px 0;
      color: #666;
    }

    .loan-actions {
      margin-top: 15px;
      display: flex;
      gap: 10px;
    }

    .no-loans {
      text-align: center;
      color: #666;
      font-style: italic;
      padding: 40px;
    }
  `]
})
export class LoansComponent implements OnInit {
  loans: LoanDto[] = [];
  availableBooks: BookDto[] = [];
  newLoan: CreateLoanDto = {
    bookId: 0,
    user: ''
  };
  showingActive = false;

  constructor(
    private loanService: LoanService,
    private bookService: BookService
  ) {}

  ngOnInit() {
    this.loadAllLoans();
    this.loadAvailableBooks();
  }

  loadAllLoans() {
    this.loanService.getAll().subscribe({
      next: (loans: LoanDto[]) => {
        this.loans = loans;
        this.showingActive = false;
      },
      error: (error: any) => {
        console.error('Erro ao carregar empréstimos:', error);
        this.loans = [];
      }
    });
  }

  loadActiveLoans() {
    this.loanService.getActiveLoans().subscribe({
      next: (loans: LoanDto[]) => {
        this.loans = loans;
        this.showingActive = true;
      },
      error: (error: any) => {
        console.error('Erro ao carregar empréstimos ativos:', error);
        this.loans = [];
      }
    });
  }

  loadAvailableBooks() {
    this.bookService.getAll().subscribe({
      next: (books: BookDto[]) => {
        this.availableBooks = books.filter((book: BookDto) => book.isAvailable);
      },
      error: (error: any) => {
        console.error('Erro ao carregar livros:', error);
        this.availableBooks = [];
      }
    });
  }

  createLoan() {
    if (this.newLoan.bookId && this.newLoan.user) {
      this.loanService.create(this.newLoan).subscribe({
        next: (loan: LoanDto) => {
          this.loans.unshift(loan);
          this.newLoan = { bookId: 0, user: '' };
          this.loadAvailableBooks();
        },        error: (error: any) => {
          console.error('Erro ao criar empréstimo:', error);
          if (typeof window !== 'undefined') {
            alert('Erro ao criar empréstimo. Verifique se o backend está rodando.');
          }
        }
      });
    }
  }

  returnBook(loanId: number) {
    this.loanService.returnBook(loanId).subscribe({
      next: (updatedLoan: LoanDto) => {
        const index = this.loans.findIndex(l => l.id === loanId);
        if (index !== -1) {
          this.loans[index] = updatedLoan;
        }
        this.loadAvailableBooks();
      },      error: (error: any) => {
        console.error('Erro ao devolver livro:', error);
        if (typeof window !== 'undefined') {
          alert('Erro ao devolver livro. Verifique se o backend está rodando.');
        }
      }
    });
  }
  deleteLoan(loanId: number) {
    if (typeof window !== 'undefined' && confirm('Tem certeza que deseja deletar este empréstimo?')) {
      this.loanService.delete(loanId).subscribe({
        next: () => {
          this.loans = this.loans.filter(l => l.id !== loanId);
          this.loadAvailableBooks();
        },        error: (error: any) => {
          console.error('Erro ao deletar empréstimo:', error);
          if (typeof window !== 'undefined') {
            alert('Erro ao deletar empréstimo. Verifique se o backend está rodando.');
          }
        }
      });
    }
  }

  formatDate(dateString: string): string {
    if (!dateString) return '';
    const date = new Date(dateString);
    return date.toLocaleDateString('pt-BR');
  }
}
