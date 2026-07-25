import { IncidentStatus } from '../../core/models';

/** Row shape from `GET /api/incidents` and `/my` (`IncidentListResponse`). */
export interface IncidentListItem {
  id: string;
  incidentNumber: string;
  businessUnit: string;
  facilityName?: string | null;
  locationName?: string | null;
  incidentDate: string;
  status: IncidentStatus;
  createdDate: string;
}

/** A stored attachment (`AttachmentResponse`). */
export interface IncidentAttachment {
  id: string;
  fileName: string;
  contentType: string;
  fileSizeBytes: number;
  storagePath: string;
}

/** Full detail from create + get-by-id (`IncidentResponse`). */
export interface IncidentDetail {
  id: string;
  incidentNumber: string;
  businessUnit: string;
  sapId: string;
  facilityId: string;
  facilityName?: string | null;
  locationId: string;
  locationName?: string | null;
  incidentDate: string;
  description: string;
  status: IncidentStatus;
  reportedByUserId: string;
  createdBy?: string | null;
  createdDate: string;
  attachments: IncidentAttachment[];
}

/** Values captured by the create form (mapped to multipart on submit). */
export interface CreateIncidentInput {
  businessUnit: string;
  sapId: string;
  facilityId: string;
  locationId: string;
  incidentDate: string; // ISO 8601
  description: string;
  attachments: File[];
}

/** Business units offered in the create form (free-text on the backend). */
export const BUSINESS_UNITS = ['Operations', 'Facilities', 'HR', 'IT', 'Finance', 'Logistics'] as const;
