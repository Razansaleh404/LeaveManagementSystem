import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Employee, LeaveRequestCreate, LeaveType } from '../../models/models';
import { EmployeeService } from '../../services/employee.service';
import { LeaveRequestService } from '../../services/leave-request.service';
import { LeaveTypeService } from '../../services/leave-type.service';

@Component({ selector: 'app-leave-request', standalone: true, imports: [CommonModule, FormsModule], templateUrl: './leave-request.component.html' })
export class LeaveRequestComponent implements OnInit {
  employees: Employee[] = [];
  leaveTypes: LeaveType[] = [];
  form: LeaveRequestCreate = { employeeID: 0, leaveTypeID: 0, fromDate: '', toDate: '', reason: '' };
  numberOfDays = 0;
  loading = false;
  message = '';
  error = '';

  constructor(private readonly employeesApi: EmployeeService, private readonly leaveTypesApi: LeaveTypeService, private readonly requestsApi: LeaveRequestService) {}
  ngOnInit(): void { this.employeesApi.getAll().subscribe(data => this.employees = data.filter(e => e.isActive)); this.leaveTypesApi.getAll().subscribe(data => this.leaveTypes = data); }
  get today(): string { return new Date().toISOString().slice(0, 10); }

  calculateDays(): void {
    if (!this.form.fromDate || !this.form.toDate || this.form.toDate < this.form.fromDate) { this.numberOfDays = 0; return; }
    const start = new Date(this.form.fromDate); const end = new Date(this.form.toDate);
    this.numberOfDays = Math.floor((end.getTime() - start.getTime()) / 86400000) + 1;
  }

  validationError(): string {
    if (this.form.fromDate && this.form.fromDate < this.today) return 'Leave dates cannot be in the past.';
    if (this.form.toDate && this.form.toDate < this.today) return 'Leave dates cannot be in the past.';
    if (this.form.fromDate && this.form.toDate && this.form.toDate < this.form.fromDate) return 'To date must be greater than or equal to from date.';
    if (!this.form.reason.trim()) return 'Reason is required.';
    return '';
  }

  submit(): void {
    const validation = this.validationError();
    if (validation) { this.error = validation; return; }
    this.loading = true; this.error = ''; this.message = '';
    this.requestsApi.create(this.form).subscribe({
      next: () => { this.message = 'Leave request submitted successfully.'; this.form = { employeeID: 0, leaveTypeID: 0, fromDate: '', toDate: '', reason: '' }; this.numberOfDays = 0; this.loading = false; },
      error: err => { this.error = err?.error?.message ?? 'Could not submit request.'; this.loading = false; }
    });
  }
}
