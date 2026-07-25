/** Mirrors the backend `VerificationResponse`. */
export interface Verification {
  id: string;
  workOrderId: string;
  workOrderNumber?: string | null;
  incidentId: string;
  incidentNumber?: string | null;
  verifiedByUserId: string;
  verificationDate: string;
  decision: string;
  comments?: string | null;
  workOrderStatus?: string | null;
  incidentStatus?: string | null;
  createdDate: string;
}

/** Payload for `POST /api/verifications` (bound to CreateVerificationCommand). */
export interface CreateVerificationInput {
  workOrderId: string;
  decision: string;
  comments?: string;
}

export const VERIFICATION_DECISIONS: { value: string; label: string }[] = [
  { value: 'Fixed', label: 'Fixed' },
  { value: 'NotFixed', label: 'Not Fixed' },
];
