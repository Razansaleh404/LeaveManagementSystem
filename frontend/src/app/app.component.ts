import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  template: `
    <nav class="navbar navbar-expand-lg navbar-dark bg-primary shadow-sm">
      <div class="container">
        <a class="navbar-brand" routerLink="/employees">Leave Management</a>
        <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#mainNav">
          <span class="navbar-toggler-icon"></span>
        </button>
        <div class="collapse navbar-collapse" id="mainNav">
          <ul class="navbar-nav ms-auto">
            <li class="nav-item"><a class="nav-link" routerLink="/employees" routerLinkActive="active">Employees</a></li>
            <li class="nav-item"><a class="nav-link" routerLink="/request-leave" routerLinkActive="active">Request Leave</a></li>
            <li class="nav-item"><a class="nav-link" routerLink="/history" routerLinkActive="active">History</a></li>
            <li class="nav-item"><a class="nav-link" routerLink="/manager" routerLinkActive="active">Manager</a></li>
          </ul>
        </div>
      </div>
    </nav>
    <main class="container py-4">
      <router-outlet></router-outlet>
    </main>
  `
})
export class AppComponent {}
