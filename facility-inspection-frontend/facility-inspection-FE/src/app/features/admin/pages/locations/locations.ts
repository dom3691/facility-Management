import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';

import { ReferenceService } from '../../../../core/services/reference.service';
import { Facility, Location } from '../../../../core/models';
import { NormalizedHttpError } from '../../../../core/interceptors/error.interceptor';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { DataColumn, DataTable } from '../../../../shared/components/data-table/data-table';
import { LocationFormDialog } from '../../components/location-form-dialog/location-form-dialog';

@Component({
  selector: 'app-admin-locations',
  imports: [
    FormsModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatProgressSpinnerModule,
    PageHeader,
    DataTable,
  ],
  templateUrl: './locations.html',
  styleUrl: './locations.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AdminLocations {
  private readonly reference = inject(ReferenceService);
  private readonly dialog = inject(MatDialog);
  private readonly snackbar = inject(MatSnackBar);

  readonly facilities = signal<Facility[]>([]);
  private readonly facilityName = (id: string) =>
    this.facilities().find((f) => f.id === id)?.name ?? '—';

  readonly columns: DataColumn[] = [
    { key: 'name', header: 'Location' },
    { key: 'code', header: 'Code', type: 'mono' },
    { key: 'facility', header: 'Facility', value: (l) => this.facilityName(l.facilityId) },
    { key: 'isActive', header: 'Active', type: 'boolean' },
  ];

  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly items = signal<Location[]>([]);
  readonly search = signal('');
  readonly facilityFilter = signal('');

  readonly visibleRows = computed(() => {
    const term = this.search().trim().toLowerCase();
    const facility = this.facilityFilter();
    return this.items().filter((l) => {
      const matchesFacility = !facility || l.facilityId === facility;
      const matchesSearch = !term || `${l.name} ${l.code ?? ''}`.toLowerCase().includes(term);
      return matchesFacility && matchesSearch;
    });
  });

  constructor() {
    this.reference.getFacilities().subscribe({
      next: (facilities) => this.facilities.set(facilities),
      error: () => this.facilities.set([]),
    });
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.reference.getLocations().subscribe({
      next: (locations) => {
        this.items.set(locations);
        this.loading.set(false);
      },
      error: (err: NormalizedHttpError) => {
        this.error.set(err?.message ?? 'Could not load locations.');
        this.loading.set(false);
      },
    });
  }

  add(): void {
    this.dialog
      .open(LocationFormDialog, {
        data: { facilities: this.facilities(), facilityId: this.facilityFilter() || undefined },
        width: '420px',
        autoFocus: false,
      })
      .afterClosed()
      .subscribe((saved?: Location) => {
        if (saved) {
          this.snackbar.open('Location created.', 'Dismiss', { duration: 3000 });
          this.load();
        }
      });
  }
}
