import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';

import { LayoutService } from '../../core/services/layout.service';
import { Sidebar } from './components/sidebar/sidebar';
import { TopNav } from './components/top-nav/top-nav';
import { Breadcrumb } from '../../shared/components/breadcrumb/breadcrumb';

/**
 * Authenticated application shell. Composes the sidebar + top navigation around a
 * routed content area, and owns the responsive behaviour:
 *
 * - Desktop / tablet: fixed sidebar, collapsible to an icon rail.
 * - Mobile (≤768px): the sidebar becomes an off-canvas drawer with a backdrop.
 *
 * Feature pages render into the `<router-outlet>`; this layer builds no pages.
 */
@Component({
  selector: 'app-main-layout',
  imports: [RouterOutlet, Sidebar, TopNav, Breadcrumb],
  templateUrl: './main-layout.html',
  styleUrl: './main-layout.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MainLayout {
  readonly layout = inject(LayoutService);
}
