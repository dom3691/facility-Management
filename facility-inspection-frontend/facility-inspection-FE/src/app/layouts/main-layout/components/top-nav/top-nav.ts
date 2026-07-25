import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

import { LayoutService } from '../../../../core/services/layout.service';
import { UserMenu } from '../user-menu/user-menu';
import { NotificationBell } from '../notification-bell/notification-bell';

/**
 * Top navigation bar: sidebar toggle, global search (visual placeholder),
 * notification bell and user menu. Uses Angular Material toolbar + controls.
 */
@Component({
  selector: 'app-top-nav',
  imports: [MatToolbarModule, MatButtonModule, MatIconModule, UserMenu, NotificationBell],
  templateUrl: './top-nav.html',
  styleUrl: './top-nav.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TopNav {
  readonly layout = inject(LayoutService);
}
