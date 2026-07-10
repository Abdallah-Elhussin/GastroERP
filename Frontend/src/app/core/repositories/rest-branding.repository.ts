import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { BrandingRepository } from './branding.repository';
import { BrandingConfig } from '../models/erp.models';

@Injectable({
  providedIn: 'root'
})
export class RestBrandingRepository extends BrandingRepository {
  private http = inject(HttpClient);
  private apiUrl = '/api/branding';

  getBrandingConfig(): Observable<BrandingConfig> {
    return this.http.get<BrandingConfig>(this.apiUrl);
  }

  updateBrandingConfig(config: Partial<BrandingConfig>): Observable<BrandingConfig> {
    return this.http.put<BrandingConfig>(this.apiUrl, config);
  }
}
