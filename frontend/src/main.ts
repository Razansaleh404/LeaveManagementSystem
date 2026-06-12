import { bootstrapApplication } from '@angular/platform-browser';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideRouter, Routes } from '@angular/router';
import { AppComponent } from './app/app.component';
import { EmployeeManagementComponent } from './app/components/employee-management/employee-management.component';
import { LeaveRequestComponent } from './app/components/leave-request/leave-request.component';
import { RequestHistoryComponent } from './app/components/request-history/request-history.component';
import { ManagerModuleComponent } from './app/components/manager-module/manager-module.component';
import { LoginComponent } from './app/components/login/login.component';
import { SignupComponent } from './app/components/signup/signup.component';
import { authGuard, loginGuard, roleGuard, roleHomeGuard } from './app/services/auth.guard';
import { authInterceptor } from './app/services/auth.interceptor';

const routes: Routes = [
  { path: '', pathMatch: 'full', canActivate: [roleHomeGuard], component: LoginComponent },
  { path: 'login', component: LoginComponent, canActivate: [loginGuard] },
  { path: 'signup', component: SignupComponent, canActivate: [loginGuard] },
  { path: 'admin', component: EmployeeManagementComponent, canActivate: [authGuard, roleGuard(['Admin'])] },
  { path: 'employees', redirectTo: 'admin' },
  { path: 'request-leave', component: LeaveRequestComponent, canActivate: [authGuard, roleGuard(['Employee'])] },
  { path: 'history', component: RequestHistoryComponent, canActivate: [authGuard, roleGuard(['Employee'])] },
  { path: 'manager', component: ManagerModuleComponent, canActivate: [authGuard, roleGuard(['Manager'])] },
  { path: '**', redirectTo: 'login' }
];

bootstrapApplication(AppComponent, {
  providers: [provideHttpClient(withInterceptors([authInterceptor])), provideRouter(routes)]
}).catch(err => console.error(err));
