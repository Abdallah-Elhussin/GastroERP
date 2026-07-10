import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { HrRepository } from './hr.repository';

@Injectable({
  providedIn: 'root'
})
export class RestHrRepository extends HrRepository {
  private http = inject(HttpClient);
  private apiUrl = '/api/hr';

  getEmployees(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/employees`);
  }

  clockIn(employeeId: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/clock-in`, { employeeId });
  }
}
