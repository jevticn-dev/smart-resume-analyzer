import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AnalysisResult } from '../models/analysis.models';

@Injectable({
  providedIn: 'root'
})
export class Analysis {
  private readonly apiUrl = `${environment.apiUrl}/analysis`;

  constructor(private http: HttpClient) {}

  analyze(formData: FormData): Observable<AnalysisResult> {
    return this.http.post<AnalysisResult>(`${this.apiUrl}/analyze`, formData);
  }
}