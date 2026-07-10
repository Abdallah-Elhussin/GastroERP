import { Injectable, signal, OnDestroy, inject } from '@angular/core';
import { HubConnection, HubConnectionBuilder, HubConnectionState } from '@microsoft/signalr';
import { Subject } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthService } from './auth.service';

@Injectable({
  providedIn: 'root'
})
export class SignalrService implements OnDestroy {
  private authService = inject(AuthService);
  private hubConnection: HubConnection | null = null;
  private hubUrl = environment.apiBaseUrl.replace('/api/v1', '') + '/hubs/gastro';

  connectionState = signal<'disconnected' | 'connecting' | 'connected'>('disconnected');
  onlineUsers = signal<string[]>([]);

  orderCreated$ = new Subject<any>();
  ticketBumped$ = new Subject<{ ticketId: string; status: string; ticket?: any }>();
  inventoryUpdated$ = new Subject<{ itemId: string; stockLevel: string; status: string }>();
  brandingUpdated$ = new Subject<any>();

  constructor() {
    this.initSignalR();
  }

  private initSignalR(): void {
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(this.hubUrl, {
        accessTokenFactory: () => this.authService.getToken() ?? ''
      })
      .withAutomaticReconnect()
      .build();

    this.connectionState.set('connecting');

    this.hubConnection.start()
      .then(() => {
        this.connectionState.set('connected');
        this.registerHandlers();
      })
      .catch(() => {
        this.connectionState.set('disconnected');
      });

    this.hubConnection.onreconnecting(() => this.connectionState.set('connecting'));
    this.hubConnection.onreconnected(() => this.connectionState.set('connected'));
    this.hubConnection.onclose(() => this.connectionState.set('disconnected'));
  }

  private registerHandlers(): void {
    if (!this.hubConnection) return;

    this.hubConnection.on('OrderCreated', (order) => this.orderCreated$.next(order));
    this.hubConnection.on('TicketBumped', (data) => this.ticketBumped$.next(data));
    this.hubConnection.on('InventoryUpdated', (data) => this.inventoryUpdated$.next(data));
    this.hubConnection.on('BrandingUpdated', (data) => this.brandingUpdated$.next(data));
    this.hubConnection.on('UserPresenceChanged', (users: string[]) => this.onlineUsers.set(users));
  }

  simulateEvent(type: 'order' | 'bump' | 'inventory' | 'branding', payload: any): void {
    switch (type) {
      case 'order':
        this.orderCreated$.next(payload);
        break;
      case 'bump':
        this.ticketBumped$.next(payload);
        break;
      case 'inventory':
        this.inventoryUpdated$.next(payload);
        break;
      case 'branding':
        this.brandingUpdated$.next(payload);
        break;
    }
  }

  ngOnDestroy(): void {
    this.hubConnection?.stop();
  }
}
