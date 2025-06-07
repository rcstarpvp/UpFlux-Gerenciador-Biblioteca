import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { BookDto, CreateBookDto } from '../models/book.model';
import { BookService } from '../services/book.service';

@Component({
  selector: 'app-books',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="books-container">
      <h2>Gerenciamento de Livros</h2>
      
      <!-- Search -->
      <div class="search-section">
        <input 
          type="text" 
          [(ngModel)]="searchQuery" 
          placeholder="Buscar por título ou autor..."
          class="search-input"
        >
        <button (click)="search()" class="btn btn-primary">Buscar</button>
        <button (click)="loadBooks()" class="btn btn-secondary">Limpar</button>
      </div>

      <!-- Add Book Form -->
      <div class="add-book-section">
        <h3>Adicionar Novo Livro</h3>        <form (ngSubmit)="addBook()" #bookForm="ngForm">
          <div class="form-row">
            <input 
              type="text" 
              [(ngModel)]="newBook.title" 
              name="title"
              placeholder="Título" 
              required
              class="form-input"
            >
            <input 
              type="text" 
              [(ngModel)]="newBook.author" 
              name="author"
              placeholder="Autor" 
              required
              class="form-input"
            >
            <button type="submit" [disabled]="!bookForm.form.valid" class="btn btn-success">
              Adicionar Livro
            </button>
          </div>
        </form>
      </div>

      <!-- Books List -->
      <div class="books-list">
        <h3>Lista de Livros</h3>
        <div class="books-grid">
          <div *ngFor="let book of books" class="book-card">
            <div class="book-header">
              <h4>{{ book.title }}</h4>
              <span class="availability" [class.available]="book.isAvailable" [class.unavailable]="!book.isAvailable">
                {{ book.isAvailable ? 'Disponível' : 'Emprestado' }}            </span>
            </div>
            <p><strong>Autor:</strong> {{ book.author }}</p>
            <p><strong>Disponível:</strong> 
              <span [class]="book.isAvailable ? 'status-available' : 'status-unavailable'">
                {{ book.isAvailable ? 'Sim' : 'Não' }}
              </span>
            </p>
            <p><strong>Criado em:</strong> {{ formatDate(book.createdAt) }}</p>
            <div class="book-actions">
              <button (click)="deleteBook(book.id)" class="btn btn-danger btn-sm">
                Deletar
              </button>
            </div>
          </div>
        </div>
        
        <div *ngIf="books.length === 0" class="no-books">
          Nenhum livro encontrado.
        </div>
      </div>
    </div>
  `,
  styles: [`
    .books-container {
      max-width: 1200px;
      margin: 0 auto;
      padding: 20px;
    }

    .search-section {
      margin-bottom: 30px;
      display: flex;
      gap: 10px;
      align-items: center;
    }

    .search-input {
      flex: 1;
      padding: 10px;
      border: 1px solid #ddd;
      border-radius: 4px;
      font-size: 16px;
    }

    .add-book-section {
      background: #f8f9fa;
      padding: 20px;
      border-radius: 8px;
      margin-bottom: 30px;
    }

    .form-row {
      display: flex;
      gap: 15px;
      margin-bottom: 15px;
    }

    .form-input {
      flex: 1;
      padding: 10px;
      border: 1px solid #ddd;
      border-radius: 4px;
      font-size: 14px;
    }

    .books-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
      gap: 20px;
    }

    .book-card {
      background: white;
      border: 1px solid #ddd;
      border-radius: 8px;
      padding: 20px;
      box-shadow: 0 2px 4px rgba(0,0,0,0.1);
    }

    .book-header {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
      margin-bottom: 15px;
    }

    .book-header h4 {
      margin: 0;
      color: #333;
    }

    .availability {
      padding: 4px 8px;
      border-radius: 4px;
      font-size: 12px;
      font-weight: bold;
    }

    .availability.available {
      background: #d4edda;
      color: #155724;
    }

    .availability.unavailable {
      background: #f8d7da;
      color: #721c24;
    }

    .book-actions {
      margin-top: 15px;
    }

    .btn {
      padding: 8px 16px;
      border: none;
      border-radius: 4px;
      cursor: pointer;
      font-size: 14px;
      font-weight: 500;
    }

    .btn-primary {
      background: #007bff;
      color: white;
    }

    .btn-secondary {
      background: #6c757d;
      color: white;
    }

    .btn-success {
      background: #28a745;
      color: white;
    }

    .btn-danger {
      background: #dc3545;
      color: white;
    }

    .btn-sm {
      padding: 6px 12px;
      font-size: 12px;
    }

    .btn:disabled {
      opacity: 0.6;
      cursor: not-allowed;
    }

    .no-books {
      text-align: center;
      color: #666;
      font-style: italic;
      padding: 40px;
    }

    h2, h3 {
      color: #333;
      margin-bottom: 20px;
    }

    p {
      margin: 8px 0;
      color: #555;
    }
  `]
})
export class BooksComponent implements OnInit {
  books: BookDto[] = [];
  searchQuery: string = '';
  newBook: CreateBookDto = {
    title: '',
    author: ''
  };

  constructor(private bookService: BookService) {}

  ngOnInit() {
    this.loadBooks();
  }

  loadBooks() {
    this.bookService.getAll().subscribe({
      next: (books) => {
        this.books = books;
      },      error: (error) => {
        console.error('Erro ao carregar livros:', error);
        if (typeof window !== 'undefined') {
          alert('Erro ao carregar livros. Verifique se a API está rodando.');
        }
      }
    });
  }

  search() {
    if (this.searchQuery.trim()) {
      this.bookService.search(this.searchQuery).subscribe({
        next: (books) => {
          this.books = books;
        },        error: (error) => {
          console.error('Erro na busca:', error);
          if (typeof window !== 'undefined') {
            alert('Erro na busca.');
          }
        }
      });
    }
  }

  addBook() {
    this.bookService.create(this.newBook).subscribe({      next: (book) => {
        this.books.push(book);
        this.resetForm();
        if (typeof window !== 'undefined') {
          alert('Livro adicionado com sucesso!');
        }
      },
      error: (error) => {
        console.error('Erro ao adicionar livro:', error);
        if (typeof window !== 'undefined') {
          alert('Erro ao adicionar livro.');
        }
      }
    });
  }
  deleteBook(id: number) {
    if (typeof window !== 'undefined' && confirm('Tem certeza que deseja deletar este livro?')) {
      this.bookService.delete(id).subscribe({        next: () => {
          this.books = this.books.filter(book => book.id !== id);
          if (typeof window !== 'undefined') {
            alert('Livro deletado com sucesso!');
          }
        },
        error: (error) => {
          console.error('Erro ao deletar livro:', error);
          if (typeof window !== 'undefined') {
            alert('Erro ao deletar livro.');
          }
        }
      });
    }
  }

  private resetForm() {    this.newBook = {
      title: '',
      author: ''
    };
  }

  formatDate(dateString: string): string {
    const date = new Date(dateString);
    return date.toLocaleDateString('pt-BR');
  }
}
