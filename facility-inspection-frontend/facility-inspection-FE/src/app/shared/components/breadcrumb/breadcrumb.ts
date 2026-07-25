import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { NavigationEnd, Router, RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { filter, map, startWith } from 'rxjs/operators';

interface Crumb {
  label: string;
  url: string;
}

/** Human labels for known route segments; unknown segments are title-cased. */
const SEGMENT_LABELS: Record<string, string> = {
  dashboard: 'Dashboard',
  analytics: 'Analytics',
  notifications: 'Notifications',
  incidents: 'Incidents',
  inspections: 'Inspections',
  'work-orders': 'Work Orders',
  verifications: 'Verifications',
  vendors: 'Vendors',
  'vendor-assignments': 'Assignments',
  'vendor-updates': 'Updates',
  'reference-data': 'Reference Data',
  'audit-logs': 'Audit Logs',
};

/**
 * Router-driven breadcrumb. Rebuilds from the URL on every navigation; the last
 * crumb is the current page (not a link). Pages can later refine labels via route
 * `data` — this URL-based default needs no per-route wiring.
 */
@Component({
  selector: 'app-breadcrumb',
  imports: [RouterLink, MatIconModule],
  templateUrl: './breadcrumb.html',
  styleUrl: './breadcrumb.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Breadcrumb {
  private readonly router = inject(Router);

  readonly crumbs = toSignal(
    this.router.events.pipe(
      filter((e) => e instanceof NavigationEnd),
      startWith(null),
      map(() => this.build()),
    ),
    { initialValue: this.build() },
  );

  private build(): Crumb[] {
    const path = this.router.url.split('?')[0].split('#')[0];
    const segments = path.split('/').filter(Boolean);

    const crumbs: Crumb[] = [{ label: 'Home', url: '/dashboard' }];
    let acc = '';
    for (const segment of segments) {
      acc += `/${segment}`;
      crumbs.push({ label: SEGMENT_LABELS[segment] ?? this.titleCase(segment), url: acc });
    }
    return crumbs;
  }

  private titleCase(segment: string): string {
    return segment
      .replace(/-/g, ' ')
      .replace(/\b\w/g, (c) => c.toUpperCase());
  }
}
