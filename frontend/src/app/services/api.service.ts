import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Employee, EmployeePayload } from '../models/employee';
import { LeaveRequest, LeaveRequestPayload, LeaveDecisionPayload } from '../models/leave-request';
import { LeaveType } from '../models/leave-type';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly baseUrl = 'https://localhost:7047/api';

  constructor(private readonly http: HttpClient) {}

  getEmployees(): Observable<Employee[]> { return this.http.get<Employee[]>(`${this.baseUrl}/employees`); }
  searchEmployees(term: string): Observable<Employee[]> { return this.http.get<Employee[]>(`${this.baseUrl}/employees/search`, { params: { term } }); }
  createEmployee(payload: EmployeePayload): Observable<Employee> { return this.http.post<Employee>(`${this.baseUrl}/employees`, payload); }
  updateEmployee(id: number, payload: EmployeePayload): Observable<Employee> { return this.http.put<Employee>(`${this.baseUrl}/employees/${id}`, payload); }
  deleteEmployee(id: number): Observable<void> { return this.http.delete<void>(`${this.baseUrl}/employees/${id}`); }

  getLeaveTypes(): Observable<LeaveType[]> { return this.http.get<LeaveType[]>(`${this.baseUrl}/leavetypes`); }

  getLeaveRequests(): Observable<LeaveRequest[]> { return this.http.get<LeaveRequest[]>(`${this.baseUrl}/leaverequests`); }
  getPendingLeaveRequests(): Observable<LeaveRequest[]> { return this.http.get<LeaveRequest[]>(`${this.baseUrl}/leaverequests/pending`); }
  filterLeaveRequests(status = '', fromDate = '', toDate = ''): Observable<LeaveRequest[]> {
    let params = new HttpParams();
    if (status) params = params.set('status', status);
    if (fromDate) params = params.set('fromDate', fromDate);
    if (toDate) params = params.set('toDate', toDate);
    return this.http.get<LeaveRequest[]>(`${this.baseUrl}/leaverequests/filter`, { params });
  }
  createLeaveRequest(payload: LeaveRequestPayload): Observable<LeaveRequest> { return this.http.post<LeaveRequest>(`${this.baseUrl}/leaverequests`, payload); }
  approveLeaveRequest(id: number, payload: LeaveDecisionPayload): Observable<LeaveRequest> { return this.http.put<LeaveRequest>(`${this.baseUrl}/leaverequests/${id}/approve`, payload); }
  rejectLeaveRequest(id: number, payload: LeaveDecisionPayload): Observable<LeaveRequest> { return this.http.put<LeaveRequest>(`${this.baseUrl}/leaverequests/${id}/reject`, payload); }
}
