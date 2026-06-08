import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Employee } from '../../models/models';
import { EmployeeService } from '../../services/employee.service';
import { AuthService } from '../../services/auth.service';

@Component({ selector: 'app-login', standalone: true, imports: [CommonModule, FormsModule], templateUrl: './login.component.html' })
export class LoginComponent implements OnInit {
  employees: Employee[] = [];
  selectedEmployeeId = 0;
  loading = false;
  error = '';
  returnUrl = '/request-leave';

  constructor(
    private readonly auth: AuthService,
    private readonly employeeService: EmployeeService,
    private readonly router: Router,
    private readonly route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    this.returnUrl = this.route.snapshot.queryParamMap.get('returnUrl') || '/request-leave';
    this.loading = true;
    this.employeeService.getAll().subscribe({
      next: data => { this.employees = data.filter(employee => employee.isActive); this.loading = false; },
      error: () => { this.error = 'Could not load employees for sign in.'; this.loading = false; }
    });
  }

  signInAsEmployee(): void {
    const employee = this.employees.find(item => item.employeeID === Number(this.selectedEmployeeId));
    if (!employee) { this.error = 'Choose your employee profile to request leave.'; return; }
    this.auth.loginAsEmployee(employee);
    this.router.navigateByUrl(this.safeReturnUrl('/request-leave'));
  }

  signInAsManager(): void {
    this.auth.loginAsRole('Manager');
    this.router.navigateByUrl(this.safeReturnUrl('/manager'));
  }

  signInAsAdmin(): void {
    this.auth.loginAsRole('Admin');
    this.router.navigateByUrl(this.safeReturnUrl('/employees'));
  }

  private safeReturnUrl(fallback: string): string {
    if (this.returnUrl === '/login' || this.returnUrl === '/') return fallback;
    return this.returnUrl.startsWith('/') ? this.returnUrl : fallback;
  }
}
