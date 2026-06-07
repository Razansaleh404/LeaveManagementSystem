import { Routes } from '@angular/router';
import { EmployeesComponent } from './components/employees/employees.component';
import { LeaveRequestComponent } from './components/leave-request/leave-request.component';
import { RequestHistoryComponent } from './components/request-history/request-history.component';
import { ManagerApprovalComponent } from './components/manager-approval/manager-approval.component';

export const routes: Routes = [
  { path: '', redirectTo: 'employees', pathMatch: 'full' },
  { path: 'employees', component: EmployeesComponent },
  { path: 'leave-request', component: LeaveRequestComponent },
  { path: 'history', component: RequestHistoryComponent },
  { path: 'approval', component: ManagerApprovalComponent },
  { path: '**', redirectTo: 'employees' }
];
