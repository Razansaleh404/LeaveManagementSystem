import { CommonModule, DatePipe } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { LeaveRequest } from '../../models/leave-request';
import { ApiService } from '../../services/api.service';

@Component({ selector: 'app-request-history', imports: [CommonModule, FormsModule, DatePipe], templateUrl: './request-history.component.html' })
export class RequestHistoryComponent implements OnInit {
  requests: LeaveRequest[] = [];
  status = '';
  fromDate = '';
  toDate = '';
  error = '';
  constructor(private readonly api: ApiService) {}
  ngOnInit(): void { this.load(); }
  load(): void { this.api.filterLeaveRequests(this.status, this.fromDate, this.toDate).subscribe({ next: r => this.requests = r, error: err => this.error = err?.error?.message ?? 'Unable to load requests.' }); }
  clear(): void { this.status = ''; this.fromDate = ''; this.toDate = ''; this.load(); }
  badge(status: string): string { return status === 'Approved' ? 'text-bg-success' : status === 'Rejected' ? 'text-bg-danger' : 'text-bg-warning'; }
}
