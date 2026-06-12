import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { environment } from '../../environments/environment';
import { AuthManager, AuthUser, LoginResponse, RegisterRequest, UserRole } from '../models/models';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly tokenKey = 'leave-management-token';
  private readonly userKey = 'leave-management-user';
  private readonly expiresAtKey = 'leave-management-token-expires-at';
  private readonly apiUrl = `${environment.apiUrl}/auth`;
  private readonly userSubject = new BehaviorSubject<AuthUser | null>(this.loadUser());
  readonly user$ = this.userSubject.asObservable();

  constructor(private readonly http: HttpClient) {}

  login(email: string, password: string): Observable<LoginResponse> {
    return this.saveSession(this.http.post<LoginResponse>(`${this.apiUrl}/login`, { email, password }));
  }

  register(request: RegisterRequest): Observable<LoginResponse> {
    return this.saveSession(this.http.post<LoginResponse>(`${this.apiUrl}/register`, request));
  }

  getManagers(): Observable<AuthManager[]> { return this.http.get<AuthManager[]>(`${this.apiUrl}/managers`); }

  logout(): void {
    localStorage.removeItem(this.tokenKey);
    localStorage.removeItem(this.userKey);
    localStorage.removeItem(this.expiresAtKey);
    this.userSubject.next(null);
  }

  get token(): string | null {
    if (this.isTokenExpired()) {
      this.logout();
      return null;
    }

    return localStorage.getItem(this.tokenKey);
  }

  get currentUser(): AuthUser | null {
    if (this.isTokenExpired()) {
      this.logout();
      return null;
    }

    return this.userSubject.value;
  }

  get isAuthenticated(): boolean { return !!this.token && !!this.currentUser; }
  hasRole(role: UserRole): boolean { return this.currentUser?.role === role; }

  redirectPathForRole(role: UserRole): string {
    return role === 'Admin' ? '/admin' : role === 'Manager' ? '/manager' : '/request-leave';
  }

  private saveSession(request$: Observable<LoginResponse>): Observable<LoginResponse> {
    return request$.pipe(
      tap(response => {
        localStorage.setItem(this.tokenKey, response.token);
        localStorage.setItem(this.userKey, JSON.stringify(response.user));
        localStorage.setItem(this.expiresAtKey, response.expiresAt);
        this.userSubject.next(response.user);
      })
    );
  }

  private loadUser(): AuthUser | null {
    if (this.isTokenExpired()) {
      this.clearStoredSession();
      return null;
    }

    const rawUser = localStorage.getItem(this.userKey);
    if (!rawUser) return null;
    try { return JSON.parse(rawUser) as AuthUser; } catch { return null; }
  }

  private isTokenExpired(): boolean {
    const token = localStorage.getItem(this.tokenKey);
    const expiresAt = localStorage.getItem(this.expiresAtKey);
    if (!token || !expiresAt) return true;

    const expiresAtTime = Date.parse(expiresAt);
    return Number.isNaN(expiresAtTime) || expiresAtTime <= Date.now();
  }

  private clearStoredSession(): void {
    localStorage.removeItem(this.tokenKey);
    localStorage.removeItem(this.userKey);
    localStorage.removeItem(this.expiresAtKey);
  }
}
