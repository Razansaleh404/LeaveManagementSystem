import { Component, OnInit } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { Employee } from '../../models/models';
import { EmployeeService } from '../../services/employee.service';

@Component({ selector: 'app-employee-management', standalone: true, imports: [CommonModule, FormsModule], templateUrl: './employee-management.component.html' })
export class EmployeeManagementComponent implements OnInit {
  employees: Employee[] = [];
  managers: Employee[] = [];
  private allEmployees: Employee[] = [];
  form: Employee = this.emptyEmployee();
  searchTerm = '';
  loading = false;
  saving = false;
  message = '';
  error = '';

  constructor(private readonly employeeService: EmployeeService) {}

  ngOnInit(): void { this.loadEmployees(); }

  get isEmailInUse(): boolean {
    const email = this.form.email.trim().toLowerCase();
    if (!email) return false;
    return this.allEmployees.some(employee => employee.email.toLowerCase() === email && employee.employeeID !== this.form.employeeID);
  }

  get isEmailAccepted(): boolean {
    return !!this.form.email.trim() && this.isEmailFormatValid(this.form.email) && !this.isEmailInUse;
  }

  get isManagerAssignmentMissing(): boolean { return this.form.role === 'Employee' && !this.form.managerID; }

  loadEmployees(): void {
    this.loading = true;
    this.employeeService.getAll().subscribe({
      next: data => { this.allEmployees = data; this.employees = data; this.managers = data.filter(employee => employee.role === 'Manager' && employee.isActive); this.loading = false; },
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

    if (employeeForm.invalid || this.isEmailInUse || this.isManagerAssignmentMissing) {
      employeeForm.control.markAllAsTouched();
      if (this.isEmailInUse) {
        this.error = 'Email already exists. Use a different email address.';
      } else if (this.isManagerAssignmentMissing) {
        this.error = 'Assigned manager is required for employees.';
      }
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

  managerName(employee: Employee): string {
    if (employee.role !== 'Employee') return '—';
    const manager = this.allEmployees.find(candidate => candidate.employeeID === employee.managerID);
    return manager ? `${manager.firstName} ${manager.lastName}` : 'Unassigned';
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
      password: this.form.password?.trim() || undefined,
      managerID: this.form.role === 'Employee' ? this.form.managerID : null
    };
  }

  private emptyEmployee(): Employee { return { employeeID: 0, firstName: '', lastName: '', email: '', department: '', role: 'Employee', password: '', isActive: true, managerID: null }; }
  private isEmailFormatValid(email: string): boolean { return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email.trim()); }
  private showError(err: any, fallback: string): void { this.error = err?.error?.message ?? fallback; this.loading = false; this.saving = false; }
}
