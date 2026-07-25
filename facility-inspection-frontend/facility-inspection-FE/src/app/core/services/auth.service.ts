import { computed, inject, Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable } from 'rxjs';
import { map, tap } from 'rxjs/operators';

import { environment } from '../../../environments/environment';
import { TokenService } from './token.service';
import {
  AdminCreateUserRequest,
  ApiResponse,
  AppRole,
  AuthResponse,
  ChangePasswordRequest,
  CreateUserResponse,
  CurrentUser,
  ForgotPasswordRequest,
  JwtClaims,
  LoginRequest,
  RegisterRequest,
  ResetPasswordRequest,
} from '../models';

/**
 * Authentication state + operations. Exposes reactive signals so guards, layouts
 * and features read identity/roles without subscriptions. The session (token +
 * user snapshot) is persisted by {@link TokenService} and rehydrated on reload.
 */
@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly tokenService = inject(TokenService);
  private readonly router = inject(Router);
  private readonly baseUrl = `${environment.apiUrl}/auth`;

  private readonly currentUserSig = signal<CurrentUser | null>(this.restoreSession());

  /** Current user, or null when signed out. */
  readonly currentUser = this.currentUserSig.asReadonly();
  /** True while a non-expired token and a resolved user exist. */
  readonly isAuthenticated = computed(
    () => this.currentUserSig() !== null && this.tokenService.hasValidToken(),
  );
  readonly roles = computed<AppRole[]>(() => this.currentUserSig()?.roles ?? []);
  readonly requiresPasswordChange = computed(
    () => this.currentUserSig()?.requiresPasswordChange === true,
  );

  /** `POST /api/auth/login`. Persists the session and resolves the user. */
  login(request: LoginRequest, remember = true): Observable<CurrentUser> {
    return this.http.post<ApiResponse<AuthResponse>>(`${this.baseUrl}/login`, request).pipe(
      map((res) => res.data as AuthResponse),
      tap((auth) => this.startSession(auth, remember)),
      map(() => this.currentUserSig()!),
    );
  }

  /**
   * `POST /api/auth/register`. Public self-service — the backend always creates
   * an Initiator. The response includes a token, so we start the session (auto
   * sign-in) just like login.
   */
  register(request: RegisterRequest, remember = true): Observable<CurrentUser> {
    return this.http.post<ApiResponse<AuthResponse>>(`${this.baseUrl}/register`, request).pipe(
      map((res) => res.data as AuthResponse),
      tap((auth) => this.startSession(auth, remember)),
      map(() => this.currentUserSig()!),
    );
  }

  /** Refreshes the full profile from `GET /api/auth/me`. */
  loadCurrentUser(): Observable<CurrentUser> {
    return this.http.get<ApiResponse<CurrentUser>>(`${this.baseUrl}/me`).pipe(
      map((res) => res.data as CurrentUser),
      tap((user) => {
        this.currentUserSig.set(user);
        this.tokenService.setUser(user);
      }),
    );
  }

  forgotPassword(request: ForgotPasswordRequest): Observable<string> {
    return this.http
      .post<ApiResponse<{ message: string }>>(`${this.baseUrl}/forgot-password`, request)
      .pipe(map((res) => res.message ?? res.data?.message ?? 'If an account exists, a reset link has been sent.'));
  }

  resetPassword(request: ResetPasswordRequest): Observable<string> {
    return this.http
      .post<ApiResponse<{ message: string }>>(`${this.baseUrl}/reset-password`, request)
      .pipe(map((res) => res.message ?? res.data?.message ?? 'Password updated.'));
  }

  changePassword(request: ChangePasswordRequest): Observable<CurrentUser> {
    return this.http.post<ApiResponse<AuthResponse>>(`${this.baseUrl}/change-password`, request).pipe(
      map((res) => res.data as AuthResponse),
      tap((auth) => this.startSession(auth, true)),
      map(() => this.currentUserSig()!),
    );
  }

  logout(redirect = true): void {
    this.tokenService.clear();
    this.currentUserSig.set(null);
    if (redirect) {
      void this.router.navigate(['/auth/login']);
    }
  }

  hasRole(role: AppRole): boolean {
    return this.roles().includes(role);
  }

  hasAnyRole(...roles: AppRole[]): boolean {
    const mine = this.roles();
    return roles.some((r) => mine.includes(r));
  }

  /** Route to use immediately after a successful sign-in or password change. */
  postAuthRoute(): CurrentUserLanding {
    if (this.requiresPasswordChange()) {
      return '/auth/change-password';
    }
    return this.landingRoute();
  }

  /** Default landing route after sign-in, chosen by role. */
  landingRoute(): CurrentUserLanding {
    if (this.hasRole(AppRole.Vendor)) {
      return '/work-orders';
    }
    return '/dashboard';
  }

  // --- internals -----------------------------------------------------------

  private startSession(auth: AuthResponse, remember: boolean): void {
    const user = this.toUser(auth);
    this.tokenService.setToken(auth.accessToken, remember);
    this.tokenService.setUser(user, remember);
    this.currentUserSig.set(user);
  }

  private toUser(auth: AuthResponse): CurrentUser {
    const [firstName, ...rest] = auth.fullName.split(' ');
    return {
      userId: auth.userId,
      email: auth.email,
      firstName: firstName ?? '',
      lastName: rest.join(' '),
      fullName: auth.fullName,
      sapId: auth.sapId,
      isActive: true,
      requiresPasswordChange: auth.requiresPasswordChange ?? false,
      roles: auth.roles,
    };
  }

  /** Rehydrates from the stored snapshot (preferred) or the JWT claims. */
  private restoreSession(): CurrentUser | null {
    if (!this.tokenService.hasValidToken()) {
      this.tokenService.clear();
      return null;
    }
    return this.tokenService.getUser() ?? this.userFromClaims();
  }

  private userFromClaims(): CurrentUser | null {
    const claims = this.tokenService.decode();
    if (!claims) {
      return null;
    }
    const name = (claims['unique_name'] as string) ?? (claims.email as string) ?? '';
    return {
      userId: (claims.sub ?? claims.nameid ?? '') as string,
      email: (claims.email as string) ?? '',
      firstName: '',
      lastName: '',
      fullName: name,
      sapId: '',
      isActive: true,
      requiresPasswordChange: false,
      roles: this.extractRoles(claims),
    };
  }

  private extractRoles(claims: JwtClaims): AppRole[] {
    const raw =
      claims.role ??
      (claims['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] as
        | string
        | string[]
        | undefined);
    if (!raw) {
      return [];
    }
    return (Array.isArray(raw) ? raw : [raw]) as AppRole[];
  }
}

type CurrentUserLanding = `/${string}`;
