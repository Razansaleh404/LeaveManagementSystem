import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { LeaveRequest, LeaveStatus } from '../../models/models';
import { LeaveRequestService } from '../../services/leave-request.service';

type ManagerStatusFilter = 'All' | LeaveStatus | 'Closed';

@Component({ selector: 'app-manager-module', standalone: true, imports: [CommonModule, FormsModule], templateUrl: './manager-module.component.html' })
export class ManagerModuleComponent implements OnInit {
  requests: LeaveRequest[] = [];
  filteredRequests: LeaveRequest[] = [];
  comments: Record<number, string> = {};
  status: ManagerStatusFilter = 'Pending';
  searchTerm = '';
  loading = false;
  message = '';
  error = '';

  readonly statusFilters: { label: ManagerStatusFilter; helper: string }[] = [
    { label: 'Pending', helper: 'Ready to review' },
    { label: 'Closed', helper: 'Completed decisions' },
    { label: 'All', helper: 'Every request' },
    { label: 'Approved', helper: 'Accepted leave' },
    { label: 'Rejected', helper: 'Declined leave' }
  ];

  constructor(private readonly requestsApi: LeaveRequestService) {}

  ngOnInit(): void { this.load(); }

  get pendingCount(): number { return this.requests.filter(request => request.status === 'Pending').length; }
  get approvedCount(): number { return this.requests.filter(request => request.status === 'Approved').length; }
  get rejectedCount(): number { return this.requests.filter(request => request.status === 'Rejected').length; }
  get closedCount(): number { return this.approvedCount + this.rejectedCount; }

  load(): void {
    this.loading = true;
    this.error = '';
    this.requestsApi.getAll().subscribe({
      next: data => {
        this.requests = data;
        this.applyFilters();
        this.loading = false;
      },
      error: () => {
        this.error = 'Could not load requests.';
        this.loading = false;
      }
    });
  }

  applyFilters(): void {
    const term = this.searchTerm.trim().toLowerCase();
    this.filteredRequests = this.requests.filter(request => {
      const matchesStatus = this.status === 'All'
        || (this.status === 'Closed' ? request.status !== 'Pending' : request.status === this.status);
      const matchesSearch = !term || [
        request.employee?.firstName,
        request.employee?.lastName,
        request.employee?.department,
        request.leaveType?.leaveName,
        request.reason,
        request.managerComments
      ].some(value => value?.toLowerCase().includes(term));

      return matchesStatus && matchesSearch;
    });
  }

  setStatus(status: ManagerStatusFilter): void {
    this.status = status;
    this.applyFilters();
  }

  approve(request: LeaveRequest): void { this.decide(request, true); }
  reject(request: LeaveRequest): void { this.decide(request, false); }

  badge(status: string): string { return status === 'Approved' ? 'bg-success' : status === 'Rejected' ? 'bg-danger' : 'bg-warning text-dark'; }

  countFor(status: ManagerStatusFilter): number {
    if (status === 'All') return this.requests.length;
    if (status === 'Closed') return this.closedCount;
    return this.requests.filter(request => request.status === status).length;
  }

  private decide(request: LeaveRequest, approve: boolean): void {
    const action = approve ? this.requestsApi.approve(request.requestID, this.comments[request.requestID] ?? '') : this.requestsApi.reject(request.requestID, this.comments[request.requestID] ?? '');
    action.subscribe({ next: () => { this.message = approve ? 'Request approved.' : 'Request rejected.'; this.error = ''; this.load(); }, error: () => this.error = 'Could not update request.' });
  }
}
