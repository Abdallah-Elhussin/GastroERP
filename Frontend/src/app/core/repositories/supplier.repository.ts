import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  CreateSupplierPayload,
  SupplierDetail,
  SupplierListItem,
  UpsertSupplierMasterPayload
} from '../models/supplier.models';
import { AccountLookup } from '../models/opening-balance.models';
import { CostCenterLookup } from '../models/inventory-valuation-group.models';

@Injectable({ providedIn: 'root' })
export class SupplierRepository {
  private http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/inventory/suppliers`;

  getList(options: {
    page?: number;
    pageSize?: number;
    search?: string;
    isActive?: boolean | null;
    isBlacklisted?: boolean | null;
  } = {}): Observable<SupplierListItem[]> {
    let params = new HttpParams()
      .set('page', String(options.page ?? 1))
      .set('pageSize', String(options.pageSize ?? 100));
    if (options.search) params = params.set('search', options.search);
    if (options.isActive != null) params = params.set('isActive', String(options.isActive));
    if (options.isBlacklisted != null) params = params.set('isBlacklisted', String(options.isBlacklisted));
    return this.http.get<unknown>(this.base, { params }).pipe(
      map(r => unwrapSupplierList(r).map(normalizeSupplierListItem))
    );
  }

  getById(id: string, includeStats = false): Observable<SupplierDetail> {
    const params = new HttpParams().set('includeStats', String(includeStats));
    return this.http.get<SupplierDetail>(`${this.base}/${id}`, { params });
  }

  getNextCode(): Observable<string> {
    return this.http.get(`${this.base}/next-code`, { responseType: 'text' }).pipe(
      map(raw => {
        const value = String(raw ?? '')
          .trim()
          .replace(/^"|"$/g, '');
        // Reject template-like values (YYYY/MM) or non SUP+digits codes.
        return /^SUP\d{10}$/.test(value) ? value : '';
      })
    );
  }

  create(payload: CreateSupplierPayload): Observable<SupplierDetail> {
    return this.http.post<SupplierDetail>(this.base, {
      tenantId: '00000000-0000-0000-0000-000000000000',
      ...payload
    });
  }

  upsertMaster(id: string, payload: UpsertSupplierMasterPayload): Observable<SupplierDetail> {
    return this.http.put<SupplierDetail>(`${this.base}/${id}/master`, payload);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }

  activate(id: string): Observable<void> {
    return this.http.post<void>(`${this.base}/${id}/activate`, {});
  }

  deactivate(id: string): Observable<void> {
    return this.http.post<void>(`${this.base}/${id}/deactivate`, {});
  }

  blacklist(id: string, reason?: string | null): Observable<void> {
    return this.http.post<void>(`${this.base}/${id}/blacklist`, { reason: reason ?? null });
  }

  clearBlacklist(id: string): Observable<void> {
    return this.http.post<void>(`${this.base}/${id}/clear-blacklist`, {});
  }

  setDefaultPaymentMethod(supplierId: string, paymentMethodId: string): Observable<void> {
    return this.http.post<void>(`${this.base}/${supplierId}/payment-methods/${paymentMethodId}/default`, {});
  }

  removePaymentMethod(supplierId: string, paymentMethodId: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/${supplierId}/payment-methods/${paymentMethodId}`);
  }

  getAccounts(): Observable<AccountLookup[]> {
    const params = new HttpParams().set('page', 1).set('pageSize', 500);
    return this.http
      .get<AccountLookup[]>(`${environment.apiBaseUrl}/finance/accounts`, { params })
      .pipe(map(r => r ?? []));
  }

  getCostCenters(): Observable<CostCenterLookup[]> {
    const params = new HttpParams().set('page', 1).set('pageSize', 200);
    return this.http
      .get<CostCenterLookup[]>(`${environment.apiBaseUrl}/finance/cost-centers`, { params })
      .pipe(map(r => r ?? []));
  }
}

function unwrapSupplierList(raw: unknown): unknown[] {
  if (Array.isArray(raw)) return raw;
  if (raw && typeof raw === 'object') {
    const obj = raw as Record<string, unknown>;
    if (Array.isArray(obj['items'])) return obj['items'];
    if (Array.isArray(obj['data'])) return obj['data'];
  }
  return [];
}

function normalizeSupplierListItem(raw: unknown): SupplierListItem {
  const r = (raw ?? {}) as Record<string, unknown>;
  const pick = <T>(...keys: string[]): T | undefined => {
    for (const k of keys) {
      if (r[k] !== undefined && r[k] !== null) return r[k] as T;
    }
    return undefined;
  };

  return {
    id: String(pick<string>('id', 'Id') ?? ''),
    code: String(pick<string>('code', 'Code') ?? ''),
    nameAr: String(pick<string>('nameAr', 'NameAr') ?? ''),
    nameEn: pick<string | null>('nameEn', 'NameEn') ?? null,
    accountNumber: pick<string | null>('accountNumber', 'AccountNumber') ?? null,
    apAccountId: pick<string | null>('apAccountId', 'ApAccountId') ?? null,
    category: Number(pick<number>('category', 'Category') ?? 0),
    city: pick<string | null>('city', 'City') ?? null,
    country: pick<string | null>('country', 'Country') ?? null,
    taxNumber: pick<string | null>('taxNumber', 'TaxNumber') ?? null,
    creditLimit: Number(pick<number>('creditLimit', 'CreditLimit') ?? 0),
    currentBalance: Number(pick<number>('currentBalance', 'CurrentBalance') ?? 0),
    lastPurchaseDate: pick<string | null>('lastPurchaseDate', 'LastPurchaseDate') ?? null,
    lastPaymentDate: pick<string | null>('lastPaymentDate', 'LastPaymentDate') ?? null,
    isActive: pick<boolean>('isActive', 'IsActive') !== false,
    isBlacklisted: pick<boolean>('isBlacklisted', 'IsBlacklisted') === true,
    isOverCreditLimit: pick<boolean>('isOverCreditLimit', 'IsOverCreditLimit') === true
  };
}
