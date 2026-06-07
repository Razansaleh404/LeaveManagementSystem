import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { LeaveRequest, LeaveRequestCreate, LeaveStatus } from '../models/models';

@Injectable({ providedIn: 'root' })
export class LeaveRequestService {
  private readonly apiUrl = `${environment.apiUrl}/leaverequests`;

  constructor(private readonly http: HttpClient) {}

  getAll(): Observable<LeaveRequest[]> { return this.http.get<LeaveRequest[]>(this.apiUrl); }
  getHistory(): Observable<LeaveRequest[]> { return this.http.get<LeaveRequest[]>(`${this.apiUrl}/history`); }
  getPending(): Observable<LeaveRequest[]> { return this.http.get<LeaveRequest[]>(`${this.apiUrl}/pending`); }
  create(request: LeaveRequestCreate): Observable<LeaveRequest> { return this.http.post<LeaveRequest>(this.apiUrl, request); }
  delete(id: number): Observable<void> { return this.http.delete<void>(`${this.apiUrl}/${id}`); }
  approve(id: number, managerComments: string): Observable<void> { return this.http.put<void>(`${this.apiUrl}/${id}/approve`, { managerComments }); }
  reject(id: number, managerComments: string): Observable<void> { return this.http.put<void>(`${this.apiUrl}/${id}/reject`, { managerComments }); }

  filter(status?: LeaveStatus | '', fromDate?: string, toDate?: string): Observable<LeaveRequest[]> {
    let params = new HttpParams();
    if (status) params = params.set('status', status);
    if (fromDate) params = params.set('fromDate', fromDate);
    if (toDate) params = params.set('toDate', toDate);
    return this.http.get<LeaveRequest[]>(`${this.apiUrl}/filter`, { params });
  }
}
