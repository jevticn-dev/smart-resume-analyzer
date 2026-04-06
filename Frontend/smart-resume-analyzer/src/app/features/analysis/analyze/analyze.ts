import { Component, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { TextareaModule } from 'primeng/textarea';
import { SelectModule } from 'primeng/select';
import { StepperModule } from 'primeng/stepper';
import { FileUploadModule } from 'primeng/fileupload';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { CardModule } from 'primeng/card';
import { FloatLabelModule } from 'primeng/floatlabel';
import { Navbar } from '../../../layout/navbar/navbar';
import { Analysis } from '../../../core/services/analysis';
import { Toast } from '../../../core/services/toast';
import { ErrorHandler } from '../../../core/services/error-handler';
import { AnalysisRequest, AnalysisResult } from '../../../core/models/analysis.models';

@Component({
  selector: 'app-analyze',
  imports: [
    FormsModule,
    ButtonModule,
    InputTextModule,
    TextareaModule,
    SelectModule,
    StepperModule,
    FileUploadModule,
    ProgressSpinnerModule,
    CardModule,
    FloatLabelModule,
    Navbar
  ],
  templateUrl: './analyze.html',
  styleUrl: './analyze.scss'
})
export class Analyze {
  private analysisService = inject(Analysis);
  private toastService = inject(Toast);
  private errorHandler = inject(ErrorHandler);
  private router = inject(Router);

  currentStep = signal<number>(1);
  isLoading = signal<boolean>(false);
  isDragOver = signal<boolean>(false);
  step1Touched = signal<boolean>(false);

  private textareaHeight = '120px';

  formData: AnalysisRequest = {
    jobTitle: '',
    jobDescription: '',
    seniorityLevel: ''
  };

  selectedFile = signal<File | null>(null);

  seniorityOptions = [
    { label: 'Intern', value: 'Intern' },
    { label: 'Junior', value: 'Junior' },
    { label: 'Mid-level', value: 'Mid-level' },
    { label: 'Senior', value: 'Senior' },
    { label: 'Lead', value: 'Lead' }
  ];

  nextStep(): void {
    if (this.currentStep() === 1) {
      this.step1Touched.set(true);
      if (!this.isStep1Valid()) {
        return;
      }
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

  onFileSelect(event: any): void {
    const file = event.files?.[0];
    if (file) {
      if (file.type !== 'application/pdf') {
        this.toastService.error('Only PDF files are allowed.');
        return;
      }
      this.selectedFile.set(file);
    }
  }

  onFileRemove(): void {
    this.selectedFile.set(null);
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
    data.append('jobDescription', this.formData.jobDescription);
    data.append('seniorityLevel', this.formData.seniorityLevel);
    data.append('cvFile', this.selectedFile()!);

    this.analysisService.analyze(data).subscribe({
      next: (result: AnalysisResult) => {
        this.isLoading.set(false);
        this.toastService.success('Analysis completed successfully.');
        this.router.navigate(['/analysis/result'], { state: { result } });
      },
      error: (err) => {
        this.isLoading.set(false);
        this.errorHandler.handle(err);
      }
    });
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