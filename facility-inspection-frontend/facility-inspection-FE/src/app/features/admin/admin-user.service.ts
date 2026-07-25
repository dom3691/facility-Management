import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

import { environment } from '../../../environments/environment';
import { AdminCreateUserRequest, ApiResponse, CreateUserResponse } from '../../core/models';

/** Admin user provisioning (`POST /api/auth/users`). Password is auto-generated server-side. */
@Injectable({ providedIn: 'root' })
export class AdminUserService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/auth`;

  create(input: AdminCreateUserRequest): Observable<CreateUserResponse> {
    return this.http
      .post<ApiResponse<CreateUserResponse>>(`${this.baseUrl}/users`, input)
      .pipe(map((res) => res.data as CreateUserResponse));
  }
}
