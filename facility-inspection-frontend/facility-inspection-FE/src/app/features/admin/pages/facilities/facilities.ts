import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';

import { ReferenceService } from '../../../../core/services/reference.service';
import { Facility } from '../../../../core/models';
import { NormalizedHttpError } from '../../../../core/interceptors/error.interceptor';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { DataColumn, DataTable } from '../../../../shared/components/data-table/data-table';
import { FacilityFormDialog } from '../../components/facility-form-dialog/facility-form-dialog';

@Component({
  selector: 'app-admin-facilities',
  imports: [
    FormsModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule,
    PageHeader,
    DataTable,
  ],
  templateUrl: './facilities.html',
  styleUrl: './facilities.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AdminFacilities {
  private readonly reference = inject(ReferenceService);
  private readonly dialog = inject(MatDialog);
  private readonly snackbar = inject(MatSnackBar);

  readonly columns: DataColumn[] = [
    { key: 'name', header: 'Facility' },
    { key: 'code', header: 'Code', type: 'mono' },
    { key: 'isActive', header: 'Active', type: 'boolean' },
  ];

  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly items = signal<Facility[]>([]);
  readonly search = signal('');

  readonly visibleRows = computed(() => {
    const term = this.search().trim().toLowerCase();
    if (!term) {
      return this.items();
    }
    return this.items().filter((f) => `${f.name} ${f.code}`.toLowerCase().includes(term));
  });

  constructor() {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.reference.getFacilities().subscribe({
      next: (facilities) => {
        this.items.set(facilities);
        this.loading.set(false);
      },
      error: (err: NormalizedHttpError) => {
        this.error.set(err?.message ?? 'Could not load facilities.');
        this.loading.set(false);
      },
    });
  }

  add(): void {
    this.dialog
      .open(FacilityFormDialog, { width: '420px', autoFocus: false })
      .afterClosed()
      .subscribe((saved?: Facility) => {
        if (saved) {
          this.snackbar.open('Facility created.', 'Dismiss', { duration: 3000 });
          this.load();
        }
      });
  }
}
