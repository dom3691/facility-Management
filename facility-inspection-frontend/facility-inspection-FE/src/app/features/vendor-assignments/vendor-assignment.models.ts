/** Mirrors the backend `VendorAssignmentResponse`. */
export interface VendorAssignment {
  id: string;
  incidentId: string;
  incidentNumber?: string | null;
  inspectionId: string;
  vendorId: string;
  vendorName?: string | null;
  vendorCategory: string;
  assignedByUserId: string;
  assignedDate: string;
  notes?: string | null;
  incidentStatus?: string | null;
  createdDate: string;
}

/** Payload for `POST /api/vendor-assignments` (bound to CreateVendorAssignmentCommand). */
export interface CreateVendorAssignmentInput {
  incidentId: string;
  inspectionId: string;
  vendorId: string;
  vendorCategory: string;
  notes?: string;
}
