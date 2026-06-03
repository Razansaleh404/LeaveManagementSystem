import { CommonModule, DatePipe } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { LeaveRequest } from '../../models/leave-request';
import { ApiService } from '../../services/api.service';

@Component({ selector: 'app-manager-approval', imports: [CommonModule, FormsModule, DatePipe], templateUrl: './manager-approval.component.html' })
export class ManagerApprovalComponent implements OnInit {
  requests: LeaveRequest[] = [];
  comments: Record<number, string> = {};
  message = '';
  error = '';
  constructor(private readonly api: ApiService) {}
  ngOnInit(): void { this.load(); }
  load(): void { this.api.getPendingLeaveRequests().subscribe({ next: r => this.requests = r, error: err => this.error = err?.error?.message ?? 'Unable to load pending requests.' }); }
  approve(request: LeaveRequest): void { this.decide(request, true); }
  reject(request: LeaveRequest): void { this.decide(request, false); }
  private decide(request: LeaveRequest, approved: boolean): void {
    const call = approved ? this.api.approveLeaveRequest(request.requestID, { managerComments: this.comments[request.requestID] }) : this.api.rejectLeaveRequest(request.requestID, { managerComments: this.comments[request.requestID] });
    call.subscribe({ next: () => { this.message = `Request ${approved ? 'approved' : 'rejected'} successfully.`; this.error = ''; this.load(); }, error: err => this.error = err?.error?.message ?? 'Unable to update request.' });
  }
}
