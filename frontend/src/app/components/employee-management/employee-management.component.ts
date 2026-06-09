import { Component, OnInit } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
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
    this.employeeService.search(this.searchTerm.trim()).subscribe({
      next: data => { this.employees = data; this.loading = false; },
      error: err => this.showError(err, 'Search failed.')
    });
  }

  save(employeeForm: NgForm): void {
    this.message = '';
    this.error = '';

    if (employeeForm.invalid) {
      employeeForm.control.markAllAsTouched();
      return;
    }

    this.saving = true;
    const employee = this.trimmedEmployee();

    if (employee.employeeID) {
      this.employeeService.update(employee).subscribe({
        next: () => this.afterSave('Employee updated successfully.'),
        error: err => this.showError(err, 'Could not save employee.')
      });
    } else {
      this.employeeService.create(employee).subscribe({
        next: () => this.afterSave('Employee added successfully.'),
        error: err => this.showError(err, 'Could not save employee.')
      });
    }
  }

  edit(employee: Employee): void {
    this.form = { ...employee };
    this.message = '';
    this.error = '';
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  resetForm(employeeForm?: NgForm): void {
    this.form = this.emptyEmployee();
    this.error = '';
    if (employeeForm) {
      employeeForm.resetForm(this.emptyEmployee());
    }
  }

  delete(employee: Employee): void {
    const confirmed = confirm(`Are you sure you want to delete ${employee.firstName} ${employee.lastName}? This action cannot be undone.`);
    if (!confirmed) return;

    this.employeeService.delete(employee.employeeID).subscribe({
      next: () => { this.message = 'Employee deleted successfully.'; this.error = ''; this.loadEmployees(); },
      error: err => this.showError(err, 'Could not delete employee. Remove related leave requests first.')
    });
  }

  private afterSave(message: string): void {
    this.message = message;
    this.resetForm();
    this.loadEmployees();
    this.saving = false;
  }

  private trimmedEmployee(): Employee {
    return {
      ...this.form,
      firstName: this.form.firstName.trim(),
      lastName: this.form.lastName.trim(),
      email: this.form.email.trim(),
      department: this.form.department.trim(),
      password: this.form.password?.trim() || undefined
    };
  }

  private emptyEmployee(): Employee { return { employeeID: 0, firstName: '', lastName: '', email: '', department: '', role: 'Employee', password: '', isActive: true }; }
  private showError(err: any, fallback: string): void { this.error = err?.error?.message ?? fallback; this.loading = false; this.saving = false; }
}
