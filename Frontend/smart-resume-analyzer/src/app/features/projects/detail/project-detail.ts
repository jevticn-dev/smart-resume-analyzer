import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { PdfViewerModule } from 'ng2-pdf-viewer';
import { ButtonModule } from 'primeng/button';
import { SkeletonModule } from 'primeng/skeleton';
import { TagModule } from 'primeng/tag';
import { DialogModule } from 'primeng/dialog';
import { TextareaModule } from 'primeng/textarea';
import { FormsModule } from '@angular/forms';
import { NgClass, DatePipe } from '@angular/common';
import { Navbar } from '../../../layout/navbar/navbar';
import { Project } from '../../../core/services/project';
import { ErrorHandler } from '../../../core/services/error-handler';
import { Toast } from '../../../core/services/toast';
import { ProjectDetail as ProjectDetailModel, CvVersionDetail } from '../../../core/models/project.models';
import { AnalysisResultPanel } from '../../../shared/analysis-result-panel/analysis-result-panel';

@Component({
  selector: 'app-project-detail',
  imports: [PdfViewerModule, ButtonModule, SkeletonModule, TagModule, DialogModule, TextareaModule, FormsModule, NgClass, Navbar, AnalysisResultPanel, DatePipe],
  templateUrl: './project-detail.html',
  styleUrl: './project-detail.scss'
})
export class ProjectDetail implements OnInit {
  private route = inject(ActivatedRoute);
  private projectService = inject(Project);
  private errorHandler = inject(ErrorHandler);
  private toast = inject(Toast);
  private router = inject(Router);

  project = signal<ProjectDetailModel | null>(null);
  isLoading = signal<boolean>(true);
  projectId = '';

  uploadModalOpen = signal<boolean>(false);
  uploadNotes = '';
  selectedFile = signal<File | null>(null);
  isDragging = signal<boolean>(false);
  isUploading = signal<boolean>(false);

  compareMode = signal<boolean>(false);
  selectedVersionIds = signal<string[]>([]);
  descriptionExpanded = signal<boolean>(false);

  ngOnInit(): void {
    this.projectId = this.route.snapshot.paramMap.get('id')!;
    this.loadProject();
  }

  loadProject(): void {
    this.isLoading.set(true);
    this.projectService.getProject(this.projectId).subscribe({
      next: (data) => {
        this.project.set(data);
        this.isLoading.set(false);
      },
      error: (err) => {
        this.errorHandler.handle(err);
        this.router.navigate(['/projects']);
      }
    });
  }

  getScoreColor(score: number): string {
    if (score >= 70) return '#10B981';
    if (score >= 40) return '#F59E0B';
    return '#EF4444';
  }

  getScoreDashOffset(score: number, radius: number): number {
    const circumference = 2 * Math.PI * radius;
    return circumference * (1 - score / 100);
  }

  getIndustryKey(industry: string): string {
    return industry.toLowerCase().replace(/[\/\s]/g, '');
  }

  versionsWithAnalysis = computed(() =>
    (this.project()?.cvVersions ?? [])
      .filter(v => v.analysis !== null)
      .slice()
      .sort((a, b) => a.versionNumber - b.versionNumber)
  );

  navigateToVersion(versionId: string): void {
    if (this.compareMode()) return;
    this.router.navigate(['/projects', this.projectId, 'versions', versionId]);
  }

  goBack(): void {
    this.router.navigate(['/projects']);
  }

  editProject(): void {
    this.router.navigate(['/projects', this.projectId, 'edit']);
  }

  openUploadModal(): void {
    this.uploadModalOpen.set(true);
    this.uploadNotes = '';
    this.selectedFile.set(null);
  }

  closeUploadModal(): void {
    this.uploadModalOpen.set(false);
  }

  onFileDrop(event: DragEvent): void {
    event.preventDefault();
    this.isDragging.set(false);
    const file = event.dataTransfer?.files[0];
    if (file && file.type === 'application/pdf') {
      this.selectedFile.set(file);
    }
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    this.isDragging.set(true);
  }

  onDragLeave(): void {
    this.isDragging.set(false);
  }

  onFileSelect(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (file) this.selectedFile.set(file);
  }

  submitUpload(): void {
    const file = this.selectedFile();
    if (!file) return;

    this.isUploading.set(true);
    this.projectService.addCvVersion(this.projectId, file, this.uploadNotes).subscribe({
      next: () => {
        this.isUploading.set(false);
        this.uploadModalOpen.set(false);
        this.toast.success('CV version uploaded successfully.');
        this.loadProject();
      },
      error: (err) => {
        this.errorHandler.handle(err);
        this.isUploading.set(false);
      }
    });
  }

  toggleCompareMode(): void {
    this.compareMode.update(v => !v);
    this.selectedVersionIds.set([]);
  }

  toggleVersionSelection(versionId: string): void {
    const current = this.selectedVersionIds();
    if (current.includes(versionId)) {
      this.selectedVersionIds.set(current.filter(id => id !== versionId));
    } else if (current.length < 2) {
      this.selectedVersionIds.set([...current, versionId]);
    }
  }

  isVersionSelected(versionId: string): boolean {
    return this.selectedVersionIds().includes(versionId);
  }

  confirmCompare(): void {
    const ids = this.selectedVersionIds();
    if (ids.length !== 2) return;
    this.router.navigate(['/projects', this.projectId, 'compare'], {
      queryParams: { versionAId: ids[0], versionBId: ids[1] }
    });
  }

  getHighSuggestions(version: CvVersionDetail): number {
    return version.analysis?.suggestions.filter(s => s.priority === 'high').length ?? 0;
  }

  getMediumSuggestions(version: CvVersionDetail): number {
    return version.analysis?.suggestions.filter(s => s.priority === 'medium').length ?? 0;
  }

  getLowSuggestions(version: CvVersionDetail): number {
    return version.analysis?.suggestions.filter(s => s.priority === 'low').length ?? 0;
  }

  canCompare = computed(() => (this.project()?.cvVersions?.length ?? 0) >= 2);

  toggleDescription(): void {
    this.descriptionExpanded.update(v => !v);
  }
}