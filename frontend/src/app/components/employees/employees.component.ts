import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { Employee, EmployeePayload } from '../../models/employee';
import { ApiService } from '../../services/api.service';

@Component({
  selector: 'app-employees',
  imports: [CommonModule, FormsModule],
  templateUrl: './employees.component.html'
})
export class EmployeesComponent implements OnInit {
  employees: Employee[] = [];
  searchTerm = '';
  editingId: number | null = null;
  message = '';
  error = '';
  formModel: EmployeePayload = this.emptyEmployee();

  constructor(private readonly api: ApiService) {}

  ngOnInit(): void { this.loadEmployees(); }

  loadEmployees(): void {
    this.api.getEmployees().subscribe({ next: data => this.employees = data, error: err => this.showError(err) });
  }

  search(): void {
    this.api.searchEmployees(this.searchTerm).subscribe({ next: data => this.employees = data, error: err => this.showError(err) });
  }

  save(form: NgForm): void {
    if (form.invalid) { this.error = 'Please complete all required employee fields.'; return; }
    const request = this.editingId ? this.api.updateEmployee(this.editingId, this.formModel) : this.api.createEmployee(this.formModel);
    request.subscribe({
      next: () => {
        this.message = this.editingId ? 'Employee updated successfully.' : 'Employee added successfully.';
        this.reset(form);
        this.loadEmployees();
      },
      error: err => this.showError(err)
    });
  }

  edit(employee: Employee): void {
    this.editingId = employee.employeeID;
    this.formModel = { employeeCode: employee.employeeCode, fullName: employee.fullName, email: employee.email, department: employee.department, isActive: employee.isActive };
  }

  delete(employee: Employee): void {
    if (!confirm(`Delete ${employee.fullName}? Employees with leave history will be marked inactive.`)) return;
    this.api.deleteEmployee(employee.employeeID).subscribe({
      next: () => { this.message = 'Employee removed or marked inactive.'; this.loadEmployees(); },
      error: err => this.showError(err)
    });
  }

  reset(form?: NgForm): void {
    this.editingId = null;
    this.formModel = this.emptyEmployee();
    form?.resetForm(this.formModel);
    this.error = '';
  }

  private emptyEmployee(): EmployeePayload { return { employeeCode: '', fullName: '', email: '', department: '', isActive: true }; }
  private showError(err: any): void { this.error = err?.error?.message ?? 'Unable to complete the employee action.'; }
}
