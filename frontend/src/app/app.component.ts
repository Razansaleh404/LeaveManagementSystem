import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from './services/auth.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive],
  template: `
    <nav class="navbar navbar-expand-lg app-navbar sticky-top" *ngIf="auth.user$ | async as user">
      <div class="container">
        <a class="navbar-brand d-flex align-items-center gap-2" [routerLink]="auth.redirectPathForRole(user.role)">
          <span class="brand-mark">L</span>
          <span>Leave Management</span>
        </a>
        <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#mainNav" aria-controls="mainNav" aria-expanded="false" aria-label="Toggle navigation">
          <span class="navbar-toggler-icon"></span>
        </button>
        <div class="collapse navbar-collapse" id="mainNav">
          <ul class="navbar-nav ms-auto gap-lg-1 align-items-lg-center">
            <li class="nav-item" *ngIf="user.role === 'Manager'"><a class="nav-link" routerLink="/employees" routerLinkActive="active">Employees</a></li>
            <li class="nav-item" *ngIf="user.role === 'Employee'"><a class="nav-link" routerLink="/request-leave" routerLinkActive="active">Request Leave</a></li>
            <li class="nav-item" *ngIf="user.role === 'Employee'"><a class="nav-link" routerLink="/history" routerLinkActive="active">History</a></li>
            <li class="nav-item" *ngIf="user.role === 'Manager'"><a class="nav-link" routerLink="/manager" routerLinkActive="active">Manager</a></li>
            <li class="nav-item user-chip">{{ user.firstName }} {{ user.lastName }} · {{ user.role }}</li>
            <li class="nav-item"><button type="button" class="btn btn-outline-primary btn-sm" (click)="logout()">Logout</button></li>
          </ul>
        </div>
      </div>
    </nav>
    <main class="container app-main py-4 py-lg-5">
      <router-outlet></router-outlet>
    </main>
  `
})
export class AppComponent {
  constructor(public readonly auth: AuthService, private readonly router: Router) {}

  logout(): void {
    this.auth.logout();
    this.router.navigateByUrl('/login');
  }
}
