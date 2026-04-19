import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ConvertGuestAnalysis, CreateProject, ProjectDetail, ProjectSummary } from '../models/project.models';

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

  deleteProject(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  convertGuestAnalysis(analysisLogId: string): Observable<ProjectDetail> {
    const dto: ConvertGuestAnalysis = { analysisLogId };
    return this.http.post<ProjectDetail>(`${this.apiUrl}/from-guest-analysis`, dto);
  }
}