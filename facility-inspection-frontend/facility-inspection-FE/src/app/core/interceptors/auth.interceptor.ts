import { inject } from '@angular/core';
import { HttpInterceptorFn } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { TokenService } from '../services/token.service';

/**
 * Attaches `Authorization: Bearer <token>` to outgoing calls that target our API.
 * External URLs (e.g. Google Fonts) are left untouched so the token never leaks.
 */
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const token = inject(TokenService).getToken();
  const isApiCall = req.url.startsWith(environment.apiUrl);

  if (token && isApiCall && !req.headers.has('Authorization')) {
    req = req.clone({ setHeaders: { Authorization: `Bearer ${token}` } });
  }

  return next(req);
};
