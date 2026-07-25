/** Mirrors the backend `AuditLogResponse`. */
export interface AuditLogEntry {
  id: string;
  entityName: string;
  entityId?: string | null;
  action: string;
  oldValues?: string | null;
  newValues?: string | null;
  performedByUserId?: string | null;
  performedByName?: string | null;
  performedDate: string;
  ipAddress?: string | null;
  userAgent?: string | null;
}
