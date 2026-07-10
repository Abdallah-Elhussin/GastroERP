import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { WorkflowRepository } from './workflow.repository';

@Injectable({
  providedIn: 'root'
})
export class RestWorkflowRepository extends WorkflowRepository {
  private http = inject(HttpClient);
  private apiUrl = '/api/workflow';

  getKanbanTasks(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/kanban`);
  }

  updateTaskColumn(taskId: string, column: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/kanban/move`, { taskId, column });
  }

  getApprovals(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/approvals`);
  }

  approveTask(taskId: string, approved: boolean): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/approvals/action`, { taskId, approved });
  }
}
