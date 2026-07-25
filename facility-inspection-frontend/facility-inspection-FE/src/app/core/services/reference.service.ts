import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

import { environment } from '../../../environments/environment';
import { ApiResponse, EnumOption, Facility, Location } from '../models';

export interface CreateFacilityInput {
  name: string;
  code: string;
  isActive: boolean;
}

export interface CreateLocationInput {
  facilityId: string;
  name: string;
  code?: string;
  isActive: boolean;
}

/** Reference/lookup data shared by feature forms (facilities, locations, enums). */
@Injectable({ providedIn: 'root' })
export class ReferenceService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/reference`;

  getFacilities(): Observable<Facility[]> {
    return this.http
      .get<ApiResponse<Facility[]>>(`${this.baseUrl}/facilities`)
      .pipe(map((res) => res.data ?? []));
  }

  getLocations(facilityId?: string): Observable<Location[]> {
    let params = new HttpParams();
    if (facilityId) {
      params = params.set('facilityId', facilityId);
    }
    return this.http
      .get<ApiResponse<Location[]>>(`${this.baseUrl}/locations`, { params })
      .pipe(map((res) => res.data ?? []));
  }

  createFacility(input: CreateFacilityInput): Observable<Facility> {
    return this.http
      .post<ApiResponse<Facility>>(`${this.baseUrl}/facilities`, input)
      .pipe(map((res) => res.data as Facility));
  }

  createLocation(input: CreateLocationInput): Observable<Location> {
    return this.http
      .post<ApiResponse<Location>>(`${this.baseUrl}/locations`, input)
      .pipe(map((res) => res.data as Location));
  }

  /** Enum lookups, e.g. `getEnumOptions('vendor-categories')`. */
  getEnumOptions(kind: string): Observable<EnumOption[]> {
    return this.http
      .get<ApiResponse<EnumOption[]>>(`${this.baseUrl}/${kind}`)
      .pipe(map((res) => res.data ?? []));
  }
}
