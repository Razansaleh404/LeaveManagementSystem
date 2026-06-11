import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
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

  ngOnInit(): void {
    this.form.employeeID = this.auth.currentUser?.employeeID ?? 0;
    this.leaveTypesApi.getAll().subscribe({
      next: data => this.leaveTypes = data,
      error: () => this.error = 'Could not load leave types. Please refresh the page and try again.'
    });
  }

  get today(): string { return new Date().toISOString().slice(0, 10); }
  get currentUser() { return this.auth.currentUser; }
  get isReasonMissing(): boolean { return !this.form.reason.trim(); }
  get isDateRangeInvalid(): boolean { return !!this.form.fromDate && !!this.form.toDate && this.form.toDate < this.form.fromDate; }

  get selectedLeaveTypeName(): string {
    return this.leaveTypes.find(type => type.leaveTypeID === this.form.leaveTypeID)?.leaveName ?? 'Not selected';
  }

  get dateSummary(): string {
    if (!this.form.fromDate && !this.form.toDate) return 'No dates selected';
    if (this.form.fromDate && !this.form.toDate) return `Starts ${this.form.fromDate}`;
    if (!this.form.fromDate && this.form.toDate) return `Ends ${this.form.toDate}`;
    return `${this.form.fromDate} to ${this.form.toDate}`;
  }

  calculateDays(): void {
    if (!this.form.fromDate || !this.form.toDate || this.isDateRangeInvalid) {
      this.numberOfDays = 0;
      return;
    }

    const start = new Date(this.form.fromDate);
    const end = new Date(this.form.toDate);
    this.numberOfDays = Math.floor((end.getTime() - start.getTime()) / 86400000) + 1;
  }

  validationError(): string {
    if (!this.form.employeeID) return 'Could not determine the signed-in employee.';
    if (!this.form.leaveTypeID) return 'Leave type is required.';
    if (!this.form.fromDate) return 'From date is required.';
    if (!this.form.toDate) return 'To date is required.';
    if (this.form.fromDate < this.today || this.form.toDate < this.today) return 'Leave dates cannot be in the past.';
    if (this.isDateRangeInvalid) return 'To date must be greater than or equal to from date.';
    if (!this.form.reason.trim()) return 'Reason is required.';
    return '';
  }

  submit(requestForm: NgForm): void {
    this.form.employeeID = this.auth.currentUser?.employeeID ?? 0;
    const validation = this.validationError();
    if (requestForm.invalid || validation) {
      requestForm.control.markAllAsTouched();
      this.error = validation || 'Please complete all required fields.';
      return;
    }

    this.loading = true;
    this.error = '';
    this.message = '';
    const request = { ...this.form, reason: this.form.reason.trim() };
    this.requestsApi.create(request).subscribe({
      next: () => {
        this.message = 'Leave request submitted successfully.';
        this.reset(requestForm);
        this.loading = false;
      },
      error: err => {
        this.error = err?.error?.message ?? 'Could not submit request.';
        this.loading = false;
      }
    });
  }

  reset(requestForm: NgForm): void {
    this.form = this.emptyRequest();
    this.numberOfDays = 0;
    this.error = '';
    requestForm.resetForm(this.form);
  }

  private emptyRequest(): LeaveRequestCreate {
    return { employeeID: this.auth.currentUser?.employeeID ?? 0, leaveTypeID: 0, fromDate: '', toDate: '', reason: '' };
  }
}
