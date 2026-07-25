import { ChangeDetectionStrategy, Component, computed, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatBadgeModule } from '@angular/material/badge';
import { MatMenuModule } from '@angular/material/menu';
import { MatDividerModule } from '@angular/material/divider';

interface NotificationPreview {
  title: string;
  message: string;
  time: string;
}

/**
 * Top-bar notification control: bell + unread badge + dropdown preview.
 * State is local placeholder for now; wire it to a NotificationService (backing
 * `GET /api/notifications/my`) when the notifications feature is built.
 */
@Component({
  selector: 'app-notification-bell',
  imports: [
    RouterLink,
    MatButtonModule,
    MatIconModule,
    MatBadgeModule,
    MatMenuModule,
    MatDividerModule,
  ],
  templateUrl: './notification-bell.html',
  styleUrl: './notification-bell.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class NotificationBell {
  /** Placeholder until the NotificationService is introduced. */
  readonly notifications = signal<NotificationPreview[]>([]);
  readonly unreadCount = computed(() => this.notifications().length);
}
