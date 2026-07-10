import { Component, ChangeDetectionStrategy, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-breadcrumb',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="text-[10px] text-[var(--text-muted)] font-semibold uppercase tracking-wider select-none text-left">
      <span *ngFor="let path of paths; let last = last">
        <span [class.text-[var(--text-primary)]]="last">{{ path }}</span>
        <span *ngIf="!last" class="mx-2">/</span>
      </span>
    </div>
  `,
  styles: [`
    :host {
      display: block;
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AppBreadcrumbComponent {
  @Input() paths: string[] = [];
}
