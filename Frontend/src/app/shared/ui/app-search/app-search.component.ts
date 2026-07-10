import { Component, ChangeDetectionStrategy, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-search',
  standalone: true,
  imports: [CommonModule, MatIconModule],
  template: `
    <div class="relative w-full">
      <span class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none text-[var(--text-muted)]">
        <mat-icon class="text-sm">search</mat-icon>
      </span>
      <input
        type="text"
        [value]="value"
        [placeholder]="placeholder"
        (input)="onInput($event)"
        class="w-full bg-[var(--bg-canvas)] text-[var(--text-primary)] pl-9 pr-4 py-2 rounded-xl border border-[var(--border-color)] focus:outline-none focus:ring-1 focus:ring-[var(--accent-color)] text-xs transition-all shadow-sm"
      />
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
export class AppSearchComponent {
  @Input() placeholder = 'Search...';
  @Input() value = '';
  @Output() queryChange = new EventEmitter<string>();

  onInput(event: Event): void {
    const query = (event.target as HTMLInputElement).value;
    this.queryChange.emit(query);
  }
}
