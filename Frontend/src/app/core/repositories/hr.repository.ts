import { Observable } from 'rxjs';

export abstract class HrRepository {
  abstract getEmployees(): Observable<any[]>;
  abstract clockIn(employeeId: string): Observable<void>;
}
