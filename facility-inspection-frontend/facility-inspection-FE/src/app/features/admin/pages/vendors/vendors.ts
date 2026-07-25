import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';

import { NormalizedHttpError } from '../../../../core/interceptors/error.interceptor';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { DataColumn, DataTable } from '../../../../shared/components/data-table/data-table';
import { ConfirmDialog } from '../../../../shared/components/confirm-dialog/confirm-dialog';
import { VendorService } from '../../../vendors/vendor.service';
import { Vendor, VENDOR_CATEGORIES, vendorCategoryLabel } from '../../../vendors/vendor.models';
import { VendorFormDialog } from '../../components/vendor-form-dialog/vendor-form-dialog';

@Component({
  selector: 'app-admin-vendors',
  imports: [
    FormsModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatPaginatorModule,
    MatProgressSpinnerModule,
    MatTooltipModule,
    PageHeader,
    DataTable,
  ],
  templateUrl: './vendors.html',
  styleUrl: './vendors.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AdminVendors {
  private readonly service = inject(VendorService);
  private readonly dialog = inject(MatDialog);
  private readonly snackbar = inject(MatSnackBar);

  readonly categories = VENDOR_CATEGORIES;

  readonly columns: DataColumn[] = [
    { key: 'vendorName', header: 'Vendor' },
    { key: 'vendorCategory', header: 'Category', value: (v) => vendorCategoryLabel(v.vendorCategory) },
    { key: 'contactPerson', header: 'Contact' },
    { key: 'email', header: 'Email' },
    { key: 'phoneNumber', header: 'Phone' },
    { key: 'isActive', header: 'Active', type: 'boolean', align: 'start' },
  ];

  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly items = signal<Vendor[]>([]);
  readonly total = signal(0);
  readonly pageIndex = signal(0);
  readonly pageSize = signal(10);
  readonly search = signal('');
  readonly categoryFilter = signal('');
  readonly activeFilter = signal<'' | 'true' | 'false'>('');

  readonly visibleRows = computed(() => {
    const term = this.search().trim().toLowerCase();
    if (!term) {
      return this.items();
    }
    return this.items().filter((v) =>
      [v.vendorName, v.contactPerson, v.email]
        .filter(Boolean)
        .some((s) => s!.toLowerCase().includes(term)),
    );
  });

  constructor() {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.error.set(null);
    const active = this.activeFilter() === '' ? null : this.activeFilter() === 'true';
    this.service
      .list(this.pageIndex() + 1, this.pageSize(), this.categoryFilter() || null, active)
      .subscribe({
        next: (result) => {
          this.items.set(result.items);
          this.total.set(result.totalCount);
          this.loading.set(false);
        },
        error: (err: NormalizedHttpError) => {
          this.error.set(err?.message ?? 'Could not load vendors.');
          this.loading.set(false);
        },
      });
  }

  onPage(event: PageEvent): void {
    this.pageIndex.set(event.pageIndex);
    this.pageSize.set(event.pageSize);
    this.load();
  }

  onFilterChange(): void {
    this.pageIndex.set(0);
    this.load();
  }

  add(): void {
    this.openForm();
  }

  edit(vendor: Vendor): void {
    this.openForm(vendor);
  }

  remove(vendor: Vendor): void {
    this.dialog
      .open(ConfirmDialog, {
        data: {
          title: 'Delete vendor',
          message: `Delete “${vendor.vendorName}”? This cannot be undone.`,
          confirmLabel: 'Delete',
          danger: true,
        },
      })
      .afterClosed()
      .subscribe((confirmed) => {
        if (confirmed) {
          this.service.delete(vendor.id).subscribe({
            next: () => {
              this.snackbar.open('Vendor deleted.', 'Dismiss', { duration: 3000 });
              this.load();
            },
            error: (err: NormalizedHttpError) =>
              this.snackbar.open(err?.message ?? 'Could not delete vendor.', 'Dismiss', { duration: 4000 }),
          });
        }
      });
  }

  private openForm(vendor?: Vendor): void {
    this.dialog
      .open(VendorFormDialog, { data: { vendor }, width: '480px', autoFocus: false })
      .afterClosed()
      .subscribe((saved?: Vendor) => {
        if (saved) {
          this.snackbar.open(vendor ? 'Vendor updated.' : 'Vendor created.', 'Dismiss', { duration: 3000 });
          this.load();
        }
      });
  }
}
