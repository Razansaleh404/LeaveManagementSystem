import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth.service';

type DemoAccount = { label: string; role: string; email: string; password: string; description: string };

@Component({ selector: 'app-login', standalone: true, imports: [CommonModule, FormsModule, RouterLink], templateUrl: './login.component.html' })
export class LoginComponent {
  email = '';
  password = '';
  loading = false;
  error = '';
  showPassword = false;
  selectedDemoEmail = '';

  readonly demoAccounts: DemoAccount[] = [
    {
      label: 'Employee demo',
      role: 'Employee',
      email: 'employee@leave.local',
      password: 'Password123!',
      description: 'Submit leave requests and review your request history.'
    },
    {
      label: 'Manager demo',
      role: 'Manager',
      email: 'manager@leave.local',
      password: 'Password123!',
      description: 'Review, approve, and reject assigned team leave requests.'
    },
    {
      label: 'Admin demo',
      role: 'Admin',
      email: 'admin@leave.local',
      password: 'Password123!',
      description: 'Manage users, roles, and manager assignments.'
    }
  ];

  constructor(private readonly auth: AuthService, private readonly router: Router) {}

  get canSubmit(): boolean { return !this.loading; }

  login(loginForm: NgForm): void {
    this.error = '';
    this.email = this.email.trim().toLowerCase();

    if (loginForm.invalid) {
      loginForm.control.markAllAsTouched();
      return;
    }

    this.loading = true;
    this.auth.login(this.email, this.password).subscribe({
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

  normalizeEmail(): void {
    this.email = this.email.trim().toLowerCase();
  }

  togglePasswordVisibility(): void {
    this.showPassword = !this.showPassword;
  }

  useDemo(account: DemoAccount, loginForm: NgForm): void {
    this.email = account.email;
    this.password = account.password;
    this.selectedDemoEmail = account.email;
    this.error = '';
    loginForm.form.markAsPristine();
    loginForm.form.markAsUntouched();
  }
}
