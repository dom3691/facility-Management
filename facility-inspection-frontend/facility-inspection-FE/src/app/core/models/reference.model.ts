/** A facility option (`GET /api/reference/facilities`). */
export interface Facility {
  id: string;
  name: string;
  code: string;
  isActive: boolean;
}

/** A location option (`GET /api/reference/locations?facilityId=`). */
export interface Location {
  id: string;
  facilityId: string;
  name: string;
  code?: string | null;
  isActive: boolean;
}

/** An enum lookup option (`GET /api/reference/{enum}`) — numeric id + name. */
export interface EnumOption {
  id: number;
  name: string;
}
