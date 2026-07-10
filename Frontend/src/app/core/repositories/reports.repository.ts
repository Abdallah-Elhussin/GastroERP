import { Observable } from 'rxjs';

export abstract class ReportsRepository {
  abstract getReportsKPIs(): Observable<any>;
  abstract getPivotSalesData(): Observable<any[]>;
  abstract scheduleReport(payload: any): Observable<void>;
}
