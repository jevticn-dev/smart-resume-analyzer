export interface RegisterRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface AuthResponse {
  token: string;
  email: string;
  firstName: string;
  lastName: string;
}

export interface UserStats {
  totalProjects: number;
  totalCvVersions: number;
  totalAnalyses: number;
  sentApplications: number;
  acceptedApplications: number;
  declinedApplications: number;
  averageMatchScore: number;
  remainingAnalysesToday: number;
}

export interface UserProfile {
  email: string;
  firstName: string;
  lastName: string;
  reminderIntervalDays: number;
  stats: UserStats;
}

export interface UpdateProfile {
  firstName: string;
  lastName: string;
  reminderIntervalDays: number;
}

export interface ChangePassword {
  currentPassword: string;
  newPassword: string;
}