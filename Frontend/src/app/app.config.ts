import { ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';

import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { routes } from './app.routes';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { MediaRepository } from './core/repositories/media.repository';
import { RestMediaRepository } from './core/repositories/rest-media.repository';
import { BrandingRepository } from './core/repositories/branding.repository';
import { RestBrandingRepository } from './core/repositories/rest-branding.repository';
import { PosRepository } from './core/repositories/pos.repository';
import { RestPosRepository } from './core/repositories/rest-pos.repository';
import { KitchenRepository } from './core/repositories/kitchen.repository';
import { RestKitchenRepository } from './core/repositories/rest-kitchen.repository';
import { SettingsRepository } from './core/repositories/settings.repository';
import { RestSettingsRepository } from './core/repositories/rest-settings.repository';
import { DashboardRepository } from './core/repositories/dashboard.repository';
import { RestDashboardRepository } from './core/repositories/rest-dashboard.repository';
import { ReportsRepository } from './core/repositories/reports.repository';
import { RestReportsRepository } from './core/repositories/rest-reports.repository';
import { HrRepository } from './core/repositories/hr.repository';
import { RestHrRepository } from './core/repositories/rest-hr.repository';
import { WorkflowRepository } from './core/repositories/workflow.repository';
import { RestWorkflowRepository } from './core/repositories/rest-workflow.repository';
import { FinanceRepository } from './core/repositories/finance.repository';
import { RestFinanceRepository } from './core/repositories/rest-finance.repository';
import { CrmRepository } from './core/repositories/crm.repository';
import { RestCrmRepository } from './core/repositories/rest-crm.repository';
import { InventoryRepository } from './core/repositories/inventory.repository';
import { RestInventoryRepository } from './core/repositories/rest-inventory.repository';
import { CatalogRepository } from './core/repositories/catalog.repository';
import { RestCatalogRepository } from './core/repositories/rest-catalog.repository';
import { MenuRepository } from './core/repositories/menu.repository';
import { RestMenuRepository } from './core/repositories/rest-menu.repository';
import { authInterceptor } from './core/interceptors/auth.interceptor';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }), 
    provideRouter(routes), 
    provideAnimationsAsync(),
    provideHttpClient(withInterceptors([authInterceptor])),
    { provide: MediaRepository, useClass: RestMediaRepository },
    { provide: BrandingRepository, useClass: RestBrandingRepository },
    { provide: PosRepository, useClass: RestPosRepository },
    { provide: KitchenRepository, useClass: RestKitchenRepository },
    { provide: SettingsRepository, useClass: RestSettingsRepository },
    { provide: DashboardRepository, useClass: RestDashboardRepository },
    { provide: ReportsRepository, useClass: RestReportsRepository },
    { provide: HrRepository, useClass: RestHrRepository },
    { provide: WorkflowRepository, useClass: RestWorkflowRepository },
    { provide: FinanceRepository, useClass: RestFinanceRepository },
    { provide: CrmRepository, useClass: RestCrmRepository },
    { provide: InventoryRepository, useClass: RestInventoryRepository },
    { provide: CatalogRepository, useClass: RestCatalogRepository },
    { provide: MenuRepository, useClass: RestMenuRepository }
  ]
};
