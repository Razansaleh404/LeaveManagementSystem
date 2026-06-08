import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { Employee, UserRole } from '../models/models';

export interface AuthSession {
  role: UserRole;
  employee?: Employee;
  displayName: string;
}

const SESSION_STORAGE_KEY = 'leave-management-session';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly sessionSubject = new BehaviorSubject<AuthSession | null>(this.loadSession());
  readonly session$ = this.sessionSubject.asObservable();

  get session(): AuthSession | null { return this.sessionSubject.value; }
  get isLoggedIn(): boolean { return this.session !== null; }
  get role(): UserRole | null { return this.session?.role ?? null; }
  get employeeId(): number | null { return this.session?.employee?.employeeID ?? null; }

  loginAsEmployee(employee: Employee): void {
    this.setSession({ role: 'Employee', employee, displayName: `${employee.firstName} ${employee.lastName}` });
  }

  loginAsRole(role: Exclude<UserRole, 'Employee'>): void {
    const displayName = role === 'Manager' ? 'Department Manager' : 'System Administrator';
    this.setSession({ role, displayName });
  }

  logout(): void {
    localStorage.removeItem(SESSION_STORAGE_KEY);
    this.sessionSubject.next(null);
  }

  canAccess(allowedRoles?: UserRole[]): boolean {
    if (!this.session) return false;
    return !allowedRoles || allowedRoles.includes(this.session.role);
  }

  private setSession(session: AuthSession): void {
    localStorage.setItem(SESSION_STORAGE_KEY, JSON.stringify(session));
    this.sessionSubject.next(session);
  }

  private loadSession(): AuthSession | null {
    const storedSession = localStorage.getItem(SESSION_STORAGE_KEY);
    if (!storedSession) return null;
    return JSON.parse(storedSession) as AuthSession;
  }
}
