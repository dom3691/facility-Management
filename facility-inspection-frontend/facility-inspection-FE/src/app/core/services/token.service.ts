import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { CurrentUser, JwtClaims } from '../models';

/**
 * Owns session persistence: the JWT access token **and** the user snapshot.
 *
 * The backend JWT only carries `sub`, `unique_name` and role claims — not the
 * user's name/email/SAP id — so the resolved user is stored alongside the token
 * to survive a page reload.
 *
 * "Remember me" chooses the backing store: localStorage (persists across browser
 * restarts) vs. sessionStorage (cleared when the tab closes).
 */
@Injectable({ providedIn: 'root' })
export class TokenService {
  private readonly tokenKey = environment.tokenStorageKey;
  private readonly userKey = `${environment.tokenStorageKey}.user`;

  // --- Token ---------------------------------------------------------------

  getToken(): string | null {
    return localStorage.getItem(this.tokenKey) ?? sessionStorage.getItem(this.tokenKey);
  }

  setToken(token: string, remember = true): void {
    this.remove(this.tokenKey);
    this.store(remember).setItem(this.tokenKey, token);
  }

  // --- User snapshot -------------------------------------------------------

  getUser(): CurrentUser | null {
    const raw = localStorage.getItem(this.userKey) ?? sessionStorage.getItem(this.userKey);
    if (!raw) {
      return null;
    }
    try {
      return JSON.parse(raw) as CurrentUser;
    } catch {
      return null;
    }
  }

  setUser(user: CurrentUser, remember = true): void {
    this.remove(this.userKey);
    this.store(remember).setItem(this.userKey, JSON.stringify(user));
  }

  // --- Lifecycle -----------------------------------------------------------

  clear(): void {
    this.remove(this.tokenKey);
    this.remove(this.userKey);
  }

  // --- JWT helpers ---------------------------------------------------------

  decode(token: string | null = this.getToken()): JwtClaims | null {
    if (!token) {
      return null;
    }
    try {
      const payload = token.split('.')[1];
      const json = atob(payload.replace(/-/g, '+').replace(/_/g, '/'));
      return JSON.parse(decodeURIComponent(escape(json))) as JwtClaims;
    } catch {
      return null;
    }
  }

  isExpired(token: string | null = this.getToken()): boolean {
    const claims = this.decode(token);
    if (!claims?.exp) {
      return true;
    }
    return Date.now() >= claims.exp * 1000 - 5_000; // 5s skew allowance
  }

  hasValidToken(): boolean {
    return !!this.getToken() && !this.isExpired();
  }

  // --- internals -----------------------------------------------------------

  private store(remember: boolean): Storage {
    return remember ? localStorage : sessionStorage;
  }

  private remove(key: string): void {
    localStorage.removeItem(key);
    sessionStorage.removeItem(key);
  }
}
