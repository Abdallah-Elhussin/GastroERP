import { Component, ChangeDetectionStrategy, Input, inject, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LanguageService } from '../../../core/services/language.service';

@Component({
  selector: 'app-chart',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="flex flex-col gap-4 text-left select-none">
      <span *ngIf="title" class="text-[10px] font-bold text-[var(--text-muted)] uppercase tracking-wider">{{ title }}</span>
      
      <!-- Responsive mockup chart bars -->
      <div class="h-44 flex items-end gap-3.5 border-b border-l border-[var(--border-color)] p-2">
        <div 
          *ngFor="let value of data" 
          [style.height.%]="value"
          class="flex-1 bg-[var(--accent-color)] bg-opacity-75 hover:bg-opacity-95 rounded-t-md transition-all relative group cursor-pointer"
        >
          <!-- Hover tooltip -->
          <span class="absolute top-[-25px] left-1/2 transform -translate-x-1/2 bg-[var(--primary-color)] text-[var(--primary-contrast)] text-[9px] font-bold px-1.5 py-0.5 rounded opacity-0 group-hover:opacity-100 transition-opacity whitespace-nowrap shadow">
            {{ value }}%
          </span>
        </div>
      </div>

      <!-- X labels -->
      <div class="flex justify-between text-[9px] text-[var(--text-muted)] font-bold px-1 font-mono uppercase">
        <span *ngFor="let label of dayLabels()">{{ label }}</span>
      </div>
    </div>
  `,
  styles: [`
    :host {
      display: block;
      width: 100%;
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AppChartComponent {
  private langService = inject(LanguageService);

  @Input() title?: string;
  @Input() data: number[] = [45, 75, 55, 90, 60, 80, 70];
  @Input() labels?: string[];

  dayLabels = computed(() => {
    this.langService.language();
    if (this.labels?.length) {
      return this.labels;
    }
    return [
      this.t('chart.mon'),
      this.t('chart.tue'),
      this.t('chart.wed'),
      this.t('chart.thu'),
      this.t('chart.fri'),
      this.t('chart.sat'),
      this.t('chart.sun')
    ];
  });

  t(key: string): string {
    return this.langService.t(key);
  }
}
