import { Observable } from 'rxjs';
import { CrmCustomerSummary } from './rest-crm.repository';

export abstract class CrmRepository {
  abstract getCustomers(page?: number, pageSize?: number, search?: string): Observable<CrmCustomerSummary[]>;
}
