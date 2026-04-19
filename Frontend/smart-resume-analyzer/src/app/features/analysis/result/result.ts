import { Component, OnInit, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { Navbar } from '../../../layout/navbar/navbar';
import { Auth } from '../../../core/services/auth';
import { Project } from '../../../core/services/project';
import { ErrorHandler } from '../../../core/services/error-handler';
import { AnalysisResult } from '../../../core/models/analysis.models';
import { AnalysisResultPanel } from '../../../shared/analysis-result-panel/analysis-result-panel';
import { ProgressSpinnerModule } from 'primeng/progressspinner';

@Component({
  selector: 'app-result',
  imports: [ButtonModule, Navbar, AnalysisResultPanel, ProgressSpinnerModule],
  templateUrl: './result.html',
  styleUrl: './result.scss'
})
export class Result implements OnInit {
  router = inject(Router);
  auth = inject(Auth);
  private projectService = inject(Project);
  private errorHandler = inject(ErrorHandler);

  result = signal<AnalysisResult | null>(null);
  isConverting = signal<boolean>(false);

  ngOnInit(): void {
    const state = history.state as { result?: AnalysisResult; pendingAnalysisLogId?: string };

    if (state?.pendingAnalysisLogId) {
      this.convertAndRedirect(state.pendingAnalysisLogId);
      return;
    }

    if (state?.result) {
      this.result.set(state.result);

      if (this.auth.isAuthenticated() && state.result.projectId) {
        this.router.navigate(['/projects', state.result.projectId]);
        return;
      }

      if (!this.auth.isAuthenticated() && state.result.analysisLogId) {
        sessionStorage.setItem('pendingAnalysisLogId', state.result.analysisLogId);
      }

      return;
    }

    this.router.navigate(['/analyze']);
  }

  private convertAndRedirect(analysisLogId: string): void {
    this.isConverting.set(true);
    this.projectService.convertGuestAnalysis(analysisLogId).subscribe({
      next: (project) => {
        this.router.navigate(['/projects', project.id]);
      },
      error: (err) => {
        this.isConverting.set(false);
        this.errorHandler.handle(err);
        this.router.navigate(['/analyze']);
      }
    });
  }

  analyzeAnother(): void {
    this.router.navigate(['/analyze']);
  }
}