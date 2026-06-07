import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { LeaveType } from '../models/models';

@Injectable({ providedIn: 'root' })
export class LeaveTypeService {
  private readonly apiUrl = `${environment.apiUrl}/leavetypes`;

  constructor(private readonly http: HttpClient) {}

  getAll(): Observable<LeaveType[]> { return this.http.get<LeaveType[]>(this.apiUrl); }
  create(leaveType: LeaveType): Observable<LeaveType> { return this.http.post<LeaveType>(this.apiUrl, leaveType); }
  update(leaveType: LeaveType): Observable<void> { return this.http.put<void>(`${this.apiUrl}/${leaveType.leaveTypeID}`, leaveType); }
  delete(id: number): Observable<void> { return this.http.delete<void>(`${this.apiUrl}/${id}`); }
}
