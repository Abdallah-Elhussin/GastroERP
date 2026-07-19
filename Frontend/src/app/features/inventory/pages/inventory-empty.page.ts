import { Component, ChangeDetectionStrategy, inject, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { LanguageService } from '../../../core/services/language.service';
import { InventoryPageShellComponent } from '../shared/inventory-page-shell.component';

/**
 * Empty inventory screen scaffold — content filled when page specs are provided.
 */
@Component({
  selector: 'app-inventory-empty-page',
  standalone: true,
  imports: [CommonModule, InventoryPageShellComponent],
  template: `
    <app-inventory-page-shell
      [breadcrumbs]="breadcrumbs()"
      [titleKey]="titleKey()"
    >
      <div class="empty-canvas" aria-hidden="true"></div>
    </app-inventory-page-shell>
  `,
  styles: [`
    .empty-canvas {
      min-height: 18rem;
      border-radius: 1rem;
      border: 1px dashed var(--border-color);
      background: var(--bg-surface);
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class InventoryEmptyPage {
  private route = inject(ActivatedRoute);
  private lang = inject(LanguageService);

  private data = toSignal(this.route.data, { initialValue: this.route.snapshot.data });

  titleKey = computed(() => (this.data()['titleKey'] as string) ?? 'nav.inventory');

  breadcrumbs = computed(() => {
    const keys = (this.data()['breadcrumb'] as string[]) ?? ['nav.inventory'];
    return keys.map((labelKey, index) => ({
      labelKey,
      path: index === 0 ? '/inventory/dashboard' : undefined
    }));
  });

  t(key: string): string {
    return this.lang.t(key);
  }
}
