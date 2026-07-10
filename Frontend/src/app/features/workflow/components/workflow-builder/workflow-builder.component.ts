import { Component, ChangeDetectionStrategy, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { LanguageService } from '../../../../core/services/language.service';

export interface WorkflowRule {
  id: string;
  name: string;
  trigger: string;
  condition: string;
  action: string;
  isActive: boolean;
}

@Component({
  selector: 'app-workflow-builder',
  standalone: true,
  imports: [CommonModule, MatButtonModule, MatIconModule],
  templateUrl: './workflow-builder.component.html',
  styleUrl: './workflow-builder.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class WorkflowBuilderComponent {
  langService = inject(LanguageService);

  rules = signal<WorkflowRule[]>([
    {
      id: 'rule-1',
      name: 'High-Value Order Kitchen Alert',
      trigger: 'Order Placed',
      condition: 'Total Value > $150.00',
      action: 'Notify Chef Administrator',
      isActive: true
    },
    {
      id: 'rule-2',
      name: 'Stock Shortage Automated Order Request',
      trigger: 'Inventory Level Dips',
      condition: 'Item Stock < 10%',
      action: 'Draft supplier purchase order',
      isActive: true
    },
    {
      id: 'rule-3',
      name: 'Shift Timeout Audit Logger',
      trigger: 'Employee Shift Ends',
      condition: 'Time > 10 Hours',
      action: 'Require manager approval signature',
      isActive: false
    }
  ]);

  toggleRule(ruleId: string): void {
    this.rules.update(list => 
      list.map(r => r.id === ruleId ? { ...r, isActive: !r.isActive } : r)
    );
  }

  deleteRule(ruleId: string): void {
    if (confirm(this.t('workflow.builder.delete'))) {
      this.rules.update(list => list.filter(r => r.id !== ruleId));
    }
  }

  addNewRule(): void {
    const name = prompt('Enter rule name:');
    if (!name) return;

    const newRule: WorkflowRule = {
      id: `rule-${Date.now()}`,
      name,
      trigger: 'Order Placed',
      condition: 'Value > $50',
      action: 'Send Alert notification',
      isActive: true
    };

    this.rules.update(curr => [...curr, newRule]);
  }

  t(key: string): string {
    return this.langService.t(key);
  }
}
