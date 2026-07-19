import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CrmRepository } from './crm.repository';

export interface CrmCustomerSummary {
  id: string;
  customerNumber: string;
  fullName: string;
  mobile: string;
  email?: string | null;
  taxNumber?: string | null;
  currency?: string;
  paymentDueDays?: number;
  paymentTerms?: string | null;
  creditLimit?: number;
  status?: number | string;
}

@Injectable({
  providedIn: 'root'
})
export class RestCrmRepository extends CrmRepository {
  private http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/crm/customers`;

  getCustomers(page = 1, pageSize = 200, search?: string): Observable<CrmCustomerSummary[]> {
    let params = new HttpParams().set('page', page).set('pageSize', pageSize);
    if (search?.trim()) params = params.set('search', search.trim());
    return this.http.get<CrmCustomerSummary[]>(this.base, { params }).pipe(
      map(rows => (Array.isArray(rows) ? rows : []))
    );
  }
}
