import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthManager, RegisterRequest } from '../../models/models';
import { AuthService } from '../../services/auth.service';

@Component({ selector: 'app-signup', standalone: true, imports: [CommonModule, FormsModule, RouterLink], templateUrl: './signup.component.html' })
export class SignupComponent implements OnInit {
  managers: AuthManager[] = [];
  form: RegisterRequest = this.emptyForm();
  loading = false;
  error = '';

  constructor(private readonly auth: AuthService, private readonly router: Router) {}

  ngOnInit(): void {
    this.auth.getManagers().subscribe({
      next: managers => this.managers = managers,
      error: () => this.error = 'Could not load managers. Please try again later.'
    });
  }

  get canSubmit(): boolean { return !this.loading && this.managers.length > 0; }

  signup(signupForm: NgForm): void {
    this.error = '';
    this.form.email = this.form.email.trim().toLowerCase();

    if (signupForm.invalid || !this.form.managerID) {
      signupForm.control.markAllAsTouched();
      this.error = !this.form.managerID ? 'Please choose your manager.' : '';
      return;
    }

    this.loading = true;
    this.auth.register({
      ...this.form,
      firstName: this.form.firstName.trim(),
      lastName: this.form.lastName.trim(),
      email: this.form.email.trim(),
      department: this.form.department.trim()
    }).subscribe({
      next: response => {
        this.loading = false;
        this.router.navigateByUrl(this.auth.redirectPathForRole(response.user.role));
      },
      error: err => {
        this.loading = false;
        this.error = err?.error?.message ?? 'Could not create your account.';
      }
    });
  }

  normalizeEmail(): void { this.form.email = this.form.email.trim().toLowerCase(); }

  private emptyForm(): RegisterRequest {
    return { firstName: '', lastName: '', email: '', department: '', password: '', managerID: 0 };
  }
}
