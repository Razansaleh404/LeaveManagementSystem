import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { Employee } from '../../models/employee';
import { LeaveType } from '../../models/leave-type';
import { ApiService } from '../../services/api.service';

@Component({ selector: 'app-leave-request', imports: [CommonModule, FormsModule], templateUrl: './leave-request.component.html' })
export class LeaveRequestComponent implements OnInit {
  employees: Employee[] = [];
  leaveTypes: LeaveType[] = [];
  message = '';
  error = '';
  model = { employeeID: 0, leaveTypeID: 0, fromDate: '', toDate: '', reason: '' };

  constructor(private readonly api: ApiService) {}
  ngOnInit(): void { this.api.getEmployees().subscribe(e => this.employees = e.filter(x => x.isActive)); this.api.getLeaveTypes().subscribe(t => this.leaveTypes = t); }

  get numberOfDays(): number {
    if (!this.model.fromDate || !this.model.toDate) return 0;
    const from = new Date(this.model.fromDate); const to = new Date(this.model.toDate);
    if (Number.isNaN(from.getTime()) || Number.isNaN(to.getTime()) || to < from) return 0;
    return Math.floor((to.getTime() - from.getTime()) / 86400000) + 1;
  }

  get today(): string { return new Date().toISOString().slice(0, 10); }

  submit(form: NgForm): void {
    if (form.invalid || this.numberOfDays < 1) { this.error = 'Please select valid employee, leave type, dates, and reason.'; return; }
    this.api.createLeaveRequest(this.model).subscribe({
      next: () => { this.message = 'Leave request submitted successfully.'; this.error = ''; form.resetForm({ employeeID: 0, leaveTypeID: 0, fromDate: '', toDate: '', reason: '' }); },
      error: err => this.error = err?.error?.message ?? 'Unable to submit leave request.'
    });
  }
}
