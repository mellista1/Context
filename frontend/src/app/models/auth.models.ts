export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  fullName: string;
  email: string;
  password: string;
}
export interface CurrentUser {
  id: string;
  email: string;
  fullName: string;
}

export interface AuthResponse {
  token: string;
  user: CurrentUser;
}