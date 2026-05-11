import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { PdfViewerModule } from 'ng2-pdf-viewer';
import { ButtonModule } from 'primeng/button';
import { SkeletonModule } from 'primeng/skeleton';
import { InputTextModule } from 'primeng/inputtext';
import { FormsModule } from '@angular/forms';
import { DialogModule } from 'primeng/dialog';
import { QuillModule } from 'ngx-quill';
import { Navbar } from '../../../layout/navbar/navbar';
import { Project } from '../../../core/services/project';
import { Email } from '../../../core/services/email';
import { ErrorHandler } from '../../../core/services/error-handler';
import { Toast } from '../../../core/services/toast';
import { CvVersionDetail, ProjectDetail as ProjectDetailModel } from '../../../core/models/project.models';
import { AnalysisResultPanel } from '../../../shared/analysis-result-panel/analysis-result-panel';
import { DatePipe } from '@angular/common';
import { ExportService } from '../../../core/services/export';

@Component({
  selector: 'app-version-detail',
  imports: [
    PdfViewerModule, ButtonModule, SkeletonModule, RouterLink,
    Navbar, AnalysisResultPanel, DatePipe, InputTextModule,
    FormsModule, DialogModule, QuillModule
  ],
  templateUrl: './version-detail.html',
  styleUrl: './version-detail.scss'
})
export class VersionDetail implements OnInit {
  private route = inject(ActivatedRoute);
  private projectService = inject(Project);
  private emailService = inject(Email);
  private errorHandler = inject(ErrorHandler);
  private toast = inject(Toast);
  private router = inject(Router);
  private exportService = inject(ExportService);

  version = signal<CvVersionDetail | null>(null);
  projectTitle = signal<string>('');
  projectCompanyEmail = signal<string>('');
  projectId = '';
  versionId = '';
  isLoading = signal<boolean>(true);
  pdfUrl = signal<string | null>(null);

  sendModalOpen = signal<boolean>(false);
  sendModalStep = signal<2 | 3>(2);
  isGeneratingDraft = signal<boolean>(false);
  isSendingEmail = signal<boolean>(false);

  deleteModalOpen = signal<boolean>(false);
  isDeleting = signal<boolean>(false);

  emailTo = '';
  emailSubject = '';
  emailBody = '';

  quillModules = {
    toolbar: [
      ['bold', 'italic', 'underline'],
      [{ list: 'ordered' }, { list: 'bullet' }],
      ['clean']
    ]
  };

  ngOnInit(): void {
    this.projectId = this.route.snapshot.paramMap.get('id')!;
    this.versionId = this.route.snapshot.paramMap.get('versionId')!;

    this.projectService.getProject(this.projectId).subscribe({
      next: (data) => {
        this.projectTitle.set(data.title);
        this.projectCompanyEmail.set(data.companyEmail ?? '');
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

  getScoreColor(score: number): string {
    if (score >= 70) return '#10B981';
    if (score >= 40) return '#F59E0B';
    return '#EF4444';
  }

  openSendModal(): void {
    this.sendModalOpen.set(true);
    this.sendModalStep.set(2);
    this.emailTo = this.projectCompanyEmail() ?? '';
    this.emailSubject = '';
    this.emailBody = '';
  }

  closeSendModal(): void {
    this.sendModalOpen.set(false);
    this.sendModalStep.set(2);
    this.isGeneratingDraft.set(false);
  }

  generateDraft(): void {
    this.isGeneratingDraft.set(true);
    this.emailService.generateDraft({
      projectId: this.projectId,
      versionId: this.versionId
    }).subscribe({
      next: (draft) => {
        this.emailSubject = draft.subject;
        this.emailBody = draft.body;
        this.isGeneratingDraft.set(false);
        this.sendModalStep.set(3);
      },
      error: (err) => {
        this.errorHandler.handle(err);
        this.isGeneratingDraft.set(false);
      }
    });
  }

  writeManually(): void {
    this.emailSubject = '';
    this.emailBody = '';
    this.sendModalStep.set(3);
  }

  submitEmail(): void {
    const version = this.version();
    if (!version || !this.emailTo || !this.emailSubject || !this.emailBody) return;

    this.isSendingEmail.set(true);
    this.emailService.sendEmail({
      projectId: this.projectId,
      versionId: this.versionId,
      toEmail: this.emailTo,
      subject: this.emailSubject,
      body: this.emailBody
    }).subscribe({
      next: () => {
        this.isSendingEmail.set(false);
        this.closeSendModal();
        this.toast.success('Application sent successfully.');
      },
      error: (err) => {
        this.errorHandler.handle(err);
        this.isSendingEmail.set(false);
      }
    });
  }

  exportPdf(): void {
    const version = this.version();
    if (!version) return;
    this.projectService.getProject(this.projectId).subscribe({
      next: (project) => {
        this.exportService.exportAnalysisPdf(project, version);
      },
      error: (err) => this.errorHandler.handle(err)
    });
  }

  confirmDeleteVersion(): void {
    this.deleteModalOpen.set(true);
  }

  cancelDelete(): void {
    this.deleteModalOpen.set(false);
  }

  deleteVersion(): void {
    this.isDeleting.set(true);
    this.projectService.deleteCvVersion(this.projectId, this.versionId).subscribe({
      next: () => {
        this.isDeleting.set(false);
        this.deleteModalOpen.set(false);
        this.toast.success('CV version deleted successfully.');
        this.router.navigate(['/projects', this.projectId]);
      },
      error: (err) => {
        this.errorHandler.handle(err);
        this.isDeleting.set(false);
      }
    });
  }
}