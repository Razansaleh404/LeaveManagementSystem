import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class DepartmentService {
  private readonly apiUrl = `${environment.apiUrl}/departments`;
  constructor(private readonly http: HttpClient) {}
  getAll(): Observable<string[]> { return this.http.get<string[]>(this.apiUrl); }
}
