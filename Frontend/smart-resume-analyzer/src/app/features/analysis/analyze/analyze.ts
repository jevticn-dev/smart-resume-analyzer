import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { TextareaModule } from 'primeng/textarea';
import { SelectModule } from 'primeng/select';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { FloatLabelModule } from 'primeng/floatlabel';
import { Navbar } from '../../../layout/navbar/navbar';
import { Analysis } from '../../../core/services/analysis';
import { Toast } from '../../../core/services/toast';
import { ErrorHandler } from '../../../core/services/error-handler';
import { AnalysisRequest, AnalysisResult } from '../../../core/models/analysis.models';
import { DialogModule } from 'primeng/dialog';
import { INDUSTRY_OPTIONS, SENIORITY_OPTIONS } from '../../../core/constants/project.constants';

@Component({
  selector: 'app-analyze',
  imports: [
    FormsModule,
    ButtonModule,
    InputTextModule,
    TextareaModule,
    SelectModule,
    ProgressSpinnerModule,
    FloatLabelModule,
    Navbar,
    DialogModule
  ],
  templateUrl: './analyze.html',
  styleUrl: './analyze.scss'
})
export class Analyze implements OnInit {
  private analysisService = inject(Analysis);
  private toastService = inject(Toast);
  private errorHandler = inject(ErrorHandler);
  private router = inject(Router);
  private route = inject(ActivatedRoute);

  currentStep = signal<number>(1);
  isLoading = signal<boolean>(false);
  isDragOver = signal<boolean>(false);
  step1Touched = signal<boolean>(false);
  showLimitDialog = false;

  formData: AnalysisRequest = {
    jobTitle: '',
    companyName: '',
    jobDescription: '',
    seniorityLevel: '',
    industry: ''
  };

  selectedFile = signal<File | null>(null);
  projectId = signal<string | null>(null);

  readonly seniorityOptions = SENIORITY_OPTIONS;
  readonly industryOptions = INDUSTRY_OPTIONS;

  ngOnInit(): void {
    const params = this.route.snapshot.queryParams;
    if (params['jobTitle'] || params['jobDescription'] || params['seniorityLevel']) {
      this.projectId.set(params['projectId'] ?? null);
      this.formData = {
        jobTitle: params['jobTitle'] ?? '',
        companyName: params['companyName'] ?? '',
        jobDescription: params['jobDescription'] ?? '',
        seniorityLevel: params['seniorityLevel'] ?? '',
        industry: params['industry'] ?? ''
      };
      this.currentStep.set(2);
      return;
    }

    const pending = sessionStorage.getItem('pendingAnalysisForm');
    if (pending) {
      sessionStorage.removeItem('pendingAnalysisForm');
      this.formData = JSON.parse(pending);
      this.currentStep.set(2);
      this.toastService.info('Your session has been restored. Please re-upload your CV to continue.');
    }
  }

  nextStep(): void {
    if (this.currentStep() === 1) {
      this.step1Touched.set(true);
      if (!this.isStep1Valid()) return;
    }
    if (this.currentStep() === 2 && !this.selectedFile()) {
      this.toastService.warn('Please upload your CV before continuing.');
      return;
    }
    this.currentStep.update(s => s + 1);
  }

  prevStep(): void {
    this.currentStep.update(s => s - 1);
  }

  isStep1Valid(): boolean {
    return !!(
      this.formData.jobTitle.trim() &&
      this.formData.jobDescription.trim() &&
      this.formData.seniorityLevel
    );
  }

  submit(): void {
    if (!this.selectedFile()) return;

    this.isLoading.set(true);

    const data = new FormData();
    data.append('jobTitle', this.formData.jobTitle);
    data.append('companyName', this.formData.companyName);
    data.append('jobDescription', this.formData.jobDescription);
    data.append('seniorityLevel', this.formData.seniorityLevel);
    data.append('industry', this.formData.industry ?? '');
    data.append('cvFile', this.selectedFile()!);
    if (this.projectId()) {
      data.append('projectId', this.projectId()!);
    }

    this.analysisService.analyze(data).subscribe({
      next: (result: AnalysisResult) => {
        this.isLoading.set(false);
        this.toastService.success('Analysis completed successfully.');
        this.router.navigate(['/analysis/result'], { state: { result } });
      },
      error: (err) => {
        this.isLoading.set(false);
        if (err.status === 429) {
          sessionStorage.setItem('pendingAnalysisForm', JSON.stringify(this.formData));
          this.showLimitDialog = true;
        } else {
          this.errorHandler.handle(err);
        }
      }
    });
  }

  retryAfterAuth(): void {
    this.showLimitDialog = false;
    this.router.navigate(['/register']);
  }

  closeLimitDialog(): void {
    this.showLimitDialog = false;
  }

  onTextareaInput(event: Event): void {
    const textarea = event.target as HTMLTextAreaElement;
    textarea.style.height = 'auto';
    textarea.style.height = Math.min(textarea.scrollHeight, window.innerHeight * 0.35) + 'px';
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    this.isDragOver.set(true);
  }

  onDragLeave(): void {
    this.isDragOver.set(false);
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    this.isDragOver.set(false);
    const file = event.dataTransfer?.files[0];
    if (file) this.validateAndSetFile(file);
  }

  onFileChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (file) this.validateAndSetFile(file);
  }

  removeFile(event: Event): void {
    event.stopPropagation();
    this.selectedFile.set(null);
  }

  getFileSize(): string {
    const file = this.selectedFile();
    if (!file) return '';
    const kb = file.size / 1024;
    return kb > 1024 ? `${(kb / 1024).toFixed(1)} MB` : `${kb.toFixed(0)} KB`;
  }

  private validateAndSetFile(file: File): void {
    if (file.type !== 'application/pdf') {
      this.toastService.error('Only PDF files are allowed.');
      return;
    }
    if (file.size > 5 * 1024 * 1024) {
      this.toastService.error('File size must not exceed 5MB.');
      return;
    }
    this.selectedFile.set(file);
  }
}