import { Injectable, inject } from '@angular/core';
import { Observable, of } from 'rxjs';
import { BrandingRepository } from './branding.repository';
import { BrandingConfig } from '../models/erp.models';
import { DataService } from '../services/data.service';

@Injectable({
  providedIn: 'root'
})
export class MockBrandingRepository extends BrandingRepository {
  dataService = inject(DataService);

  getBrandingConfig(): Observable<BrandingConfig> {
    return of(this.dataService.branding());
  }

  updateBrandingConfig(config: Partial<BrandingConfig>): Observable<BrandingConfig> {
    this.dataService.updateBranding(config);
    return of(this.dataService.branding());
  }
}
