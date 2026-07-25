/**
 * Mirrors the backend global response envelope (`ApiResponse<T>` /
 * `ApiResponseWrapperFilter`). Every API call returns this shape.
 */
export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T | null;
  errors: ApiErrorDetail[];
}

/** Mirrors the backend `ErrorDetails`. */
export interface ApiErrorDetail {
  code?: string;
  field?: string;
  message: string;
}
