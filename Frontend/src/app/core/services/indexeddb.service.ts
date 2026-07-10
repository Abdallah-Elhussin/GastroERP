import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class IndexedDbService {
  private dbName = 'gastro_db';
  private dbVersion = 1;
  private db: IDBDatabase | null = null;

  constructor() {
    this.openDatabase();
  }

  private openDatabase(): Promise<IDBDatabase> {
    return new Promise((resolve, reject) => {
      const request = indexedDB.open(this.dbName, this.dbVersion);

      request.onupgradeneeded = (event) => {
        const db = (event.target as IDBOpenDBRequest).result;
        if (!db.objectStoreNames.contains('orders')) {
          db.createObjectStore('orders', { keyPath: 'id' });
        }
        if (!db.objectStoreNames.contains('kds_tickets')) {
          db.createObjectStore('kds_tickets', { keyPath: 'id' });
        }
      };

      request.onsuccess = (event) => {
        this.db = (event.target as IDBOpenDBRequest).result;
        resolve(this.db);
      };

      request.onerror = (event) => {
        reject('Error opening IndexedDB database: ' + (event.target as IDBOpenDBRequest).error);
      };
    });
  }

  private getStore(storeName: 'orders' | 'kds_tickets', mode: IDBTransactionMode): Promise<IDBObjectStore> {
    return new Promise((resolve, reject) => {
      if (this.db) {
        const transaction = this.db.transaction(storeName, mode);
        resolve(transaction.objectStore(storeName));
      } else {
        this.openDatabase().then((db) => {
          const transaction = db.transaction(storeName, mode);
          resolve(transaction.objectStore(storeName));
        }).catch(reject);
      }
    });
  }

  saveOfflineOrder(order: any): Promise<void> {
    return this.getStore('orders', 'readwrite').then(store => {
      return new Promise<void>((resolve, reject) => {
        const request = store.put(order);
        request.onsuccess = () => resolve();
        request.onerror = () => reject(request.error);
      });
    });
  }

  getOfflineOrders(): Promise<any[]> {
    return this.getStore('orders', 'readonly').then(store => {
      return new Promise<any[]>((resolve, reject) => {
        const request = store.getAll();
        request.onsuccess = () => resolve(request.result);
        request.onerror = () => reject(request.error);
      });
    });
  }

  deleteOfflineOrder(orderId: string): Promise<void> {
    return this.getStore('orders', 'readwrite').then(store => {
      return new Promise<void>((resolve, reject) => {
        const request = store.delete(orderId);
        request.onsuccess = () => resolve();
        request.onerror = () => reject(request.error);
      });
    });
  }

  clearOfflineOrders(): Promise<void> {
    return this.getStore('orders', 'readwrite').then(store => {
      return new Promise<void>((resolve, reject) => {
        const request = store.clear();
        request.onsuccess = () => resolve();
        request.onerror = () => reject(request.error);
      });
    });
  }

  saveKdsTicket(ticket: any): Promise<void> {
    return this.getStore('kds_tickets', 'readwrite').then(store => {
      return new Promise<void>((resolve, reject) => {
        const request = store.put(ticket);
        request.onsuccess = () => resolve();
        request.onerror = () => reject(request.error);
      });
    });
  }

  getKdsTickets(): Promise<any[]> {
    return this.getStore('kds_tickets', 'readonly').then(store => {
      return new Promise<any[]>((resolve, reject) => {
        const request = store.getAll();
        request.onsuccess = () => resolve(request.result);
        request.onerror = () => reject(request.error);
      });
    });
  }

  deleteKdsTicket(ticketId: string): Promise<void> {
    return this.getStore('kds_tickets', 'readwrite').then(store => {
      return new Promise<void>((resolve, reject) => {
        const request = store.delete(ticketId);
        request.onsuccess = () => resolve();
        request.onerror = () => reject(request.error);
      });
    });
  }
}
