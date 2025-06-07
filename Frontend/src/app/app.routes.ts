import { Routes } from '@angular/router';
import { BooksComponent } from './components/books.component';
import { LoansComponent } from './components/loans.component';
import { DashboardComponent } from './components/dashboard.component';

export const routes: Routes = [
  { path: '', redirectTo: '/dashboard', pathMatch: 'full' },
  { path: 'dashboard', component: DashboardComponent },
  { path: 'books', component: BooksComponent },
  { path: 'loans', component: LoansComponent }
];
