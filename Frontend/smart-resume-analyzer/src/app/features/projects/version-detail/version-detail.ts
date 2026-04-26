import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { PdfViewerModule } from 'ng2-pdf-viewer';
import { ButtonModule } from 'primeng/button';
import { SkeletonModule } from 'primeng/skeleton';
import { Navbar } from '../../../layout/navbar/navbar';
import { Project } from '../../../core/services/project';
import { ErrorHandler } from '../../../core/services/error-handler';
import { CvVersionDetail } from '../../../core/models/project.models';
import { AnalysisResultPanel } from '../../../shared/analysis-result-panel/analysis-result-panel';
import { DatePipe } from '@angular/common';

@Component({
  selector: 'app-version-detail',
  imports: [PdfViewerModule, ButtonModule, SkeletonModule, RouterLink, Navbar, AnalysisResultPanel, DatePipe],
  templateUrl: './version-detail.html',
  styleUrl: './version-detail.scss'
})
export class VersionDetail implements OnInit {
  private route = inject(ActivatedRoute);
  private projectService = inject(Project);
  private errorHandler = inject(ErrorHandler);
  private router = inject(Router);

  version = signal<CvVersionDetail | null>(null);
  projectTitle = signal<string>('');
  isLoading = signal<boolean>(true);
  pdfUrl = signal<string | null>(null);

  projectId = '';
  versionId = '';

  ngOnInit(): void {
    this.projectId = this.route.snapshot.paramMap.get('id')!;
    this.versionId = this.route.snapshot.paramMap.get('versionId')!;

    this.projectService.getProject(this.projectId).subscribe({
      next: (data) => {
        this.projectTitle.set(data.title);
        const found = data.cvVersions.find(v => v.id === this.versionId) ?? null;
        this.version.set(found);
        if (found) {
          this.pdfUrl.set(this.projectService.getCvVersionUrl(this.projectId, this.versionId));
        }
        this.isLoading.set(false);
      },
      error: (err) => {
        this.errorHandler.handle(err);
        this.router.navigate(['/projects', this.projectId]);
      }
    });
  }

  goToProject(): void {
    this.router.navigate(['/projects', this.projectId]);
  }
}