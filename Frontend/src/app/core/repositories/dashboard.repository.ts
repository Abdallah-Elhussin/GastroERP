import { Observable } from 'rxjs';
import {
  EnterpriseDashboardActivities,
  EnterpriseDashboardCustomers,
  EnterpriseDashboardDelivery,
  EnterpriseDashboardFilter,
  EnterpriseDashboardFinance,
  EnterpriseDashboardHr,
  EnterpriseDashboardInventory,
  EnterpriseDashboardKitchen,
  EnterpriseDashboardOverview,
  EnterpriseDashboardProducts,
  EnterpriseDashboardSales
} from '../models/enterprise-dashboard.models';

export abstract class DashboardRepository {
  abstract getOverview(filter: EnterpriseDashboardFilter): Observable<EnterpriseDashboardOverview | null>;
  abstract getSales(filter: EnterpriseDashboardFilter): Observable<EnterpriseDashboardSales | null>;
  abstract getProducts(filter: EnterpriseDashboardFilter): Observable<EnterpriseDashboardProducts | null>;
  abstract getCustomers(filter: EnterpriseDashboardFilter): Observable<EnterpriseDashboardCustomers | null>;
  abstract getInventory(filter: EnterpriseDashboardFilter): Observable<EnterpriseDashboardInventory | null>;
  abstract getFinance(filter: EnterpriseDashboardFilter): Observable<EnterpriseDashboardFinance | null>;
  abstract getKitchen(filter: EnterpriseDashboardFilter): Observable<EnterpriseDashboardKitchen | null>;
  abstract getDelivery(filter: EnterpriseDashboardFilter): Observable<EnterpriseDashboardDelivery | null>;
  abstract getHr(filter: EnterpriseDashboardFilter): Observable<EnterpriseDashboardHr | null>;
  abstract getActivities(filter: EnterpriseDashboardFilter): Observable<EnterpriseDashboardActivities | null>;
}
