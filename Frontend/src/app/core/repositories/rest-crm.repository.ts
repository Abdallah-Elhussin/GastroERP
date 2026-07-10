import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CrmRepository } from './crm.repository';

@Injectable({
  providedIn: 'root'
})
export class RestCrmRepository extends CrmRepository {
  private http = inject(HttpClient);
  private apiUrl = '/api/crm';

  getCustomers(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/customers`);
  }
}
