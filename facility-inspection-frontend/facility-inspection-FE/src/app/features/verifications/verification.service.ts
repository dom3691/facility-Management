import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

import { environment } from '../../../environments/environment';
import { ApiResponse, PaginatedResult } from '../../core/models';
import { WorkOrder } from '../work-orders/work-order.models';
import { CreateVerificationInput, Verification } from './verification.models';

@Injectable({ providedIn: 'root' })
export class VerificationService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/verifications`;

  /** Completed work orders awaiting verification (Initiator/Inspector/Admin). */
  getPending(pageNumber: number, pageSize: number): Observable<PaginatedResult<WorkOrder>> {
    const params = new HttpParams().set('pageNumber', pageNumber).set('pageSize', pageSize);
    return this.http
      .get<ApiResponse<PaginatedResult<WorkOrder>>>(`${this.baseUrl}/pending`, { params })
      .pipe(map((res) => res.data as PaginatedResult<WorkOrder>));
  }

  getByWorkOrder(workOrderId: string): Observable<Verification[]> {
    return this.http
      .get<ApiResponse<Verification[]>>(`${this.baseUrl}/by-work-order/${workOrderId}`)
      .pipe(map((res) => res.data ?? []));
  }

  /** Records a verification decision (Initiator/Admin). */
  create(input: CreateVerificationInput): Observable<Verification> {
    return this.http
      .post<ApiResponse<Verification>>(this.baseUrl, input)
      .pipe(map((res) => res.data as Verification));
  }
}
