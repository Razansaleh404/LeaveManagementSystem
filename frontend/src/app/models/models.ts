export type UserRole = 'Employee' | 'Manager' | 'Admin';

export const DEPARTMENTS = ['Human Resources', 'Finance', 'Engineering', 'Sales', 'Marketing', 'Operations', 'Customer Support'] as const;

export interface Employee {
  employeeID: number;
  firstName: string;
  lastName: string;
  email: string;
  department: string;
  isActive: boolean;
}

export interface LeaveType {
  leaveTypeID: number;
  leaveName: string;
}

export type LeaveStatus = 'Pending' | 'Approved' | 'Rejected';

export interface LeaveRequest {
  requestID: number;
  employeeID: number;
  leaveTypeID: number;
  fromDate: string;
  toDate: string;
  numberOfDays: number;
  reason: string;
  status: LeaveStatus;
  managerComments?: string;
  createdDate: string;
  employee?: Employee;
  leaveType?: LeaveType;
}

export interface LeaveRequestCreate {
  employeeID: number;
  leaveTypeID: number;
  fromDate: string;
  toDate: string;
  reason: string;
}
