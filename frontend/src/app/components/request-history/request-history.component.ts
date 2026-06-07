import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { LeaveRequest, LeaveStatus } from '../../models/models';
import { LeaveRequestService } from '../../services/leave-request.service';

@Component({ selector: 'app-request-history', standalone: true, imports: [CommonModule, FormsModule], templateUrl: './request-history.component.html' })
export class RequestHistoryComponent implements OnInit {
  requests: LeaveRequest[] = [];
  status: LeaveStatus | '' = '';
  fromDate = '';
  toDate = '';
  loading = false;
  error = '';
  constructor(private readonly requestsApi: LeaveRequestService) {}
  ngOnInit(): void { this.load(); }
  load(): void { this.loading = true; this.requestsApi.getHistory().subscribe({ next: data => { this.requests = data; this.loading = false; }, error: () => { this.error = 'Could not load request history.'; this.loading = false; } }); }
  applyFilters(): void { this.loading = true; this.requestsApi.filter(this.status, this.fromDate, this.toDate).subscribe({ next: data => { this.requests = data; this.loading = false; }, error: () => { this.error = 'Could not filter requests.'; this.loading = false; } }); }
  clear(): void { this.status = ''; this.fromDate = ''; this.toDate = ''; this.load(); }
  badge(status: string): string { return status === 'Approved' ? 'bg-success' : status === 'Rejected' ? 'bg-danger' : 'bg-warning text-dark'; }
}
