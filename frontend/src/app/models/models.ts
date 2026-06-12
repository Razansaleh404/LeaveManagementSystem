export type UserRole = 'Employee' | 'Manager' | 'Admin';

export interface Employee {
  employeeID: number;
  firstName: string;
  lastName: string;
  email: string;
  department: string;
  role: UserRole;
  password?: string;
  isActive: boolean;
  managerID?: number | null;
}

export interface AuthUser {
  employeeID: number;
  firstName: string;
  lastName: string;
  email: string;
  department: string;
  role: UserRole;
  managerID?: number | null;
}

export interface LoginResponse {
  token: string;
  expiresAt: string;
  user: AuthUser;
}

export interface RegisterRequest {
  firstName: string;
  lastName: string;
  email: string;
  department: string;
  password: string;
  managerID: number;
}

export interface AuthManager {
  employeeID: number;
  firstName: string;
  lastName: string;
  department: string;
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
  managerID: number;
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
