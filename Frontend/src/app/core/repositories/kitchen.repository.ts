import { Observable } from 'rxjs';
import { KdsStation, KdsTicket } from '../models/erp.models';
import { DispatchPosKitchenPayload } from './rest-kitchen.repository';

export abstract class KitchenRepository {
  abstract getStations(): Observable<KdsStation[]>;
  abstract getBoard(stationId?: string): Observable<KdsTicket[]>;
  abstract dispatchFromPos(payload: DispatchPosKitchenPayload): Observable<KdsTicket[]>;
  abstract bumpTicket(ticketId: string, status: KdsTicket['status']): Observable<void>;
}
