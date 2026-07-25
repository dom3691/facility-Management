import { ChangeDetectionStrategy, Component, computed, input, TemplateRef } from '@angular/core';
import { DatePipe, NgTemplateOutlet } from '@angular/common';
import { MatTableModule } from '@angular/material/table';

/** Column definition for the generic {@link DataTable}. */
export interface DataColumn {
  key: string;
  header: string;
  type?: 'text' | 'mono' | 'date' | 'boolean';
  /** Value accessor; defaults to `row[key]`. */
  value?: (row: any) => unknown;
  align?: 'start' | 'end';
}

/**
 * Reusable, declarative data table over Angular Material's `mat-table`.
 * Pass `columns` + `rows`; optionally project a per-row `actions` template
 * (`<ng-template let-row>…</ng-template>`) for the trailing actions column.
 */
@Component({
  selector: 'app-data-table',
  imports: [MatTableModule, DatePipe, NgTemplateOutlet],
  templateUrl: './data-table.html',
  styleUrl: './data-table.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DataTable {
  readonly columns = input.required<DataColumn[]>();
  readonly rows = input.required<readonly any[]>();
  readonly actions = input<TemplateRef<{ $implicit: any }> | null>(null);
  readonly emptyMessage = input<string>('No records to display.');

  readonly displayedColumns = computed(() => {
    const keys = this.columns().map((c) => c.key);
    return this.actions() ? [...keys, '__actions'] : keys;
  });

  cellValue(col: DataColumn, row: any): unknown {
    return col.value ? col.value(row) : row?.[col.key];
  }
}
