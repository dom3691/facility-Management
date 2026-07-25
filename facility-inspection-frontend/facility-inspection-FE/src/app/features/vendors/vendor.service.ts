import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

import { environment } from '../../../environments/environment';
import { ApiResponse, PaginatedResult } from '../../core/models';
import { Vendor } from './vendor.models';

/** Vendor create/update fields. `vendorCategory` is the numeric enum id. */
export interface VendorInput {
  vendorName: string;
  vendorCategory: number;
  contactPerson?: string | null;
  email?: string | null;
  phoneNumber?: string | null;
  isActive: boolean;
}

@Injectable({ providedIn: 'root' })
export class VendorService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/vendors`;

  /** Lists vendors (Inspector/Admin), optionally filtered by category / active flag. */
  list(
    pageNumber: number,
    pageSize: number,
    category?: string | null,
    isActive?: boolean | null,
  ): Observable<PaginatedResult<Vendor>> {
    let params = new HttpParams().set('pageNumber', pageNumber).set('pageSize', pageSize);
    if (category) {
      params = params.set('category', category);
    }
    if (isActive !== undefined && isActive !== null) {
      params = params.set('isActive', isActive);
    }
    return this.http
      .get<ApiResponse<PaginatedResult<Vendor>>>(this.baseUrl, { params })
      .pipe(map((res) => res.data as PaginatedResult<Vendor>));
  }

  getById(id: string): Observable<Vendor> {
    return this.http
      .get<ApiResponse<Vendor>>(`${this.baseUrl}/${id}`)
      .pipe(map((res) => res.data as Vendor));
  }

  create(input: VendorInput): Observable<Vendor> {
    return this.http
      .post<ApiResponse<Vendor>>(this.baseUrl, this.toBody(input))
      .pipe(map((res) => res.data as Vendor));
  }

  update(id: string, input: VendorInput): Observable<Vendor> {
    return this.http
      .put<ApiResponse<Vendor>>(`${this.baseUrl}/${id}`, { id, ...this.toBody(input) })
      .pipe(map((res) => res.data as Vendor));
  }

  delete(id: string): Observable<void> {
    return this.http.delete<ApiResponse<null>>(`${this.baseUrl}/${id}`).pipe(map(() => undefined));
  }

  private toBody(input: VendorInput): Record<string, unknown> {
    return {
      vendorName: input.vendorName,
      vendorCategory: input.vendorCategory,
      contactPerson: input.contactPerson || null,
      email: input.email || null,
      phoneNumber: input.phoneNumber || null,
      isActive: input.isActive,
    };
  }
}
