import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

import { environment } from '../../../environments/environment';
import { ApiResponse, IncidentStatus, PaginatedResult } from '../../core/models';
import { CreateIncidentInput, IncidentDetail, IncidentListItem } from './incident.models';

@Injectable({ providedIn: 'root' })
export class IncidentService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/incidents`;

  /** All incidents (Admin/Inspector), optionally filtered by status. */
  list(
    pageNumber: number,
    pageSize: number,
    status?: IncidentStatus | null,
  ): Observable<PaginatedResult<IncidentListItem>> {
    let params = new HttpParams()
      .set('pageNumber', pageNumber)
      .set('pageSize', pageSize);
    if (status) {
      params = params.set('status', status);
    }
    return this.http
      .get<ApiResponse<PaginatedResult<IncidentListItem>>>(this.baseUrl, { params })
      .pipe(map((res) => res.data as PaginatedResult<IncidentListItem>));
  }

  /** The current user's own incidents. */
  listMine(pageNumber: number, pageSize: number): Observable<PaginatedResult<IncidentListItem>> {
    const params = new HttpParams().set('pageNumber', pageNumber).set('pageSize', pageSize);
    return this.http
      .get<ApiResponse<PaginatedResult<IncidentListItem>>>(`${this.baseUrl}/my`, { params })
      .pipe(map((res) => res.data as PaginatedResult<IncidentListItem>));
  }

  /** The inspection queue: incidents pending inspection (Inspector/Admin). */
  pendingInspection(
    pageNumber: number,
    pageSize: number,
  ): Observable<PaginatedResult<IncidentListItem>> {
    const params = new HttpParams().set('pageNumber', pageNumber).set('pageSize', pageSize);
    return this.http
      .get<ApiResponse<PaginatedResult<IncidentListItem>>>(`${this.baseUrl}/pending-inspection`, {
        params,
      })
      .pipe(map((res) => res.data as PaginatedResult<IncidentListItem>));
  }

  getById(id: string): Observable<IncidentDetail> {
    return this.http
      .get<ApiResponse<IncidentDetail>>(`${this.baseUrl}/${id}`)
      .pipe(map((res) => res.data as IncidentDetail));
  }

  /** Creates an incident as multipart/form-data (fields + `attachments` files). */
  create(input: CreateIncidentInput): Observable<IncidentDetail> {
    const form = new FormData();
    form.append('BusinessUnit', input.businessUnit);
    form.append('SAPId', input.sapId);
    form.append('FacilityId', input.facilityId);
    form.append('LocationId', input.locationId);
    form.append('IncidentDate', input.incidentDate);
    form.append('Description', input.description);
    for (const file of input.attachments) {
      form.append('attachments', file, file.name);
    }
    // No explicit Content-Type: the browser sets the multipart boundary.
    return this.http
      .post<ApiResponse<IncidentDetail>>(this.baseUrl, form)
      .pipe(map((res) => res.data as IncidentDetail));
  }
}
