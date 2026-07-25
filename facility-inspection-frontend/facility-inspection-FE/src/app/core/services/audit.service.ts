import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

import { environment } from '../../../environments/environment';
import { ApiResponse, AuditLogEntry } from '../models';

/**
 * Reads audit-trail entries. The backend restricts these endpoints to Admins, so
 * callers should gate usage by role (a non-admin call returns 403).
 */
@Injectable({ providedIn: 'root' })
export class AuditService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/audit-logs`;

  getByEntity(entityName: string, entityId: string): Observable<AuditLogEntry[]> {
    return this.http
      .get<ApiResponse<AuditLogEntry[]>>(`${this.baseUrl}/by-entity/${entityName}/${entityId}`)
      .pipe(map((res) => res.data ?? []));
  }
}
