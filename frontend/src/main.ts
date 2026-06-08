import { bootstrapApplication } from '@angular/platform-browser';
import { provideHttpClient } from '@angular/common/http';
import { provideRouter, Routes } from '@angular/router';
import { AppComponent } from './app/app.component';
import { EmployeeManagementComponent } from './app/components/employee-management/employee-management.component';
import { LeaveRequestComponent } from './app/components/leave-request/leave-request.component';
import { RequestHistoryComponent } from './app/components/request-history/request-history.component';
import { ManagerModuleComponent } from './app/components/manager-module/manager-module.component';
import { LoginComponent } from './app/components/login/login.component';
import { roleGuard } from './app/services/auth.guard';

const routes: Routes = [
  { path: '', redirectTo: 'login', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { path: 'employees', component: EmployeeManagementComponent, canActivate: [roleGuard], data: { roles: ['Admin'] } },
  { path: 'request-leave', component: LeaveRequestComponent, canActivate: [roleGuard], data: { roles: ['Employee'] } },
  { path: 'history', component: RequestHistoryComponent, canActivate: [roleGuard], data: { roles: ['Employee', 'Manager', 'Admin'] } },
  { path: 'manager', component: ManagerModuleComponent, canActivate: [roleGuard], data: { roles: ['Manager', 'Admin'] } },
  { path: '**', redirectTo: 'login' }
];

bootstrapApplication(AppComponent, {
  providers: [provideHttpClient(), provideRouter(routes)]
}).catch(err => console.error(err));
