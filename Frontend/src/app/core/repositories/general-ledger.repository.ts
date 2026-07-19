import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { GeneralLedgerFilter, GeneralLedgerResult } from '../models/general-ledger.models';

@Injectable({ providedIn: 'root' })
export class GeneralLedgerRepository {
  private http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/finance/reports/general-ledger`;

  inquire(filter: GeneralLedgerFilter = {}): Observable<GeneralLedgerResult> {
    let params = new HttpParams()
      .set('page', String(filter.page ?? 1))
      .set('pageSize', String(filter.pageSize ?? 50))
      .set('includeOpeningBalance', String(filter.includeOpeningBalance !== false));

    if (filter.accountId) params = params.set('accountId', filter.accountId);
    if (filter.companyId) params = params.set('companyId', filter.companyId);
    if (filter.branchId) params = params.set('branchId', filter.branchId);
    if (filter.fiscalPeriodId) params = params.set('fiscalPeriodId', filter.fiscalPeriodId);
    if (filter.fiscalYear != null) params = params.set('fiscalYear', String(filter.fiscalYear));
    if (filter.fromDate) params = params.set('fromDate', filter.fromDate);
    if (filter.toDate) params = params.set('toDate', filter.toDate);
    if (filter.costCenterId) params = params.set('costCenterId', filter.costCenterId);
    if (filter.parentAccountId) params = params.set('parentAccountId', filter.parentAccountId);
    if (filter.accountType != null) params = params.set('accountType', String(filter.accountType));
    if (filter.currency) params = params.set('currency', filter.currency);
    if (filter.sourceModule != null) params = params.set('sourceModule', String(filter.sourceModule));
    if (filter.documentNumber) params = params.set('documentNumber', filter.documentNumber);
    if (filter.search) params = params.set('search', filter.search);

    return this.http.get<GeneralLedgerResult>(this.base, { params }).pipe(
      map(r => ({
        openingBalance: r?.openingBalance ?? 0,
        totalDebit: r?.totalDebit ?? 0,
        totalCredit: r?.totalCredit ?? 0,
        closingBalance: r?.closingBalance ?? 0,
        totalCount: r?.totalCount ?? 0,
        page: r?.page ?? 1,
        pageSize: r?.pageSize ?? 50,
        lines: r?.lines ?? []
      }))
    );
  }
}
