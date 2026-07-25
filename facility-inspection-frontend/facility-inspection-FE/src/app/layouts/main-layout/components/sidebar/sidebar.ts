import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';

import { AuthService } from '../../../../core/services/auth.service';
import { LayoutService } from '../../../../core/services/layout.service';
import { NAV_GROUPS, NavItem } from '../../navigation';

/**
 * Left sidebar: brand, role-filtered grouped navigation, and a collapse control.
 * When {@link LayoutService.rail} is active it renders icons only (labels become
 * tooltips). Empty groups (after role filtering) are hidden.
 */
@Component({
  selector: 'app-sidebar',
  imports: [RouterLink, RouterLinkActive, MatIconModule, MatButtonModule, MatTooltipModule],
  templateUrl: './sidebar.html',
  styleUrl: './sidebar.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Sidebar {
  private readonly auth = inject(AuthService);
  readonly layout = inject(LayoutService);

  readonly visibleGroups = computed(() =>
    NAV_GROUPS.map((group) => ({
      ...group,
      items: group.items.filter((item) => this.canSee(item)),
    })).filter((group) => group.items.length > 0),
  );

  private canSee(item: NavItem): boolean {
    return !item.roles || this.auth.hasAnyRole(...item.roles);
  }

  onNavigate(): void {
    this.layout.closeOnNavigate();
  }
}
