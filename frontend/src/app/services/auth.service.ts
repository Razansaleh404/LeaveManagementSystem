import { HttpClient } from '@angular/common/http';
import { Injectable, computed, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { environment } from '../../environments/environment';
import { AuthUser, LoginRequest, LoginResponse, Role } from '../models/models';

const TOKEN_KEY = 'leave_management_token';
const USER_KEY = 'leave_management_user';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly apiUrl = `${environment.apiUrl}/auth`;
  private readonly userSignal = signal<AuthUser | null>(this.loadStoredUser());

  readonly currentUser = this.userSignal.asReadonly();
  readonly isAuthenticated = computed(() => !!this.token && !!this.userSignal());

  constructor(private readonly http: HttpClient) {}

  get token(): string | null { return localStorage.getItem(TOKEN_KEY); }
  get user(): AuthUser | null { return this.userSignal(); }

  login(credentials: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/login`, credentials).pipe(
      tap(response => this.storeSession(response))
    );
  }

  logout(): void {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
    this.userSignal.set(null);
  }

  hasRole(roles: Role[]): boolean {
    const role = this.userSignal()?.role;
    return !!role && roles.includes(role);
  }

  private storeSession(response: LoginResponse): void {
    const user: AuthUser = {
      employeeID: response.employeeID,
      email: response.email,
      fullName: response.fullName,
      role: response.role
    };
    localStorage.setItem(TOKEN_KEY, response.token);
    localStorage.setItem(USER_KEY, JSON.stringify(user));
    this.userSignal.set(user);
  }

  private loadStoredUser(): AuthUser | null {
    const raw = localStorage.getItem(USER_KEY);
    if (!raw || !localStorage.getItem(TOKEN_KEY)) return null;
    try { return JSON.parse(raw) as AuthUser; } catch { return null; }
  }
}
