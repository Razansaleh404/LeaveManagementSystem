import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({ selector: 'app-login', standalone: true, imports: [CommonModule, FormsModule], templateUrl: './login.component.html' })
export class LoginComponent {
  email = 'employee@leave.local';
  password = 'Password123!';
  loading = false;
  error = '';

  constructor(private readonly auth: AuthService, private readonly router: Router) {}

  login(): void {
    this.loading = true;
    this.error = '';
    this.auth.login(this.email, this.password).subscribe({
      next: response => {
        this.loading = false;
        this.router.navigateByUrl(this.auth.redirectPathForRole(response.user.role));
      },
      error: err => {
        this.loading = false;
        this.error = err?.error?.message ?? 'Could not sign in. Please check your credentials.';
      }
    });
  }
}
