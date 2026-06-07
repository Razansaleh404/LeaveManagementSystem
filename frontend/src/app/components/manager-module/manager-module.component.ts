import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { LeaveRequest } from '../../models/models';
import { LeaveRequestService } from '../../services/leave-request.service';

@Component({ selector: 'app-manager-module', standalone: true, imports: [CommonModule, FormsModule], templateUrl: './manager-module.component.html' })
export class ManagerModuleComponent implements OnInit {
  requests: LeaveRequest[] = [];
  comments: Record<number, string> = {};
  loading = false;
  message = '';
  error = '';
  constructor(private readonly requestsApi: LeaveRequestService) {}
  ngOnInit(): void { this.load(); }
  load(): void { this.loading = true; this.requestsApi.getPending().subscribe({ next: data => { this.requests = data; this.loading = false; }, error: () => { this.error = 'Could not load pending requests.'; this.loading = false; } }); }
  approve(request: LeaveRequest): void { this.decide(request, true); }
  reject(request: LeaveRequest): void { this.decide(request, false); }
  private decide(request: LeaveRequest, approve: boolean): void {
    const action = approve ? this.requestsApi.approve(request.requestID, this.comments[request.requestID] ?? '') : this.requestsApi.reject(request.requestID, this.comments[request.requestID] ?? '');
    action.subscribe({ next: () => { this.message = approve ? 'Request approved.' : 'Request rejected.'; this.error = ''; this.load(); }, error: () => this.error = 'Could not update request.' });
  }
}
