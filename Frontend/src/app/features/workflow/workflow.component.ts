import { Component, ChangeDetectionStrategy, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { LanguageService } from '../../core/services/language.service';
import { WorkflowKanbanComponent } from './components/workflow-kanban/workflow-kanban.component';
import { WorkflowBuilderComponent } from './components/workflow-builder/workflow-builder.component';
import { ApprovalCenterComponent } from './components/approval-center/approval-center.component';
import { ActivityFeedComponent } from './components/activity-feed/activity-feed.component';

@Component({
  selector: 'app-workflow',
  standalone: true,
  imports: [
    CommonModule,
    MatButtonModule,
    MatIconModule,
    WorkflowKanbanComponent,
    WorkflowBuilderComponent,
    ApprovalCenterComponent,
    ActivityFeedComponent
  ],
  templateUrl: './workflow.component.html',
  styleUrl: './workflow.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class WorkflowComponent {
  langService = inject(LanguageService);

  activeTab = signal<'kanban' | 'builder' | 'approvals' | 'activity'>('kanban');

  setActiveTab(tab: 'kanban' | 'builder' | 'approvals' | 'activity'): void {
    this.activeTab.set(tab);
  }

  t(key: string): string {
    return this.langService.t(key);
  }
}
