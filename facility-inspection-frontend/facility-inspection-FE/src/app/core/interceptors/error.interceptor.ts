import { inject } from '@angular/core';
import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { catchError, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { ApiResponse } from '../models';

/** Normalised error surfaced to features (from the API envelope where possible). */
export interface NormalizedHttpError {
  status: number;
  message: string;
  fieldErrors: Record<string, string[]>;
}

/**
 * Centralised HTTP error handling:
 * - 401 → clear session and bounce to login.
 * - otherwise → unwrap the backend `ApiResponse` error envelope into a
 *   consistent shape and rethrow, so callers can show messages/field errors.
 */
export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401) {
        auth.logout();
      }
      return throwError(() => normalize(error));
    }),
  );
};

function normalize(error: HttpErrorResponse): NormalizedHttpError {
  const body = error.error as ApiResponse<unknown> | string | null;
  const fieldErrors: Record<string, string[]> = {};
  let message = error.message;

  if (body && typeof body === 'object' && 'errors' in body) {
    message = body.message || message;
    for (const detail of body.errors ?? []) {
      const key = detail.field ?? '_';
      (fieldErrors[key] ??= []).push(detail.message);
    }
  } else if (typeof body === 'string' && body) {
    message = body;
  }

  if (error.status === 0) {
    message = 'Cannot reach the server. Check that the API is running and CORS is configured.';
  }

  return { status: error.status, message, fieldErrors };
}
