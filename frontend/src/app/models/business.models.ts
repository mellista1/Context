export interface CreateBusinessRequest {
  name: string;
  description?: string;
  address: string;
}

export interface BusinessResponse {
  id: number;
  name: string;
  description?: string;
  address: string;
  isActive: boolean;
  createdAt: string;
}