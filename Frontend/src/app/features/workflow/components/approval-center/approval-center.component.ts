import { Component, ChangeDetectionStrategy, signal, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { WorkflowRepository } from '../../../../core/repositories/workflow.repository';
import { LanguageService } from '../../../../core/services/language.service';

export interface ApprovalItem {
  id: string;
  requester: string;
  avatar: string;
  type: 'Leave Request' | 'Purchase Order' | 'Void Ticket';
  details: string;
  meta: string; // e.g. '3 Days', '$1,200.00', 'Ticket #1084'
  status: 'pending' | 'approved' | 'rejected';
}

@Component({
  selector: 'app-approval-center',
  standalone: true,
  imports: [CommonModule, MatButtonModule, MatIconModule],
  templateUrl: './approval-center.component.html',
  styleUrl: './approval-center.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ApprovalCenterComponent implements OnInit {
  workflowRepo = inject(WorkflowRepository);
  langService = inject(LanguageService);

  requests = signal<ApprovalItem[]>([
    {
      id: 'app-1',
      requester: 'Julian Sterling',
      avatar: 'https://images.unsplash.com/photo-1577219491135-ce391730fb2c?w=80',
      type: 'Leave Request',
      details: 'Requesting emergency personal leave for family commitments.',
      meta: '3 Days (Paid)',
      status: 'pending'
    },
    {
      id: 'app-2',
      requester: 'Julian S.',
      avatar: 'https://images.unsplash.com/photo-1577219491135-ce391730fb2c?w=80',
      type: 'Purchase Order',
      details: 'Draft PO #2094 submitted for signature validation. Contains kitchen storage refrigeration parts.',
      meta: '$1,850.00',
      status: 'pending'
    },
    {
      id: 'app-3',
      requester: 'Chef Julian',
      avatar: 'https://images.unsplash.com/photo-1577219491135-ce391730fb2c?w=80',
      type: 'Void Ticket',
      details: 'Void request for order ticket item #1084: 2x Gold Wagyu burgers (customer changed mind).',
      meta: 'Ticket #1084',
      status: 'approved'
    }
  ]);

  ngOnInit(): void {
    this.workflowRepo.getApprovals().subscribe(data => {
      if (data && data.length > 0) {
        this.requests.set(data);
      }
    });
  }

  resolveRequest(requestId: string, status: 'approved' | 'rejected'): void {
    const approved = status === 'approved';
    this.workflowRepo.approveTask(requestId, approved).subscribe(() => {
      this.requests.update(list => 
        list.map(r => r.id === requestId ? { ...r, status } : r)
      );
    });
  }

  getStatusLabel(status: 'approved' | 'rejected' | 'pending'): string {
    if (status === 'approved') return this.t('workflow.status.approved');
    if (status === 'rejected') return this.t('workflow.status.rejected');
    return status;
  }

  t(key: string): string {
    return this.langService.t(key);
  }
}
