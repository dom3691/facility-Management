import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

import { environment } from '../../../environments/environment';
import { ApiResponse, PaginatedResult } from '../../core/models';
import { WorkOrder } from './work-order.models';

@Injectable({ providedIn: 'root' })
export class WorkOrderService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/work-orders`;

  /** All work orders (Admin/Inspector), optionally filtered by status. */
  list(
    pageNumber: number,
    pageSize: number,
    status?: string | null,
  ): Observable<PaginatedResult<WorkOrder>> {
    let params = new HttpParams().set('pageNumber', pageNumber).set('pageSize', pageSize);
    if (status) {
      params = params.set('status', status);
    }
    return this.http
      .get<ApiResponse<PaginatedResult<WorkOrder>>>(this.baseUrl, { params })
      .pipe(map((res) => res.data as PaginatedResult<WorkOrder>));
  }

  /** The current vendor's own work orders. */
  myVendor(pageNumber: number, pageSize: number): Observable<PaginatedResult<WorkOrder>> {
    const params = new HttpParams().set('pageNumber', pageNumber).set('pageSize', pageSize);
    return this.http
      .get<ApiResponse<PaginatedResult<WorkOrder>>>(`${this.baseUrl}/my-vendor-work-orders`, {
        params,
      })
      .pipe(map((res) => res.data as PaginatedResult<WorkOrder>));
  }

  getById(id: string): Observable<WorkOrder> {
    return this.http
      .get<ApiResponse<WorkOrder>>(`${this.baseUrl}/${id}`)
      .pipe(map((res) => res.data as WorkOrder));
  }
}
