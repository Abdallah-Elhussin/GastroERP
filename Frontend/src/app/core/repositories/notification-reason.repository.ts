import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  NotificationReason,
  NotificationReasonListFilter,
  UpsertNotificationReasonPayload
} from '../models/notification-reason.models';

@Injectable({ providedIn: 'root' })
export class NotificationReasonRepository {
  private http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/finance/notification-reasons`;

  getList(options: NotificationReasonListFilter = {}): Observable<NotificationReason[]> {
    let params = new HttpParams()
      .set('page', '1')
      .set('pageSize', String(options.pageSize ?? 200));
    if (options.search?.trim()) params = params.set('search', options.search.trim());
    if (options.noteType != null) params = params.set('noteType', String(options.noteType));
    if (options.partyType != null) params = params.set('partyType', String(options.partyType));
    if (options.isActive != null) params = params.set('isActive', String(options.isActive));
    return this.http.get<NotificationReason[]>(this.base, { params }).pipe(map(r => r ?? []));
  }

  getById(id: string): Observable<NotificationReason> {
    return this.http.get<NotificationReason>(`${this.base}/${id}`);
  }

  create(payload: UpsertNotificationReasonPayload): Observable<NotificationReason> {
    return this.http.post<NotificationReason>(this.base, payload);
  }

  update(id: string, payload: UpsertNotificationReasonPayload): Observable<NotificationReason> {
    return this.http.put<NotificationReason>(`${this.base}/${id}`, payload);
  }

  delete(id: string): Observable<unknown> {
    return this.http.delete(`${this.base}/${id}`);
  }
}
