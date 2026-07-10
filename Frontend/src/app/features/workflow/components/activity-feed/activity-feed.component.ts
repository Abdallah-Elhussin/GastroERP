import { Component, ChangeDetectionStrategy, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { LanguageService } from '../../../../core/services/language.service';

export interface FeedItem {
  id: string;
  author: string;
  avatar: string;
  text: string;
  time: string;
  attachments?: string[]; // mock image attachment URLs
}

@Component({
  selector: 'app-activity-feed',
  standalone: true,
  imports: [CommonModule, MatButtonModule, MatIconModule],
  templateUrl: './activity-feed.component.html',
  styleUrl: './activity-feed.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ActivityFeedComponent {
  langService = inject(LanguageService);

  feed = signal<FeedItem[]>([
    {
      id: 'feed-1',
      author: 'Julian Sterling',
      avatar: 'https://images.unsplash.com/photo-1577219491135-ce391730fb2c?w=80',
      text: 'Drafted and sent PO #2094 to supplier for organic salad ingredient inventory restocking.',
      time: '12m ago',
      attachments: ['https://images.unsplash.com/photo-1587314168485-3236d6710814?w=150']
    },
    {
      id: 'feed-2',
      author: 'System automation',
      avatar: 'https://images.unsplash.com/photo-1543007630-9710e4a00a20?w=80',
      text: 'Trigger alert rule: "High-Value Order Kitchen Alert" executed on order ticket #1084 ($180.00).',
      time: '45m ago'
    },
    {
      id: 'feed-3',
      author: 'Chef Julian',
      avatar: 'https://images.unsplash.com/photo-1577219491135-ce391730fb2c?w=80',
      text: 'Updated company primary theme colors to corporate royal purple via live branding engine settings.',
      time: '2h ago'
    }
  ]);

  private sanitizeText(raw: string): string {
    // Basic XSS stripping of HTML/script tag injections
    return raw.replace(/<[^>]*>/g, '');
  }

  addComment(commentInput: HTMLTextAreaElement): void {
    const text = this.sanitizeText(commentInput.value.trim());
    if (!text) return;

    const newItem: FeedItem = {
      id: `feed-${Date.now()}`,
      author: 'Julian Sterling',
      avatar: 'https://images.unsplash.com/photo-1577219491135-ce391730fb2c?w=80',
      text,
      time: 'Just now'
    };

    this.feed.update(list => [newItem, ...list]);
    commentInput.value = '';
  }

  attachFile(): void {
    alert('Simulating file attachments... File uploaded and linked to timeline activity.');
    this.feed.update(list => {
      if (list.length > 0) {
        const first = list[0];
        const updated = {
          ...first,
          attachments: [...(first.attachments || []), 'https://images.unsplash.com/photo-1543007630-9710e4a00a20?w=150']
        };
        return [updated, ...list.slice(1)];
      }
      return list;
    });
  }

  t(key: string): string {
    return this.langService.t(key);
  }
}
