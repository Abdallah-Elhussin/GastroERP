import { Injectable, signal, inject } from '@angular/core';
import { IndexedDbService } from './indexeddb.service';
import { PosRepository } from '../repositories/pos.repository';
import { KitchenRepository } from '../repositories/kitchen.repository';

@Injectable({
  providedIn: 'root'
})
export class OfflineSyncService {
  private dbService = inject(IndexedDbService);
  private posRepo = inject(PosRepository);
  private kitchenRepo = inject(KitchenRepository);

  isOnline = signal<boolean>(navigator.onLine);
  pendingOrdersCount = signal<number>(0);

  constructor() {
    this.initNetworkListeners();
    this.updatePendingCount();
  }

  private initNetworkListeners(): void {
    window.addEventListener('online', () => {
      this.isOnline.set(true);
      console.log('[Offline Sync]: Network restored. Triggering database synchronization...');
      this.syncOfflineData();
    });

    window.addEventListener('offline', () => {
      this.isOnline.set(false);
      console.warn('[Offline Sync]: Network connection lost. Falling back to local IndexedDB store.');
    });
  }

  private async updatePendingCount(): Promise<void> {
    const orders = await this.dbService.getOfflineOrders();
    this.pendingOrdersCount.set(orders.length);
  }

  async addOrderToSyncQueue(order: any): Promise<void> {
    await this.dbService.saveOfflineOrder(order);
    await this.updatePendingCount();
    
    if (this.isOnline()) {
      await this.syncOfflineData();
    }
  }

  async syncOfflineData(): Promise<void> {
    if (!this.isOnline()) return;

    try {
      const offlineOrders = await this.dbService.getOfflineOrders();
      if (offlineOrders.length === 0) return;

      console.log(`[Offline Sync]: Syncing ${offlineOrders.length} pending orders with server...`);

      for (const order of offlineOrders) {
        // Mock sending to backend REST controller
        this.posRepo.checkout(order.cartItems).subscribe({
          next: async () => {
            await this.dbService.deleteOfflineOrder(order.id);
            console.log(`[Offline Sync]: Unsent order ${order.id} synced successfully.`);
            await this.updatePendingCount();
          },
          error: (err) => {
            console.error(`[Offline Sync]: Failed to sync order ${order.id}. Retrying on next connection cycle.`, err);
          }
        });
      }
    } catch (error) {
      console.error('[Offline Sync]: Error reading offline cache:', error);
    }
  }
}
