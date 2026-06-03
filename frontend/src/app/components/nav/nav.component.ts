import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';

@Component({
  selector: 'app-nav',
  imports: [RouterLink, RouterLinkActive],
  template: `
    <nav class="navbar navbar-expand-lg navbar-dark bg-primary shadow-sm">
      <div class="container">
        <a class="navbar-brand fw-semibold" routerLink="/employees">Leave Management</a>
        <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#mainNav">
          <span class="navbar-toggler-icon"></span>
        </button>
        <div id="mainNav" class="collapse navbar-collapse">
          <ul class="navbar-nav ms-auto">
            <li class="nav-item"><a class="nav-link" routerLink="/employees" routerLinkActive="active">Employees</a></li>
            <li class="nav-item"><a class="nav-link" routerLink="/leave-request" routerLinkActive="active">New Leave Request</a></li>
            <li class="nav-item"><a class="nav-link" routerLink="/history" routerLinkActive="active">Request History</a></li>
            <li class="nav-item"><a class="nav-link" routerLink="/approval" routerLinkActive="active">Manager Approval</a></li>
          </ul>
        </div>
      </div>
    </nav>
  `
})
export class NavComponent {}
