import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { LeaveRequestCreate, LeaveType } from '../../models/models';
import { AuthService } from '../../services/auth.service';
import { LeaveRequestService } from '../../services/leave-request.service';
import { LeaveTypeService } from '../../services/leave-type.service';

@Component({ selector: 'app-leave-request', standalone: true, imports: [CommonModule, FormsModule], templateUrl: './leave-request.component.html' })
export class LeaveRequestComponent implements OnInit {
  leaveTypes: LeaveType[] = [];
  form: LeaveRequestCreate = { employeeID: 0, leaveTypeID: 0, fromDate: '', toDate: '', reason: '' };
  numberOfDays = 0;
  loading = false;
  message = '';
  error = '';

  constructor(private readonly auth: AuthService, private readonly leaveTypesApi: LeaveTypeService, private readonly requestsApi: LeaveRequestService) {}
  ngOnInit(): void { this.form.employeeID = this.auth.currentUser?.employeeID ?? 0; this.leaveTypesApi.getAll().subscribe(data => this.leaveTypes = data); }
  get today(): string { return new Date().toISOString().slice(0, 10); }
  get currentUser() { return this.auth.currentUser; }
  get isReasonMissing(): boolean { return !this.form.reason.trim(); }

  calculateDays(): void {
    if (!this.form.fromDate || !this.form.toDate || this.form.toDate < this.form.fromDate) { this.numberOfDays = 0; return; }
    const start = new Date(this.form.fromDate); const end = new Date(this.form.toDate);
    this.numberOfDays = Math.floor((end.getTime() - start.getTime()) / 86400000) + 1;
  }

  validationError(): string {
    if (!this.form.employeeID) return 'Could not determine the signed-in employee.';
    if (this.form.fromDate && this.form.fromDate < this.today) return 'Leave dates cannot be in the past.';
    if (this.form.toDate && this.form.toDate < this.today) return 'Leave dates cannot be in the past.';
    if (this.form.fromDate && this.form.toDate && this.form.toDate < this.form.fromDate) return 'To date must be greater than or equal to from date.';
    if (!this.form.reason.trim()) return 'Reason is required.';
    return '';
  }

  submit(): void {
    this.form.employeeID = this.auth.currentUser?.employeeID ?? 0;
    const validation = this.validationError();
    if (validation) { this.error = validation; return; }
    this.loading = true; this.error = ''; this.message = '';
    const request = { ...this.form, reason: this.form.reason.trim() };
    this.requestsApi.create(request).subscribe({
      next: () => { this.message = 'Leave request submitted successfully.'; this.form = { employeeID: this.auth.currentUser?.employeeID ?? 0, leaveTypeID: 0, fromDate: '', toDate: '', reason: '' }; this.numberOfDays = 0; this.loading = false; },
      error: err => { this.error = err?.error?.message ?? 'Could not submit request.'; this.loading = false; }
    });
  }
}
