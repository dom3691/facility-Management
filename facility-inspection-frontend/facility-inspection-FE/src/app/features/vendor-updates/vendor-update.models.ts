export interface VendorUpdateAttachment {
  id: string;
  fileName: string;
  contentType: string;
  fileSizeBytes: number;
  storagePath: string;
}

/** Mirrors the backend `VendorUpdateResponse`. */
export interface VendorUpdate {
  id: string;
  workOrderId: string;
  progressComment?: string | null;
  progressPercentage?: number | null;
  isCompletionUpdate: boolean;
  completionComment?: string | null;
  statusAtUpdate: string;
  updatedByUserId: string;
  updatedDate: string;
  attachments: VendorUpdateAttachment[];
}

/** Progress update payload (mapped to multipart on submit). */
export interface CreateVendorUpdateInput {
  workOrderId: string;
  progressComment: string;
  progressPercentage?: number | null;
  attachments: File[];
}

/** Completion payload (mapped to multipart on submit). */
export interface MarkCompleteInput {
  workOrderId: string;
  completionComment: string;
  attachments: File[];
}
