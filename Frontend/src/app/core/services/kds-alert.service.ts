import { Injectable, inject } from '@angular/core';
import { KdsTicket } from '../models/erp.models';

@Injectable({
  providedIn: 'root'
})
export class KdsAlertService {
  private audioCtx: AudioContext | null = null;
  private lastAlertAt = new Map<string, number>();
  private readonly repeatMs = 30000;

  playOverdueAlert(tickets: KdsTicket[]): void {
    const now = Date.now();
    const overdue = tickets.some(t => this.isOverdue(t));
    if (!overdue) return;

    const key = tickets.filter(t => this.isOverdue(t)).map(t => t.id).sort().join('|');
    const last = this.lastAlertAt.get(key) ?? 0;
    if (now - last < this.repeatMs) return;

    this.lastAlertAt.set(key, now);
    this.beep();
  }

  isOverdue(ticket: KdsTicket): boolean {
    const threshold = ticket.status === 'new' ? 300 : ticket.status === 'preparing' ? 600 : Number.MAX_SAFE_INTEGER;
    return ticket.timer > threshold;
  }

  private beep(): void {
    try {
      if (!this.audioCtx) {
        this.audioCtx = new AudioContext();
      }
      const ctx = this.audioCtx;
      const oscillator = ctx.createOscillator();
      const gain = ctx.createGain();
      oscillator.type = 'square';
      oscillator.frequency.value = 880;
      gain.gain.value = 0.08;
      oscillator.connect(gain);
      gain.connect(ctx.destination);
      oscillator.start();
      setTimeout(() => {
        oscillator.stop();
        oscillator.disconnect();
        gain.disconnect();
      }, 180);
    } catch {
      // Audio may be blocked until user interaction — ignore silently.
    }
  }
}
