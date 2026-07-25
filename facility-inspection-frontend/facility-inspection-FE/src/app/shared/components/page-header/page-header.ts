import { ChangeDetectionStrategy, Component, input } from '@angular/core';

/**
 * Reusable page header for feature pages: title + optional subtitle, with an
 * `[actions]`-projected slot for page-level buttons. Breadcrumbs are rendered by
 * the shell above the content, so they aren't duplicated here.
 *
 * Usage:
 *   <app-page-header title="Incidents" subtitle="All reported incidents">
 *     <button actions mat-flat-button>Report incident</button>
 *   </app-page-header>
 */
@Component({
  selector: 'app-page-header',
  templateUrl: './page-header.html',
  styleUrl: './page-header.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PageHeader {
  readonly title = input.required<string>();
  readonly subtitle = input<string>();
}
