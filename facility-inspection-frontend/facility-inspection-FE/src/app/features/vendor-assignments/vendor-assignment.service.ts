import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

import { environment } from '../../../environments/environment';
import { ApiResponse } from '../../core/models';
import { CreateVendorAssignmentInput, VendorAssignment } from './vendor-assignment.models';

@Injectable({ providedIn: 'root' })
export class VendorAssignmentService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/vendor-assignments`;

  getByIncident(incidentId: string): Observable<VendorAssignment[]> {
    return this.http
      .get<ApiResponse<VendorAssignment[]>>(`${this.baseUrl}/by-incident/${incidentId}`)
      .pipe(map((res) => res.data ?? []));
  }

  /** Assigns a vendor (Inspector/Admin). Body binds to CreateVendorAssignmentCommand. */
  create(input: CreateVendorAssignmentInput): Observable<VendorAssignment> {
    return this.http
      .post<ApiResponse<VendorAssignment>>(this.baseUrl, input)
      .pipe(map((res) => res.data as VendorAssignment));
  }
}
