import { Observable } from 'rxjs';

export abstract class FinanceRepository {
  abstract getLedgerEntries(): Observable<any[]>;
}
