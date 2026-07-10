import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { FinanceRepository } from './finance.repository';

@Injectable({
  providedIn: 'root'
})
export class RestFinanceRepository extends FinanceRepository {
  private http = inject(HttpClient);
  private apiUrl = '/api/finance';

  getLedgerEntries(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/ledger`);
  }
}
