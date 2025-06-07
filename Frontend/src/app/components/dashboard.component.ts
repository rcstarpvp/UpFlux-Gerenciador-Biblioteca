import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BookService } from '../services/book.service';
import { LoanService } from '../services/loan.service';
import { BookDto } from '../models/book.model';
import { LoanDto } from '../models/loan.model';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="dashboard-container">
      <h2>Dashboard da Biblioteca</h2>
      
      <div class="stats-grid">
        <div class="stat-card">
          <div class="stat-number">{{ totalBooks }}</div>
          <div class="stat-label">Total de Livros</div>
        </div>
        
        <div class="stat-card">
          <div class="stat-number">{{ availableBooks }}</div>
          <div class="stat-label">Livros Disponíveis</div>
        </div>
        
        <div class="stat-card">
          <div class="stat-number">{{ totalLoans }}</div>
          <div class="stat-label">Total de Empréstimos</div>
        </div>
        
        <div class="stat-card">
          <div class="stat-number">{{ activeLoans }}</div>
          <div class="stat-label">Empréstimos Ativos</div>
        </div>
      </div>

      <div class="recent-section">
        <div class="recent-books">
          <h3>Livros Recentes</h3>
          <div class="recent-list">
            <div *ngFor="let book of recentBooks" class="recent-item">
              <strong>{{ book.title }}</strong>
              <span>{{ book.author }}</span>
            </div>
            <div *ngIf="recentBooks.length === 0" class="no-data">
              Nenhum livro encontrado
            </div>
          </div>
        </div>

        <div class="recent-loans">
          <h3>Empréstimos Recentes</h3>
          <div class="recent-list">            <div *ngFor="let loan of recentLoans" class="recent-item">
              <strong>Livro ID: {{ loan.bookId }}</strong>
              <span>{{ loan.user }}</span>
            </div>
            <div *ngIf="recentLoans.length === 0" class="no-data">
              Nenhum empréstimo encontrado
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .dashboard-container {
      max-width: 1200px;
      margin: 0 auto;
      padding: 20px;
    }

    .stats-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
      gap: 20px;
      margin-bottom: 40px;
    }

    .stat-card {
      background: white;
      padding: 30px 20px;
      border-radius: 12px;
      text-align: center;
      box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
      border-left: 4px solid #3498db;
    }

    .stat-number {
      font-size: 3rem;
      font-weight: bold;
      color: #2c3e50;
      margin-bottom: 10px;
    }

    .stat-label {
      font-size: 1rem;
      color: #7f8c8d;
      font-weight: 500;
    }

    .recent-section {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 30px;
    }

    .recent-books, .recent-loans {
      background: white;
      padding: 25px;
      border-radius: 12px;
      box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
    }

    .recent-books h3, .recent-loans h3 {
      margin-top: 0;
      margin-bottom: 20px;
      color: #2c3e50;
      border-bottom: 2px solid #ecf0f1;
      padding-bottom: 10px;
    }

    .recent-list {
      max-height: 300px;
      overflow-y: auto;
    }

    .recent-item {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 12px 0;
      border-bottom: 1px solid #ecf0f1;
    }

    .recent-item:last-child {
      border-bottom: none;
    }

    .recent-item strong {
      color: #2c3e50;
      flex: 1;
      margin-right: 10px;
    }

    .recent-item span {
      color: #7f8c8d;
      font-size: 0.9rem;
    }

    .no-data {
      text-align: center;
      color: #bdc3c7;
      font-style: italic;
      padding: 20px;
    }

    h2 {
      color: #2c3e50;
      margin-bottom: 30px;
      font-size: 2rem;
    }

    @media (max-width: 768px) {
      .recent-section {
        grid-template-columns: 1fr;
      }
      
      .stats-grid {
        grid-template-columns: repeat(2, 1fr);
      }
      
      .stat-number {
        font-size: 2rem;
      }
    }
  `]
})
export class DashboardComponent implements OnInit {
  totalBooks: number = 0;
  availableBooks: number = 0;
  totalLoans: number = 0;
  activeLoans: number = 0;
  recentBooks: BookDto[] = [];
  recentLoans: LoanDto[] = [];

  constructor(
    private bookService: BookService,
    private loanService: LoanService
  ) {}

  ngOnInit() {
    this.loadStats();
  }

  loadStats() {
    // Load books statistics
    this.bookService.getAll().subscribe({
      next: (books) => {
        this.totalBooks = books.length;
        this.availableBooks = books.filter(book => book.isAvailable).length;
        this.recentBooks = books.slice(-5).reverse(); // Last 5 books
      },
      error: (error) => {
        console.error('Erro ao carregar estatísticas de livros:', error);
      }
    });

    // Load loans statistics
    this.loanService.getAll().subscribe({
      next: (loans) => {
        this.totalLoans = loans.length;
        this.recentLoans = loans.slice(-5).reverse(); // Last 5 loans
      },
      error: (error) => {
        console.error('Erro ao carregar estatísticas de empréstimos:', error);
      }
    });

    // Load active loans
    this.loanService.getActiveLoans().subscribe({
      next: (activeLoans) => {
        this.activeLoans = activeLoans.length;
      },
      error: (error) => {
        console.error('Erro ao carregar empréstimos ativos:', error);
      }
    });
  }
}
