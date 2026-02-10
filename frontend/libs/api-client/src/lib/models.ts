// Auth Models
export interface LoginRequest {
  username: string;
  password: string;
}

export interface LoginResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  user: UserInfo;
}

export interface UserInfo {
  id: string;
  username: string;
  email: string;
  fullName: string;
}

export interface RegisterRequest {
  username: string;
  email: string;
  password: string;
  fullName: string;
  phoneNumber?: string;
}

export interface RegisterResponse {
  userId: string;
  username: string;
  email: string;
}

export interface RefreshTokenRequest {
  refreshToken: string;
}

export interface RefreshTokenResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
}

// Asset Models
export interface AssetDto {
  id: string;
  assetCode: string;
  name: string;
  categoryName: string;
  categoryType: string;
  purchasePrice: number;
  currentValue: number;
  status: string;
  purchaseDate: string;
  location?: string;
}

export interface CreateAssetRequest {
  assetCode: string;
  name: string;
  categoryId: string;
  description?: string;
  serialNumber?: string;
  manufacturer?: string;
  model?: string;
  purchaseDate: string;
  purchasePrice: number;
  location?: string;
  warrantyExpiry?: string;
  notes?: string;
}

export interface CreateAssetResponse {
  id: string;
  assetCode: string;
  name: string;
}

// Common Models
export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}
