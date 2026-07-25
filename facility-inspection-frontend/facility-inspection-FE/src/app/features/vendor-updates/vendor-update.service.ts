import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

import { environment } from '../../../environments/environment';
import { ApiResponse } from '../../core/models';
import { CreateVendorUpdateInput, MarkCompleteInput, VendorUpdate } from './vendor-update.models';

@Injectable({ providedIn: 'root' })
export class VendorUpdateService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/vendor-updates`;

  getByWorkOrder(workOrderId: string): Observable<VendorUpdate[]> {
    return this.http
      .get<ApiResponse<VendorUpdate[]>>(`${this.baseUrl}/by-work-order/${workOrderId}`)
      .pipe(map((res) => res.data ?? []));
  }

  /** Posts a progress update (multipart: fields + `attachments` evidence). */
  createProgress(input: CreateVendorUpdateInput): Observable<VendorUpdate> {
    const form = new FormData();
    form.append('WorkOrderId', input.workOrderId);
    form.append('ProgressComment', input.progressComment);
    if (input.progressPercentage !== null && input.progressPercentage !== undefined) {
      form.append('ProgressPercentage', String(input.progressPercentage));
    }
    for (const file of input.attachments) {
      form.append('attachments', file, file.name);
    }
    return this.http
      .post<ApiResponse<VendorUpdate>>(this.baseUrl, form)
      .pipe(map((res) => res.data as VendorUpdate));
  }

  /** Marks the work order complete (multipart: completion comment + `attachments`). */
  markComplete(input: MarkCompleteInput): Observable<VendorUpdate> {
    const form = new FormData();
    form.append('CompletionComment', input.completionComment);
    for (const file of input.attachments) {
      form.append('attachments', file, file.name);
    }
    return this.http
      .post<ApiResponse<VendorUpdate>>(`${this.baseUrl}/${input.workOrderId}/mark-complete`, form)
      .pipe(map((res) => res.data as VendorUpdate));
  }
}
