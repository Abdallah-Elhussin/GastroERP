import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { KitchenRepository } from './kitchen.repository';
import { KdsStation, KdsTicket } from '../models/erp.models';

export interface DispatchPosKitchenPayload {
  orderReference: string;
  tableLabel: string;
  orderType: string;
  items: Array<{
    name: string;
    quantity: number;
    notes?: string;
    categoryKey?: string;
  }>;
}

interface ApiKdsTicketView {
  id: string;
  ticketNumber: string;
  tableLabel: string;
  orderType: string;
  kitchenStationId: string;
  stationType: number;
  stationNameAr: string;
  stationNameEn?: string;
  kdsStatus: string;
  timerSeconds: number;
  createdAt: string;
  items: Array<{
    id: string;
    name: string;
    quantity: number;
    notes: string[];
  }>;
}

interface ApiKitchenStation {
  id: string;
  branchId: string;
  nameAr: string;
  nameEn?: string;
  stationType: number;
  isActive: boolean;
  sortOrder: number;
}

@Injectable({
  providedIn: 'root'
})
export class RestKitchenRepository extends KitchenRepository {
  private http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/sales/kitchen`;

  getStations(): Observable<KdsStation[]> {
    return this.http.get<ApiKitchenStation[]>(`${this.base}/stations`).pipe(
      map(rows => rows.map(mapApiStation))
    );
  }

  getBoard(stationId?: string): Observable<KdsTicket[]> {
    const params = stationId ? `?stationId=${stationId}` : '';
    return this.http.get<ApiKdsTicketView[]>(`${this.base}/board${params}`).pipe(
      map(rows => rows.map(mapApiTicket))
    );
  }

  dispatchFromPos(payload: DispatchPosKitchenPayload): Observable<KdsTicket[]> {
    return this.http.post<ApiKdsTicketView[]>(`${this.base}/dispatch`, payload).pipe(
      map(rows => rows.map(mapApiTicket))
    );
  }

  bumpTicket(ticketId: string, status: KdsTicket['status']): Observable<void> {
    const action = status === 'new'
      ? 'start'
      : status === 'preparing'
        ? 'ready'
        : 'complete';
    return this.http.patch<void>(`${this.base}/tickets/${ticketId}/${action}`, {});
  }
}

export function mapApiTicket(raw: ApiKdsTicketView): KdsTicket {
  return {
    id: raw.id,
    ticketNumber: raw.ticketNumber,
    tableNo: raw.tableLabel,
    timer: raw.timerSeconds,
    status: raw.kdsStatus as KdsTicket['status'],
    orderTime: new Date(raw.createdAt),
    stationId: raw.kitchenStationId,
    stationType: raw.stationType,
    stationNameAr: raw.stationNameAr,
    stationNameEn: raw.stationNameEn,
    items: raw.items.map(i => ({
      name: i.name,
      quantity: Number(i.quantity),
      notes: i.notes ?? []
    }))
  };
}

export function mapApiStation(raw: ApiKitchenStation): KdsStation {
  return {
    id: raw.id,
    nameAr: raw.nameAr,
    nameEn: raw.nameEn,
    stationType: raw.stationType,
    sortOrder: raw.sortOrder
  };
}
