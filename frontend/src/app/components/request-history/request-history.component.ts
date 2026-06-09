import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { LeaveRequest, LeaveStatus } from '../../models/models';
import { LeaveRequestService } from '../../services/leave-request.service';

type RequestStatusFilter = 'All' | LeaveStatus | 'Closed';

@Component({ selector: 'app-request-history', standalone: true, imports: [CommonModule, FormsModule], templateUrl: './request-history.component.html' })
export class RequestHistoryComponent implements OnInit {
  requests: LeaveRequest[] = [];
  filteredRequests: LeaveRequest[] = [];
  status: RequestStatusFilter = 'All';
  fromDate = '';
  toDate = '';
  searchTerm = '';
  loading = false;
  error = '';

  readonly statusFilters: { label: RequestStatusFilter; helper: string }[] = [
    { label: 'All', helper: 'Every request' },
    { label: 'Pending', helper: 'Needs action' },
    { label: 'Closed', helper: 'Approved or rejected' },
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
    this.requestsApi.getHistory().subscribe({
      next: data => {
        this.requests = data;
        this.applyFilters();
        this.loading = false;
      },
      error: () => {
        this.error = 'Could not load request history.';
        this.loading = false;
      }
    });
  }

  applyFilters(): void {
    const term = this.searchTerm.trim().toLowerCase();
    this.filteredRequests = this.requests.filter(request => {
      const matchesStatus = this.status === 'All'
        || (this.status === 'Closed' ? request.status !== 'Pending' : request.status === this.status);
      const matchesFromDate = !this.fromDate || request.fromDate >= this.fromDate;
      const matchesToDate = !this.toDate || request.toDate <= this.toDate;
      const matchesSearch = !term || [
        request.employee?.firstName,
        request.employee?.lastName,
        request.employee?.department,
        request.leaveType?.leaveName,
        request.reason,
        request.managerComments
      ].some(value => value?.toLowerCase().includes(term));

      return matchesStatus && matchesFromDate && matchesToDate && matchesSearch;
    });
  }

  setStatus(status: RequestStatusFilter): void {
    this.status = status;
    this.applyFilters();
  }

  clear(): void {
    this.status = 'All';
    this.fromDate = '';
    this.toDate = '';
    this.searchTerm = '';
    this.applyFilters();
  }

  countFor(status: RequestStatusFilter): number {
    if (status === 'All') return this.requests.length;
    if (status === 'Closed') return this.closedCount;
    return this.requests.filter(request => request.status === status).length;
  }

  badge(status: string): string { return status === 'Approved' ? 'bg-success' : status === 'Rejected' ? 'bg-danger' : 'bg-warning text-dark'; }
}
