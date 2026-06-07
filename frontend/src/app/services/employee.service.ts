import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Employee } from '../models/models';

@Injectable({ providedIn: 'root' })
export class EmployeeService {
  private readonly apiUrl = `${environment.apiUrl}/employees`;

  constructor(private readonly http: HttpClient) {}

  getAll(): Observable<Employee[]> { return this.http.get<Employee[]>(this.apiUrl); }
  get(id: number): Observable<Employee> { return this.http.get<Employee>(`${this.apiUrl}/${id}`); }
  search(term: string): Observable<Employee[]> { return this.http.get<Employee[]>(`${this.apiUrl}/search`, { params: { term } }); }
  create(employee: Employee): Observable<Employee> { return this.http.post<Employee>(this.apiUrl, employee); }
  update(employee: Employee): Observable<void> { return this.http.put<void>(`${this.apiUrl}/${employee.employeeID}`, employee); }
  delete(id: number): Observable<void> { return this.http.delete<void>(`${this.apiUrl}/${id}`); }
}
