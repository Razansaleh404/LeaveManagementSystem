import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({ selector: 'app-login', standalone: true, imports: [CommonModule, FormsModule], templateUrl: './login.component.html' })
export class LoginComponent {
  email = '';
  password = '';
  loading = false;
  error = '';

  readonly demoAccounts = [
    { label: 'Employee demo', email: 'employee@leave.local', password: 'Password123!' },
    { label: 'Manager demo', email: 'manager@leave.local', password: 'Password123!' }
  ];

  constructor(private readonly auth: AuthService, private readonly router: Router) {}

  login(loginForm: NgForm): void {
    this.error = '';

    if (loginForm.invalid) {
      loginForm.control.markAllAsTouched();
      return;
    }

    this.loading = true;
    const email = this.email.trim().toLowerCase();
    this.auth.login(email, this.password).subscribe({
      next: response => {
        this.loading = false;
        this.router.navigateByUrl(this.auth.redirectPathForRole(response.user.role));
      },
      error: err => {
        this.loading = false;
        this.error = err?.error?.message ?? 'Could not sign in. Make sure the email exists, is active, and the password is correct.';
      }
    });
  }

  useDemo(account: { email: string; password: string }, loginForm: NgForm): void {
    this.email = account.email;
    this.password = account.password;
    this.error = '';
    loginForm.form.markAsPristine();
    loginForm.form.markAsUntouched();
  }
}
