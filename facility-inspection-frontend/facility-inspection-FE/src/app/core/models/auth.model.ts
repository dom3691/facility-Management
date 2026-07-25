import { AppRole } from './role.model';

/** Payload for `POST /api/auth/login`. */
export interface LoginRequest {
  email: string;
  password: string;
}

/** Payload for `POST /api/auth/register` and Admin `POST /api/auth/users`. */
export interface RegisterRequest {
  firstName: string;
  lastName: string;
  sapId: string;
  email: string;
  phoneNumber?: string;
  password: string;
  /** Honoured only by the Admin `users` endpoint; ignored by public register. */
  role?: AppRole;
}

/** Response `data` from login/register/change-password (`AuthResponse`). */
export interface AuthResponse {
  accessToken: string;
  expiresAtUtc: string;
  userId: string;
  email: string;
  fullName: string;
  sapId: string;
  roles: AppRole[];
  requiresPasswordChange: boolean;
}

/** Admin `POST /api/auth/users` — password is generated server-side. */
export interface AdminCreateUserRequest {
  firstName: string;
  lastName: string;
  sapId: string;
  email: string;
  phoneNumber?: string;
  role: AppRole;
  vendorId?: string;
}

export interface CreateUserResponse {
  userId: string;
  email: string;
  fullName: string;
  roles: AppRole[];
  invitationEmailSent: boolean;
}

export interface ForgotPasswordRequest {
  email: string;
}

export interface ResetPasswordRequest {
  email: string;
  token: string;
  newPassword: string;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
}

/** Decoded JWT claims of interest (names align with the backend token generator). */
export interface JwtClaims {
  sub?: string;
  nameid?: string;
  email?: string;
  role?: string | string[];
  exp?: number;
  [claim: string]: unknown;
}
