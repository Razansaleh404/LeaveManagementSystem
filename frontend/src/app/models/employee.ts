export interface Employee {
  employeeID: number;
  employeeCode: string;
  fullName: string;
  email: string;
  department: string;
  isActive: boolean;
}

export type EmployeePayload = Omit<Employee, 'employeeID'>;
