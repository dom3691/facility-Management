/** Mirrors the backend `PaginatedResult<T>`. */
export interface PaginatedResult<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

/** Common query params for paged list endpoints. */
export interface PageQuery {
  pageNumber?: number;
  pageSize?: number;
}
