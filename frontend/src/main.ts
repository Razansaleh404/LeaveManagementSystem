import { bootstrapApplication } from '@angular/platform-browser';
import { provideHttpClient } from '@angular/common/http';
import { provideRouter, Routes } from '@angular/router';
import { AppComponent } from './app/app.component';
import { EmployeeManagementComponent } from './app/components/employee-management/employee-management.component';
import { LeaveRequestComponent } from './app/components/leave-request/leave-request.component';
import { RequestHistoryComponent } from './app/components/request-history/request-history.component';
import { ManagerModuleComponent } from './app/components/manager-module/manager-module.component';

const routes: Routes = [
  { path: '', redirectTo: 'employees', pathMatch: 'full' },
  { path: 'employees', component: EmployeeManagementComponent },
  { path: 'request-leave', component: LeaveRequestComponent },
  { path: 'history', component: RequestHistoryComponent },
  { path: 'manager', component: ManagerModuleComponent },
  { path: '**', redirectTo: 'employees' }
];

bootstrapApplication(AppComponent, {
  providers: [provideHttpClient(), provideRouter(routes)]
}).catch(err => console.error(err));
