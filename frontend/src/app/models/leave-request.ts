export interface LeaveRequest {
  requestID: number;
  employeeID: number;
  employeeName: string;
  leaveTypeID: number;
  leaveTypeName: string;
  fromDate: string;
  toDate: string;
  numberOfDays: number;
  reason: string;
  status: 'Pending' | 'Approved' | 'Rejected';
  createdDate: string;
  managerComments?: string;
}

export interface LeaveRequestPayload {
  employeeID: number;
  leaveTypeID: number;
  fromDate: string;
  toDate: string;
  reason: string;
}

export interface LeaveDecisionPayload {
  managerComments?: string;
}
