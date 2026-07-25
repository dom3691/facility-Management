/** Mirrors the backend `VendorResponse`. */
export interface Vendor {
  id: string;
  vendorName: string;
  vendorCategory: string;
  contactPerson?: string | null;
  email?: string | null;
  phoneNumber?: string | null;
  isActive: boolean;
  createdDate: string;
}

/** Vendor category options (values match the backend `VendorCategory` enum). */
export const VENDOR_CATEGORIES: { value: string; label: string }[] = [
  { value: 'InHouse', label: 'In House' },
  { value: 'Leadway', label: 'Leadway' },
  { value: 'ExternalVendor', label: 'External Vendor' },
];

export function vendorCategoryLabel(value: string | null | undefined): string {
  return VENDOR_CATEGORIES.find((c) => c.value === value)?.label ?? (value ?? '');
}

/**
 * Category options for create/update forms. The backend binds the request body's
 * enum from a NUMBER (no string-enum converter), so the form value is the int id;
 * `name` is the string returned by GET responses (used to pre-fill on edit).
 */
export const VENDOR_CATEGORY_INT_OPTIONS: { value: number; name: string; label: string }[] = [
  { value: 1, name: 'InHouse', label: 'In House' },
  { value: 2, name: 'Leadway', label: 'Leadway' },
  { value: 3, name: 'ExternalVendor', label: 'External Vendor' },
];

export function categoryNameToInt(name: string | null | undefined): number | null {
  return VENDOR_CATEGORY_INT_OPTIONS.find((o) => o.name === name)?.value ?? null;
}
