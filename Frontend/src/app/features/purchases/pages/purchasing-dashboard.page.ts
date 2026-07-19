import { ChangeDetectionStrategy, Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { LanguageService } from '../../../core/services/language.service';

interface PurchaseKpiCard {
  key: string;
  icon: string;
  value: number;
  metaA: string;
  metaB: string;
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
  lang = inject(LanguageService);
  loading = signal(false);

  cards = signal<PurchaseKpiCard[]>([
    {
      key: 'pur.dash.card.openPos',
      icon: 'shopping_cart',
      value: 0,
      metaA: 'pur.dash.meta.late',
      metaB: 'pur.dash.meta.total',
      link: '/purchases/purchase-orders'
    },
    {
      key: 'pur.dash.card.uninvoicedReceipts',
      icon: 'inventory',
      value: 0,
      metaA: 'pur.dash.meta.pending',
      metaB: 'pur.dash.meta.total',
      link: '/purchases/goods-receipts'
    },
    {
      key: 'pur.dash.card.unpaidInvoices',
      icon: 'receipt_long',
      value: 0,
      metaA: 'pur.dash.meta.draft',
      metaB: 'pur.dash.meta.total',
      link: '/purchases/purchase-invoices'
    },
    {
      key: 'pur.dash.card.suppliers',
      icon: 'local_shipping',
      value: 0,
      metaA: 'pur.dash.meta.active',
      metaB: 'pur.dash.meta.overCredit',
      link: '/purchases/suppliers'
    }
  ]);

  t = (key: string) => this.lang.t(key);

  ngOnInit(): void {
    // KPI API will be wired in the next purchasing analytics phase.
    this.loading.set(false);
  }

  refresh(): void {
    this.ngOnInit();
  }
}
