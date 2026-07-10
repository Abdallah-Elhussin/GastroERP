import { Injectable, signal, inject, DestroyRef, computed } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { interval, switchMap, catchError, of, tap } from 'rxjs';
import { KitchenRepository } from '../repositories/kitchen.repository';
import { SignalrService } from './signalr.service';
import { KdsTicket } from '../models/erp.models';
import { mapApiTicket } from '../repositories/rest-kitchen.repository';

@Injectable({
  providedIn: 'root'
})
export class KdsService {
  private kitchenRepo = inject(KitchenRepository);
  private signalr = inject(SignalrService);
  private destroyRef = inject(DestroyRef);

  tickets = signal<KdsTicket[]>([]);
  isLoading = signal(false);
  lastSyncAt = signal<Date | null>(null);

  newTickets = computed(() => this.tickets().filter(t => t.status === 'new'));
  preparingTickets = computed(() => this.tickets().filter(t => t.status === 'preparing'));
  readyTickets = computed(() => this.tickets().filter(t => t.status === 'ready'));

  constructor() {
    this.loadBoard();
    this.bindRealtime();

    interval(15000).pipe(
      takeUntilDestroyed(this.destroyRef),
      switchMap(() => this.kitchenRepo.getBoard()),
      catchError(() => of([]))
    ).subscribe(tickets => {
      this.tickets.set(this.mergeTimers(tickets));
      this.lastSyncAt.set(new Date());
    });
  }

  loadBoard(): void {
    this.isLoading.set(true);
    this.kitchenRepo.getBoard().pipe(
      catchError(() => of([]))
    ).subscribe({
      next: tickets => {
        this.tickets.set(this.mergeTimers(tickets));
        this.lastSyncAt.set(new Date());
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  bumpTicket(ticket: KdsTicket): void {
    this.kitchenRepo.bumpTicket(ticket.id, ticket.status).pipe(
      catchError(() => of(void 0))
    ).subscribe(() => {
      if (ticket.status === 'ready') {
        this.tickets.update(list => list.filter(t => t.id !== ticket.id));
      } else {
        const nextStatus = ticket.status === 'new' ? 'preparing' : 'ready';
        this.tickets.update(list => list.map(t =>
          t.id === ticket.id ? { ...t, status: nextStatus, timer: 0 } : t
        ));
      }
    });
  }

  private bindRealtime(): void {
    this.signalr.orderCreated$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(raw => {
      const ticket = this.normalizeIncoming(raw);
      if (!ticket) return;
      this.tickets.update(list => list.some(t => t.id === ticket.id) ? list : [ticket, ...list]);
    });

    this.signalr.ticketBumped$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(data => {
      if (data.status === 'completed') {
        this.tickets.update(list => list.filter(t => t.id !== data.ticketId));
        return;
      }

      if (data.ticket) {
        const ticket = this.normalizeIncoming(data.ticket);
        if (!ticket) return;
        this.tickets.update(list => {
          const idx = list.findIndex(t => t.id === ticket.id);
          if (idx === -1) return [ticket, ...list];
          const copy = [...list];
          copy[idx] = ticket;
          return copy;
        });
        return;
      }

      this.tickets.update(list => list.map(t => {
        if (t.id !== data.ticketId) return t;
        const next = data.status === 'preparing' ? 'preparing' : data.status === 'ready' ? 'ready' : t.status;
        return { ...t, status: next as KdsTicket['status'], timer: next === 'preparing' ? 0 : t.timer };
      }));
    });

    interval(1000).pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => {
      this.tickets.update(list => list.map(t => ({ ...t, timer: t.timer + 1 })));
    });
  }

  private mergeTimers(incoming: KdsTicket[]): KdsTicket[] {
    const existing = new Map(this.tickets().map(t => [t.id, t.timer]));
    return incoming.map(t => ({ ...t, timer: existing.get(t.id) ?? t.timer }));
  }

  private normalizeIncoming(raw: unknown): KdsTicket | null {
    if (!raw || typeof raw !== 'object') return null;
    const obj = raw as Record<string, unknown>;
    if (obj['kdsStatus']) return mapApiTicket(raw as Parameters<typeof mapApiTicket>[0]);
    return {
      id: String(obj['id'] ?? ''),
      ticketNumber: String(obj['ticketNumber'] ?? obj['id'] ?? ''),
      tableNo: String(obj['tableNo'] ?? obj['tableLabel'] ?? '—'),
      timer: Number(obj['timer'] ?? obj['timerSeconds'] ?? 0),
      status: (obj['status'] ?? obj['kdsStatus'] ?? 'new') as KdsTicket['status'],
      orderTime: obj['orderTime'] ? new Date(String(obj['orderTime'])) : new Date(),
      stationId: String(obj['stationId'] ?? obj['kitchenStationId'] ?? ''),
      stationType: Number(obj['stationType'] ?? 99),
      items: Array.isArray(obj['items']) ? (obj['items'] as KdsTicket['items']) : []
    };
  }
}
