import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthResponse, ChangePassword, LoginRequest, RegisterRequest, UpdateProfile, UserProfile } from '../models/auth.models';

@Injectable({
  providedIn: 'root'
})
export class Auth {
  private readonly apiUrl = `${environment.apiUrl}/auth`;

  currentUser = signal<UserProfile | null>(null);
  isAuthenticated = signal<boolean>(false);

  constructor(private http: HttpClient, private router: Router) {
    const token = localStorage.getItem('token');
    if (token) {
      this.isAuthenticated.set(true);
    }
  }

  register(request: RegisterRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/register`, request).pipe(
      tap(response => this.handleAuthSuccess(response))
    );
  }

  login(request: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/login`, request).pipe(
      tap(response => this.handleAuthSuccess(response))
    );
  }

  loadCurrentUser(): Observable<UserProfile> {
    return this.http.get<UserProfile>(`${this.apiUrl}/me`).pipe(
      tap(user => this.currentUser.set(user))
    );
  }

  logout(): void {
    localStorage.removeItem('token');
    this.currentUser.set(null);
    this.isAuthenticated.set(false);
    this.router.navigate(['/']);
  }

  getToken(): string | null {
    return localStorage.getItem('token');
  }

  private handleAuthSuccess(response: AuthResponse): void {
    localStorage.setItem('token', response.token);
    this.currentUser.set({
      email: response.email,
      firstName: response.firstName,
      lastName: response.lastName,
      reminderIntervalDays: 7,
      stats: {
        totalProjects: 0,
        totalCvVersions: 0,
        totalAnalyses: 0,
        sentApplications: 0,
        acceptedApplications: 0,
        declinedApplications: 0,
        averageMatchScore: 0,
        remainingAnalysesToday: 0
      }
    });
    this.isAuthenticated.set(true);

    const pendingForm = sessionStorage.getItem('pendingAnalysisForm');
    const pendingLogId = sessionStorage.getItem('pendingAnalysisLogId');

    if (pendingForm) {
      sessionStorage.removeItem('pendingAnalysisLogId');
      this.router.navigate(['/analyze']);
    } else if (pendingLogId) {
      sessionStorage.removeItem('pendingAnalysisLogId');
      this.router.navigate(['/analysis/result'], { state: { pendingAnalysisLogId: pendingLogId } });
    } else {
      this.router.navigate(['/dashboard']);
    }
  }

  updateProfile(dto: UpdateProfile): Observable<UserProfile> {
    return this.http.put<UserProfile>(`${this.apiUrl}/profile`, dto).pipe(
      tap(user => this.currentUser.set(user))
    );
  }

  changePassword(dto: ChangePassword): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/change-password`, dto);
  }
}