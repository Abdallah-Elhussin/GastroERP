import { Component, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AppSidebarComponent } from '../../ui/app-sidebar/app-sidebar.component';
import { AppToolbarComponent } from '../../ui/app-toolbar/app-toolbar.component';

@Component({
  selector: 'app-portal-layout',
  standalone: true,
  imports: [
    CommonModule,
    AppSidebarComponent,
    AppToolbarComponent
  ],
  template: `
    <div class="h-screen w-screen flex overflow-hidden bg-[var(--bg-canvas)]">
      <!-- Main Navigation Sidenav -->
      <app-sidebar></app-sidebar>

      <!-- Center Container Panel -->
      <div class="flex-1 flex flex-col overflow-hidden">
        <!-- Top Toolbar Header -->
        <app-toolbar></app-toolbar>

        <!-- Main Dynamic Page Scroll area -->
        <main class="flex-1 overflow-y-auto p-6 relative">
          <ng-content></ng-content>
        </main>
      </div>
    </div>
  `,
  styles: [`
    :host {
      display: block;
      width: 100vw;
      height: 100vh;
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PortalLayoutComponent {}
