import { Observable } from 'rxjs';

export abstract class DashboardRepository {
  abstract getSalesOverview(): Observable<any>;
  abstract getWidgetPermissions(): Observable<any>;
}
