import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { Employee } from '../../models/models';
import { EmployeeService } from '../../services/employee.service';

@Component({ selector: 'app-employee-management', standalone: true, imports: [CommonModule, FormsModule], templateUrl: './employee-management.component.html' })
export class EmployeeManagementComponent implements OnInit {
  employees: Employee[] = [];
  form: Employee = this.emptyEmployee();
  searchTerm = '';
  loading = false;
  saving = false;
  message = '';
  error = '';

  constructor(private readonly employeeService: EmployeeService) {}

  ngOnInit(): void { this.loadEmployees(); }

  loadEmployees(): void {
    this.loading = true;
    this.employeeService.getAll().subscribe({
      next: data => { this.employees = data; this.loading = false; },
      error: err => this.showError(err, 'Could not load employees.')
    });
  }

  search(): void {
    if (!this.searchTerm.trim()) { this.loadEmployees(); return; }
    this.loading = true;
    this.employeeService.search(this.searchTerm).subscribe({
      next: data => { this.employees = data; this.loading = false; },
      error: err => this.showError(err, 'Search failed.')
    });
  }

  save(): void {
    this.saving = true;
    this.error = '';
    if (this.form.employeeID) {
      this.employeeService.update(this.form).subscribe({
        next: () => this.afterSave('Employee updated.'),
        error: err => this.showError(err, 'Could not save employee.')
      });
    } else {
      this.employeeService.create(this.form).subscribe({
        next: () => this.afterSave('Employee added.'),
        error: err => this.showError(err, 'Could not save employee.')
      });
    }
  }

  edit(employee: Employee): void { this.form = { ...employee }; this.message = ''; this.error = ''; }
  resetForm(): void { this.form = this.emptyEmployee(); }

  delete(employee: Employee): void {
    if (!confirm(`Delete ${employee.firstName} ${employee.lastName}?`)) return;
    this.employeeService.delete(employee.employeeID).subscribe({
      next: () => { this.message = 'Employee deleted.'; this.loadEmployees(); },
      error: err => this.showError(err, 'Could not delete employee. Remove related leave requests first.')
    });
  }

  private afterSave(message: string): void { this.message = message; this.resetForm(); this.loadEmployees(); this.saving = false; }
  private emptyEmployee(): Employee { return { employeeID: 0, firstName: '', lastName: '', email: '', department: '', isActive: true }; }
  private showError(err: any, fallback: string): void { this.error = err?.error?.message ?? fallback; this.loading = false; this.saving = false; }
}
