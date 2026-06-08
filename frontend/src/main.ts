import { bootstrapApplication } from '@angular/platform-browser';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideRouter, Routes } from '@angular/router';
import { AppComponent } from './app/app.component';
import { EmployeeManagementComponent } from './app/components/employee-management/employee-management.component';
import { LeaveRequestComponent } from './app/components/leave-request/leave-request.component';
import { RequestHistoryComponent } from './app/components/request-history/request-history.component';
import { ManagerModuleComponent } from './app/components/manager-module/manager-module.component';
import { LoginComponent } from './app/components/login/login.component';
import { authGuard } from './app/services/auth.guard';
import { authInterceptor } from './app/services/auth.interceptor';

const routes: Routes = [
  { path: '', redirectTo: 'login', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { path: 'employees', component: EmployeeManagementComponent, canActivate: [authGuard], data: { roles: ['Admin'] } },
  { path: 'request-leave', component: LeaveRequestComponent, canActivate: [authGuard], data: { roles: ['Employee'] } },
  { path: 'history', component: RequestHistoryComponent, canActivate: [authGuard] },
  { path: 'manager', component: ManagerModuleComponent, canActivate: [authGuard], data: { roles: ['Manager', 'Admin'] } },
  { path: '**', redirectTo: 'login' }
];

bootstrapApplication(AppComponent, {
  providers: [provideHttpClient(withInterceptors([authInterceptor])), provideRouter(routes)]
}).catch(err => console.error(err));
