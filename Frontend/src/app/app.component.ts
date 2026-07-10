import { Component, inject, effect } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { DataService } from './core/services/data.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent {
  title = 'gastro-erp-client';
  dataService = inject(DataService);

  constructor() {
    effect(() => {
      const config = this.dataService.branding();
      if (config.primaryColor) {
        document.documentElement.style.setProperty('--primary-color', config.primaryColor);
      }
      if (config.accentColor) {
        document.documentElement.style.setProperty('--accent-color', config.accentColor);
      }
      if (config.fontFamily) {
        document.documentElement.style.setProperty('--font-family', config.fontFamily);
      }
    });
  }
}
