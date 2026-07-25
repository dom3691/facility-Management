import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { forkJoin } from 'rxjs';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { ReferenceService } from '../../../../core/services/reference.service';
import { EnumOption } from '../../../../core/models';
import { PageHeader } from '../../../../shared/components/page-header/page-header';

interface EnumGroup {
  title: string;
  icon: string;
  options: EnumOption[];
}

const ENUM_DEFS: { key: string; title: string; icon: string }[] = [
  { key: 'incident-statuses', title: 'Incident Statuses', icon: 'report_problem' },
  { key: 'inspection-classifications', title: 'Inspection Classifications', icon: 'fact_check' },
  { key: 'vendor-categories', title: 'Vendor Categories', icon: 'store' },
  { key: 'work-order-statuses', title: 'Work Order Statuses', icon: 'engineering' },
  { key: 'verification-decisions', title: 'Verification Decisions', icon: 'verified' },
];

/** Read-only reference/lookup data (workflow enums). */
@Component({
  selector: 'app-admin-reference-data',
  imports: [MatIconModule, MatProgressSpinnerModule, PageHeader],
  templateUrl: './reference-data.html',
  styleUrl: './reference-data.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AdminReferenceData {
  private readonly reference = inject(ReferenceService);

  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly groups = signal<EnumGroup[]>([]);

  constructor() {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.error.set(null);
    forkJoin(ENUM_DEFS.map((def) => this.reference.getEnumOptions(def.key))).subscribe({
      next: (results) => {
        this.groups.set(
          ENUM_DEFS.map((def, i) => ({ title: def.title, icon: def.icon, options: results[i] })),
        );
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Could not load reference data.');
        this.loading.set(false);
      },
    });
  }
}
