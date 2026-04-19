import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { PdfViewerModule } from 'ng2-pdf-viewer';
import { ButtonModule } from 'primeng/button';
import { SkeletonModule } from 'primeng/skeleton';
import { Navbar } from '../../../layout/navbar/navbar';
import { Project } from '../../../core/services/project';
import { ErrorHandler } from '../../../core/services/error-handler';
import { ProjectDetail as ProjectDetailModel } from '../../../core/models/project.models';
import { AnalysisResultPanel } from '../../../shared/analysis-result-panel/analysis-result-panel';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-project-detail',
  imports: [PdfViewerModule, ButtonModule, SkeletonModule, Navbar, AnalysisResultPanel],
  templateUrl: './project-detail.html',
  styleUrl: './project-detail.scss'
})
export class ProjectDetail implements OnInit {
  private route = inject(ActivatedRoute);
  private projectService = inject(Project);
  private errorHandler = inject(ErrorHandler);
  private router = inject(Router);

  project = signal<ProjectDetailModel | null>(null);
  isLoading = signal<boolean>(true);
  pdfUrl = signal<string | null>(null);

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id')!;
    this.projectService.getProject(id).subscribe({
      next: (data) => {
        this.project.set(data);
        if (data.cvVersion) {
          const token = localStorage.getItem('token');
          this.pdfUrl.set(`${environment.apiUrl}/project/${id}/cv?token=${token}`);
        }
        this.isLoading.set(false);
      },
      error: (err) => {
        this.errorHandler.handle(err);
        this.router.navigate(['/projects']);
      }
    });
  }

  runAnalysis(): void {
    const p = this.project();
    if (!p) return;
    this.router.navigate(['/analyze'], {
      queryParams: {
        projectId: p.id,
        jobTitle: p.jobTitle,
        companyName: p.companyName,
        jobDescription: p.jobDescription,
        seniorityLevel: p.seniority
      }
    });
  }

  goBack(): void {
    this.router.navigate(['/projects']);
  }
}