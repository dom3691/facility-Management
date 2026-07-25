import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

import { environment } from '../../../environments/environment';
import { ApiResponse, PaginatedResult } from '../../core/models';
import { CreateInspectionInput, Inspection } from './inspection.models';

@Injectable({ providedIn: 'root' })
export class InspectionService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/inspections`;

  getById(id: string): Observable<Inspection> {
    return this.http
      .get<ApiResponse<Inspection>>(`${this.baseUrl}/${id}`)
      .pipe(map((res) => res.data as Inspection));
  }

  getByIncident(incidentId: string): Observable<Inspection[]> {
    return this.http
      .get<ApiResponse<Inspection[]>>(`${this.baseUrl}/by-incident/${incidentId}`)
      .pipe(map((res) => res.data ?? []));
  }

  getMine(pageNumber: number, pageSize: number): Observable<PaginatedResult<Inspection>> {
    const params = new HttpParams().set('pageNumber', pageNumber).set('pageSize', pageSize);
    return this.http
      .get<ApiResponse<PaginatedResult<Inspection>>>(`${this.baseUrl}/my`, { params })
      .pipe(map((res) => res.data as PaginatedResult<Inspection>));
  }

  /** Records an inspection as multipart/form-data (fields + `attachments` files). */
  create(input: CreateInspectionInput): Observable<Inspection> {
    const form = new FormData();
    form.append('IncidentId', input.incidentId);
    form.append('Classification', input.classification);
    form.append('Comments', input.comments);
    for (const file of input.attachments) {
      form.append('attachments', file, file.name);
    }
    return this.http
      .post<ApiResponse<Inspection>>(this.baseUrl, form)
      .pipe(map((res) => res.data as Inspection));
  }
}
