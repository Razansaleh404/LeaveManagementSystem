import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from './services/auth.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive],
  template: `
    <nav class="navbar navbar-expand-lg app-navbar sticky-top">
      <div class="container">
        <a class="navbar-brand d-flex align-items-center gap-2" routerLink="/request-leave">
          <span class="brand-mark">L</span>
          <span>Leave Management</span>
        </a>
        <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#mainNav" aria-controls="mainNav" aria-expanded="false" aria-label="Toggle navigation">
          <span class="navbar-toggler-icon"></span>
        </button>
        <div class="collapse navbar-collapse" id="mainNav">
          <ul class="navbar-nav ms-auto gap-lg-1 align-items-lg-center">
            <ng-container *ngIf="session$ | async as session; else signedOutLinks">
              <li class="nav-item" *ngIf="session.role === 'Admin'"><a class="nav-link" routerLink="/employees" routerLinkActive="active">Employees</a></li>
              <li class="nav-item" *ngIf="session.role === 'Employee'"><a class="nav-link" routerLink="/request-leave" routerLinkActive="active">Request Leave</a></li>
              <li class="nav-item" *ngIf="session.role === 'Employee' || session.role === 'Manager' || session.role === 'Admin'"><a class="nav-link" routerLink="/history" routerLinkActive="active">History</a></li>
              <li class="nav-item" *ngIf="session.role === 'Manager' || session.role === 'Admin'"><a class="nav-link" routerLink="/manager" routerLinkActive="active">Manager</a></li>
              <li class="nav-item session-chip" aria-label="Current user">
                <span class="session-name">{{ session.displayName }}</span>
                <span class="session-role">{{ session.role }}</span>
              </li>
              <li class="nav-item"><button class="btn btn-outline-secondary btn-sm logout-button" type="button" (click)="logout()">Logout</button></li>
            </ng-container>
            <ng-template #signedOutLinks>
              <li class="nav-item"><a class="nav-link" routerLink="/login" routerLinkActive="active">Login</a></li>
            </ng-template>
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
  session$;

  constructor(private readonly auth: AuthService, private readonly router: Router) {
    this.session$ = this.auth.session$;
  }

  logout(): void {
    this.auth.logout();
    this.router.navigate(['/login']);
  }
}
