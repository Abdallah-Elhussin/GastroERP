import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  AccountImportPreview,
  AccountImportRow,
  AccountTreeNode,
  AccountingSettings,
  ChartAccount,
  UpsertAccountPayload
} from '../models/chart-of-account.models';

@Injectable({ providedIn: 'root' })
export class ChartOfAccountRepository {
  private http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/finance/accounts`;
  private readonly settingsBase = `${environment.apiBaseUrl}/finance/accounting-settings`;

  getTree(options: {
    accountType?: number | null;
    includeInactive?: boolean;
    search?: string;
  } = {}): Observable<AccountTreeNode[]> {
    let params = new HttpParams().set('includeInactive', String(options.includeInactive ?? true));
    if (options.accountType != null) params = params.set('accountType', String(options.accountType));
    if (options.search?.trim()) params = params.set('search', options.search.trim());
    return this.http.get<AccountTreeNode[]>(`${this.base}/tree`, { params }).pipe(map(r => r ?? []));
  }

  getById(id: string): Observable<ChartAccount> {
    return this.http.get<ChartAccount>(`${this.base}/${id}`);
  }

  create(payload: UpsertAccountPayload & { accountNumber: string; accountType: number }): Observable<ChartAccount> {
    return this.http.post<ChartAccount>(this.base, payload);
  }

  update(id: string, payload: UpsertAccountPayload): Observable<unknown> {
    return this.http.put(`${this.base}/${id}`, {
      nameAr: payload.nameAr,
      nameEn: payload.nameEn,
      accountCategory: payload.accountCategory,
      isSummaryAccount: payload.isSummaryAccount,
      sortOrder: payload.sortOrder,
      currency: payload.currency,
      notes: payload.notes,
      accountClassificationId: payload.accountClassificationId
    });
  }

  reparent(id: string, newParentAccountId: string | null): Observable<unknown> {
    return this.http.post(`${this.base}/${id}/reparent`, { newParentAccountId });
  }

  renumber(id: string, newAccountNumber: string): Observable<unknown> {
    return this.http.post(`${this.base}/${id}/renumber`, { newAccountNumber });
  }

  activate(id: string): Observable<unknown> {
    return this.http.post(`${this.base}/${id}/activate`, {});
  }

  deactivate(id: string): Observable<unknown> {
    return this.http.post(`${this.base}/${id}/deactivate`, {});
  }

  delete(id: string): Observable<unknown> {
    return this.http.delete(`${this.base}/${id}`);
  }

  exportCsv(): Observable<Blob> {
    return this.http.get(`${this.base}/export`, { responseType: 'blob' });
  }

  downloadTemplate(): Observable<Blob> {
    return this.http.get(`${this.base}/excel-template`, { responseType: 'blob' });
  }

  import(rows: AccountImportRow[], commit: boolean): Observable<AccountImportPreview> {
    return this.http.post<AccountImportPreview>(`${this.base}/import`, { rows, commit });
  }

  getSettings(): Observable<AccountingSettings> {
    return this.http.get<AccountingSettings>(this.settingsBase);
  }

  saveSettings(payload: AccountingSettings): Observable<AccountingSettings> {
    return this.http.put<AccountingSettings>(this.settingsBase, payload);
  }
}
