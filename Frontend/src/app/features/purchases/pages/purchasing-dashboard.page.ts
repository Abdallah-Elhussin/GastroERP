import { ChangeDetectionStrategy, Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { catchError, of } from 'rxjs';
import { LanguageService } from '../../../core/services/language.service';
import { PurchasingDashboardRepository } from '../../../core/repositories/purchasing-dashboard.repository';
import {
  PurchasingDashboardActivity,
  PurchasingDashboardAlert,
  PurchasingDashboardSummary
} from '../../../core/models/purchasing-dashboard.models';

interface PurchaseKpiCard {
  key: string;
  icon: string;
  value: number;
  metaALabel: string;
  metaAValue: number;
  metaBLabel: string;
  metaBValue: number;
  link: string;
}

@Component({
  selector: 'app-purchasing-dashboard-page',
  standalone: true,
  imports: [CommonModule, RouterLink, MatIconModule],
  templateUrl: './purchasing-dashboard.page.html',
  styleUrl: './purchasing-dashboard.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PurchasingDashboardPage implements OnInit {
  private readonly repo = inject(PurchasingDashboardRepository);
  lang = inject(LanguageService);

  loading = signal(false);
  error = signal<string | null>(null);
  alerts = signal<PurchasingDashboardAlert[]>([]);
  recent = signal<PurchasingDashboardActivity[]>([]);

  cards = signal<PurchaseKpiCard[]>([
    {
      key: 'pur.dash.card.openPos',
      icon: 'shopping_cart',
      value: 0,
      metaALabel: 'pur.dash.meta.late',
      metaAValue: 0,
      metaBLabel: 'pur.dash.meta.total',
      metaBValue: 0,
      link: '/purchases/purchase-orders'
    },
    {
      key: 'pur.dash.card.uninvoicedReceipts',
      icon: 'inventory',
      value: 0,
      metaALabel: 'pur.dash.meta.pending',
      metaAValue: 0,
      metaBLabel: 'pur.dash.meta.total',
      metaBValue: 0,
      link: '/purchases/goods-receipts'
    },
    {
      key: 'pur.dash.card.unpaidInvoices',
      icon: 'receipt_long',
      value: 0,
      metaALabel: 'pur.dash.meta.draft',
      metaAValue: 0,
      metaBLabel: 'pur.dash.meta.total',
      metaBValue: 0,
      link: '/purchases/purchase-invoices'
    },
    {
      key: 'pur.dash.card.suppliers',
      icon: 'local_shipping',
      value: 0,
      metaALabel: 'pur.dash.meta.active',
      metaAValue: 0,
      metaBLabel: 'pur.dash.meta.overCredit',
      metaBValue: 0,
      link: '/purchases/suppliers'
    }
  ]);

  t = (key: string) => this.lang.t(key);

  ngOnInit(): void {
    this.load();
  }

  refresh(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.repo
      .getDashboard()
      .pipe(
        catchError(err => {
          this.error.set(err?.error?.error ?? this.t('pur.dash.loadFailed'));
          this.loading.set(false);
          return of(null);
        })
      )
      .subscribe(summary => {
        if (!summary) return;
        this.applySummary(summary);
        this.loading.set(false);
      });
  }

  alertMessage(alert: PurchasingDashboardAlert): string {
    return this.lang.language() === 'ar' ? alert.messageAr : alert.messageEn;
  }

  activityKindLabel(kind: string): string {
    const key = `pur.dash.activity.${kind}`;
    const translated = this.t(key);
    return translated === key ? kind : translated;
  }

  private applySummary(s: PurchasingDashboardSummary): void {
    this.cards.set([
      {
        key: 'pur.dash.card.openPos',
        icon: 'shopping_cart',
        value: s.openPurchaseOrders,
        metaALabel: 'pur.dash.meta.late',
        metaAValue: s.latePurchaseOrders,
        metaBLabel: 'pur.dash.meta.total',
        metaBValue: s.totalPurchaseOrders,
        link: '/purchases/purchase-orders'
      },
      {
        key: 'pur.dash.card.uninvoicedReceipts',
        icon: 'inventory',
        value: s.uninvoicedReceipts,
        metaALabel: 'pur.dash.meta.pending',
        metaAValue: s.pendingReceipts,
        metaBLabel: 'pur.dash.meta.total',
        metaBValue: s.totalReceipts,
        link: '/purchases/goods-receipts'
      },
      {
        key: 'pur.dash.card.unpaidInvoices',
        icon: 'receipt_long',
        value: s.unpaidInvoices,
        metaALabel: 'pur.dash.meta.draft',
        metaAValue: s.draftInvoices,
        metaBLabel: 'pur.dash.meta.total',
        metaBValue: s.totalInvoices,
        link: '/purchases/purchase-invoices'
      },
      {
        key: 'pur.dash.card.suppliers',
        icon: 'local_shipping',
        value: s.totalSuppliers,
        metaALabel: 'pur.dash.meta.active',
        metaAValue: s.activeSuppliers,
        metaBLabel: 'pur.dash.meta.overCredit',
        metaBValue: s.overCreditSuppliers,
        link: '/purchases/suppliers'
      }
    ]);
    this.alerts.set(s.alerts ?? []);
    this.recent.set(s.recentActivities ?? []);
  }
}
