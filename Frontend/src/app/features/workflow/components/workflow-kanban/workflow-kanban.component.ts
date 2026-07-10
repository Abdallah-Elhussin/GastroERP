import { Component, ChangeDetectionStrategy, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { LanguageService } from '../../../../core/services/language.service';

export interface KanbanTask {
  id: string;
  title: string;
  description: string;
  category: 'Orders' | 'Inventory' | 'HR' | 'Finance';
  assignee: { name: string; avatar: string };
  deadline: string;
  progress: number; // percentage
  status: 'todo' | 'in_progress' | 'review' | 'done';
}

@Component({
  selector: 'app-workflow-kanban',
  standalone: true,
  imports: [CommonModule, MatButtonModule, MatIconModule, MatMenuModule],
  templateUrl: './workflow-kanban.component.html',
  styleUrl: './workflow-kanban.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class WorkflowKanbanComponent {
  langService = inject(LanguageService);

  tasks = signal<KanbanTask[]>([
    { 
      id: 'task-1', 
      title: 'Review supplier invoice #9024', 
      description: 'Confirm raw material pricing for bulk wagyu beef orders matches purchase log.', 
      category: 'Finance', 
      assignee: { name: 'Julian S.', avatar: 'https://images.unsplash.com/photo-1577219491135-ce391730fb2c?w=80' },
      deadline: '2026-07-12', 
      progress: 40, 
      status: 'todo' 
    },
    { 
      id: 'task-2', 
      title: 'Restock organic salad ingredients', 
      description: 'Inventory levels dipped below safe limits. Restock organic greens and dressing batches.', 
      category: 'Inventory', 
      assignee: { name: 'Julian S.', avatar: 'https://images.unsplash.com/photo-1577219491135-ce391730fb2c?w=80' },
      deadline: '2026-07-10', 
      progress: 80, 
      status: 'in_progress' 
    },
    { 
      id: 'task-3', 
      title: 'Setup KDS station monitor #2', 
      description: 'Configure layout display for desserts prep queue at the pastry station.', 
      category: 'Orders', 
      assignee: { name: 'Chef Julian', avatar: 'https://images.unsplash.com/photo-1577219491135-ce391730fb2c?w=80' },
      deadline: '2026-07-11', 
      progress: 100, 
      status: 'review' 
    },
    { 
      id: 'task-4', 
      title: 'Onboard 2 new kitchen helpers', 
      description: 'Verify health cards, log contact emails, and sign employment contract templates.', 
      category: 'HR', 
      assignee: { name: 'Julian Sterling', avatar: 'https://images.unsplash.com/photo-1577219491135-ce391730fb2c?w=80' },
      deadline: '2026-07-09', 
      progress: 100, 
      status: 'done' 
    }
  ]);

  columns = [
    { labelKey: 'workflow.kanban.col.todo', value: 'todo' as const, bg: 'border-l-4 border-l-slate-400' },
    { labelKey: 'workflow.kanban.col.inProgress', value: 'in_progress' as const, bg: 'border-l-4 border-l-blue-500' },
    { labelKey: 'workflow.kanban.col.review', value: 'review' as const, bg: 'border-l-4 border-l-amber-500' },
    { labelKey: 'workflow.kanban.col.done', value: 'done' as const, bg: 'border-l-4 border-l-emerald-500' }
  ];

  getTasksByColumn(status: 'todo' | 'in_progress' | 'review' | 'done') {
    return this.tasks().filter(t => t.status === status);
  }

  moveTask(taskId: string, targetStatus: 'todo' | 'in_progress' | 'review' | 'done'): void {
    this.tasks.update(current => 
      current.map(t => t.id === taskId ? { ...t, status: targetStatus, progress: targetStatus === 'done' ? 100 : t.progress } : t)
    );
  }

  addNewTask(): void {
    const title = prompt('Enter task title:');
    if (!title) return;
    const desc = prompt('Enter task details:');
    
    const newTask: KanbanTask = {
      id: `task-${Date.now()}`,
      title,
      description: desc || '',
      category: 'Orders',
      assignee: { name: 'Julian S.', avatar: 'https://images.unsplash.com/photo-1577219491135-ce391730fb2c?w=80' },
      deadline: new Date().toISOString().split('T')[0],
      progress: 0,
      status: 'todo'
    };

    this.tasks.update(curr => [...curr, newTask]);
  }

  t(key: string): string {
    return this.langService.t(key);
  }
}
