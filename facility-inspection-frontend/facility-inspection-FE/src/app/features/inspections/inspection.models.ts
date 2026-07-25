/** Classification options for the inspection form (values match the backend enum). */
export const INSPECTION_CLASSIFICATIONS: { value: string; label: string }[] = [
  { value: 'Good', label: 'Good' },
  { value: 'Faulty', label: 'Faulty' },
  { value: 'RunDown', label: 'Run Down' },
  { value: 'Damaged', label: 'Damaged' },
];

export interface InspectionAttachment {
  id: string;
  fileName: string;
  contentType: string;
  fileSizeBytes: number;
  storagePath: string;
}

/** Mirrors the backend `InspectionResponse`. */
export interface Inspection {
  id: string;
  incidentId: string;
  incidentNumber?: string | null;
  inspectorUserId: string;
  inspectionDate: string;
  classification: string;
  comments: string;
  requiresVendor: boolean;
  incidentStatus?: string | null;
  createdBy?: string | null;
  createdDate: string;
  attachments: InspectionAttachment[];
}

/** Values captured by the inspection form (mapped to multipart on submit). */
export interface CreateInspectionInput {
  incidentId: string;
  classification: string;
  comments: string;
  attachments: File[];
}
