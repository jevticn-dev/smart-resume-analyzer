import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CompareVersions, ConvertGuestAnalysis, CreateProject, ProjectDetail, ProjectSummary, UpdateProject } from '../models/project.models';

@Injectable({
  providedIn: 'root'
})
export class Project {
  private readonly apiUrl = `${environment.apiUrl}/project`;

  constructor(private http: HttpClient) {}

  getProjects(): Observable<ProjectSummary[]> {
    return this.http.get<ProjectSummary[]>(this.apiUrl);
  }

  getProject(id: string): Observable<ProjectDetail> {
    return this.http.get<ProjectDetail>(`${this.apiUrl}/${id}`);
  }

  createProject(dto: CreateProject): Observable<ProjectDetail> {
    return this.http.post<ProjectDetail>(this.apiUrl, dto);
  }

  updateProject(id: string, dto: UpdateProject): Observable<ProjectDetail> {
    return this.http.put<ProjectDetail>(`${this.apiUrl}/${id}`, dto);
  }

  addCvVersion(projectId: string, file: File, notes: string): Observable<ProjectDetail> {
    const formData = new FormData();
    formData.append('cvFile', file);
    formData.append('notes', notes);
    return this.http.post<ProjectDetail>(`${this.apiUrl}/${projectId}/versions`, formData);
  }

  getCvVersionUrl(projectId: string, versionId: string): string {
    const token = localStorage.getItem('token');
    return `${this.apiUrl}/${projectId}/versions/${versionId}/cv?token=${token}`;
  }

  compareVersions(projectId: string, versionAId: string, versionBId: string): Observable<CompareVersions> {
    return this.http.get<CompareVersions>(`${this.apiUrl}/${projectId}/compare`, {
      params: { versionAId, versionBId }
    });
  }

  deleteProject(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  deleteCvVersion(projectId: string, versionId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${projectId}/versions/${versionId}`);
  }

  convertGuestAnalysis(analysisLogId: string): Observable<ProjectDetail> {
    const dto: ConvertGuestAnalysis = { analysisLogId };
    return this.http.post<ProjectDetail>(`${this.apiUrl}/from-guest-analysis`, dto);
  }
}