import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { environment } from '../../environments/environment';
import { AuthUser, LoginResponse, UserRole } from '../models/models';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly tokenKey = 'leave-management-token';
  private readonly userKey = 'leave-management-user';
  private readonly apiUrl = `${environment.apiUrl}/auth`;
  private readonly userSubject = new BehaviorSubject<AuthUser | null>(this.loadUser());
  readonly user$ = this.userSubject.asObservable();

  constructor(private readonly http: HttpClient) {}

  login(email: string, password: string): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/login`, { email, password }).pipe(
      tap(response => {
        localStorage.setItem(this.tokenKey, response.token);
        localStorage.setItem(this.userKey, JSON.stringify(response.user));
        this.userSubject.next(response.user);
      })
    );
  }

  logout(): void {
    localStorage.removeItem(this.tokenKey);
    localStorage.removeItem(this.userKey);
    this.userSubject.next(null);
  }

  get token(): string | null { return localStorage.getItem(this.tokenKey); }
  get currentUser(): AuthUser | null { return this.userSubject.value; }
  get isAuthenticated(): boolean { return !!this.token && !!this.currentUser; }
  hasRole(role: UserRole): boolean { return this.currentUser?.role === role; }

  redirectPathForRole(role: UserRole): string {
    return role === 'Manager' ? '/manager' : '/request-leave';
  }

  private loadUser(): AuthUser | null {
    const rawUser = localStorage.getItem(this.userKey);
    if (!rawUser) return null;
    try { return JSON.parse(rawUser) as AuthUser; } catch { return null; }
  }
}
