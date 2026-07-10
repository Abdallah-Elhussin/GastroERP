import { Component, ChangeDetectionStrategy, inject, signal, computed, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AppButtonComponent } from '../../shared/ui/app-button/app-button.component';
import { AppCardComponent } from '../../shared/ui/app-card/app-card.component';
import { AppChartComponent } from '../../shared/ui/app-chart/app-chart.component';
import { AppStatCardComponent } from '../../shared/ui/app-stat-card/app-stat-card.component';
import { AppEmptyStateComponent } from '../../shared/ui/app-empty-state/app-empty-state.component';
import { MatIconModule } from '@angular/material/icon';
import { DataService } from '../../core/services/data.service';
import { LanguageService } from '../../core/services/language.service';
import { AuthService } from '../../core/services/auth.service';
import { DashboardRepository } from '../../core/repositories/dashboard.repository';

export interface DashboardWidget {
  id: string;
  titleKey: string;
  visible: boolean;
  pinned: boolean;
  requiredRole: 'Chef Administrator' | 'Any';
  size: 'col-span-1' | 'col-span-2' | 'col-span-3';
}

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    MatIconModule,
    AppButtonComponent,
    AppCardComponent,
    AppChartComponent,
    AppStatCardComponent,
    AppEmptyStateComponent
  ],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DashboardComponent {
  dataService = inject(DataService);
  langService = inject(LanguageService);
  authService = inject(AuthService);
  dashboardRepo = inject(DashboardRepository);

  private readonly LAYOUT_KEY = 'gastro-dashboard-layout';

  isCustomizing = signal<boolean>(false);

  private readonly defaultWidgets: DashboardWidget[] = [
    { id: 'sales', titleKey: 'dash.widget.sales', visible: true, pinned: false, requiredRole: 'Chef Administrator', size: 'col-span-2' },
    { id: 'kds', titleKey: 'dash.widget.kds', visible: true, pinned: false, requiredRole: 'Any', size: 'col-span-1' },
    { id: 'stock', titleKey: 'dash.widget.stock', visible: true, pinned: false, requiredRole: 'Any', size: 'col-span-1' },
    { id: 'hr', titleKey: 'dash.widget.hr', visible: true, pinned: false, requiredRole: 'Chef Administrator', size: 'col-span-2' }
  ];

  widgets = signal<DashboardWidget[]>([...this.defaultWidgets]);

  activeWidgets = computed(() => this.widgets().filter(w => w.visible));

  kdsStatuses = computed(() => {
    this.langService.language();
    return [
      { labelKey: 'dash.kds.new', count: 2, bg: 'bg-blue-500', color: 'text-blue-500' },
      { labelKey: 'dash.kds.preparing', count: 2, bg: 'bg-amber-500', color: 'text-amber-500' },
      { labelKey: 'dash.kds.ready', count: 0, bg: 'bg-emerald-500', color: 'text-emerald-500' }
    ];
  });

  stockAlerts = computed(() => {
    this.langService.language();
    return [
      { nameKey: 'dash.item.salad', status: 'low_stock', badgeKey: 'dash.stock.low' },
      { nameKey: 'dash.item.truffles', status: 'out_of_stock', badgeKey: 'dash.stock.out' }
    ];
  });

  constructor() {
    const saved = localStorage.getItem(this.LAYOUT_KEY);
    if (saved) {
      try {
        const parsed = JSON.parse(saved) as Array<Partial<DashboardWidget> & { title?: string }>;
        this.widgets.set(parsed.map(w => ({
          id: w.id!,
          titleKey: w.titleKey ?? this.defaultWidgets.find(d => d.id === w.id)?.titleKey ?? 'dash.widget.sales',
          visible: w.visible ?? true,
          pinned: w.pinned ?? false,
          requiredRole: w.requiredRole ?? 'Any',
          size: w.size ?? 'col-span-1'
        })));
      } catch (e) {
        console.error('Failed to parse saved layout', e);
      }
    }

    this.dashboardRepo.getWidgetPermissions().subscribe();

    effect(() => {
      localStorage.setItem(this.LAYOUT_KEY, JSON.stringify(this.widgets()));
    });
  }

  toggleCustomizeMode(): void {
    this.isCustomizing.update(val => !val);
  }

  toggleWidget(id: string): void {
    this.widgets.update(list => list.map(w => w.id === id ? { ...w, visible: !w.visible } : w));
  }

  togglePin(id: string): void {
    this.widgets.update(list => list.map(w => w.id === id ? { ...w, pinned: !w.pinned } : w));
  }

  resizeWidget(id: string, newSize: 'col-span-1' | 'col-span-2' | 'col-span-3'): void {
    this.widgets.update(list => list.map(w => w.id === id ? { ...w, size: newSize } : w));
  }

  resetLayout(): void {
    this.widgets.set([...this.defaultWidgets]);
    this.isCustomizing.set(false);
  }

  t(key: string): string {
    return this.langService.t(key);
  }
}
