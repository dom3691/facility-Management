import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

import { environment } from '../../../environments/environment';
import { ApiResponse } from '../../core/models';
import { DashboardSummary } from './dashboard.models';

/** Reads the role-scoped dashboard summary from the backend. */
@Injectable({ providedIn: 'root' })
export class DashboardService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/dashboard`;

  getSummary(): Observable<DashboardSummary> {
    return this.http
      .get<ApiResponse<DashboardSummary>>(`${this.baseUrl}/summary`)
      .pipe(map((res) => res.data as DashboardSummary));
  }
}
