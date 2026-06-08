import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({ selector: 'app-login', standalone: true, imports: [CommonModule, FormsModule], templateUrl: './login.component.html' })
export class LoginComponent {
  email = '';
  password = '';
  loading = false;
  error = '';

  readonly demoUsers = [
    { role: 'Admin', email: 'admin@demo.com', password: 'Admin123!' },
    { role: 'Manager', email: 'manager@demo.com', password: 'Manager123!' },
    { role: 'Employee', email: 'employee@demo.com', password: 'Employee123!' }
  ];

  constructor(private readonly auth: AuthService, private readonly router: Router) {}

  useDemo(user: { email: string; password: string }): void {
    this.email = user.email;
    this.password = user.password;
  }

  submit(): void {
    this.loading = true;
    this.error = '';
    this.auth.login({ email: this.email, password: this.password }).subscribe({
      next: response => {
        const target = response.role === 'Admin' ? '/employees' : response.role === 'Manager' ? '/manager' : '/request-leave';
        this.router.navigateByUrl(target);
      },
      error: err => {
        this.error = err?.error?.message ?? 'Login failed.';
        this.loading = false;
      }
    });
  }
}
