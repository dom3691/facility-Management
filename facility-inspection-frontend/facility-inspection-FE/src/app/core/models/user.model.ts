import { AppRole } from './role.model';

/** Mirrors the backend `CurrentUserResponse` (`GET /api/auth/me`). */
export interface CurrentUser {
  userId: string;
  email: string;
  firstName: string;
  lastName: string;
  fullName: string;
  sapId: string;
  phoneNumber?: string | null;
  isActive: boolean;
  requiresPasswordChange: boolean;
  roles: AppRole[];
}
