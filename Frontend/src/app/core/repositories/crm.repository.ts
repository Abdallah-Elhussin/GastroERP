import { Observable } from 'rxjs';

export abstract class CrmRepository {
  abstract getCustomers(): Observable<any[]>;
}
