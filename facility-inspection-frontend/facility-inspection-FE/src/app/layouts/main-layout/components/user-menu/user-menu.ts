import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatDividerModule } from '@angular/material/divider';

import { AuthService } from '../../../../core/services/auth.service';

/** Top-bar user control: avatar initials + dropdown (identity, sign out). */
@Component({
  selector: 'app-user-menu',
  imports: [MatButtonModule, MatIconModule, MatMenuModule, MatDividerModule],
  templateUrl: './user-menu.html',
  styleUrl: './user-menu.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UserMenu {
  private readonly auth = inject(AuthService);

  readonly user = this.auth.currentUser;

  readonly initials = computed(() => {
    const name = this.user()?.fullName?.trim();
    if (!name) {
      return '?';
    }
    return name
      .split(/\s+/)
      .map((part) => part[0])
      .slice(0, 2)
      .join('')
      .toUpperCase();
  });

  readonly primaryRole = computed(() => this.user()?.roles?.[0] ?? '');

  logout(): void {
    this.auth.logout();
  }
}
