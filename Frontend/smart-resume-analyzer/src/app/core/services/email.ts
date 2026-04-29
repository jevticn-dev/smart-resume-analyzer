import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { EmailDraft, GenerateEmailDraftRequest, SendEmailRequest } from '../models/email.models';
import { ProjectDetail } from '../models/project.models';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class Email {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/email`;

  generateDraft(dto: GenerateEmailDraftRequest): Observable<EmailDraft> {
    return this.http.post<EmailDraft>(`${this.apiUrl}/generate-draft`, dto);
  }

  sendEmail(dto: SendEmailRequest): Observable<ProjectDetail> {
    return this.http.post<ProjectDetail>(`${this.apiUrl}/send`, dto);
  }
}