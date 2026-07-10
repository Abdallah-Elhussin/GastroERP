import { Observable } from 'rxjs';
import { BrandingConfig } from '../models/erp.models';

export abstract class BrandingRepository {
  abstract getBrandingConfig(): Observable<BrandingConfig>;
  abstract updateBrandingConfig(config: Partial<BrandingConfig>): Observable<BrandingConfig>;
}
