import { Observable } from 'rxjs';

export abstract class WorkflowRepository {
  abstract getKanbanTasks(): Observable<any[]>;
  abstract updateTaskColumn(taskId: string, column: string): Observable<void>;
  abstract getApprovals(): Observable<any[]>;
  abstract approveTask(taskId: string, approved: boolean): Observable<void>;
}
