import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { TextareaModule } from 'primeng/textarea';
import { SelectModule } from 'primeng/select';
import { FloatLabelModule } from 'primeng/floatlabel';
import { SkeletonModule } from 'primeng/skeleton';
import { Navbar } from '../../../layout/navbar/navbar';
import { Project } from '../../../core/services/project';
import { Toast } from '../../../core/services/toast';
import { ErrorHandler } from '../../../core/services/error-handler';
import { UpdateProject } from '../../../core/models/project.models';
import { INDUSTRY_OPTIONS, SENIORITY_OPTIONS, STATUS_OPTIONS } from '../../../core/constants/project.constants';

@Component({
  selector: 'app-project-edit',
  imports: [
    FormsModule,
    ButtonModule,
    InputTextModule,
    TextareaModule,
    SelectModule,
    FloatLabelModule,
    SkeletonModule,
    Navbar
  ],
  templateUrl: './project-edit.html',
  styleUrl: './project-edit.scss'
})
export class ProjectEdit implements OnInit {
  private route = inject(ActivatedRoute);
  private projectService = inject(Project);
  private toastService = inject(Toast);
  private errorHandler = inject(ErrorHandler);
  private router = inject(Router);

  isLoading = signal<boolean>(true);
  isSaving = signal<boolean>(false);
  touched = signal<boolean>(false);
  projectId = '';

  formData: UpdateProject = {
    title: '',
    jobTitle: '',
    companyName: '',
    industry: '',
    seniorityLevel: '',
    jobDescription: '',
    status: '',
    companyEmail: ''
  };

  seniorityOptions = SENIORITY_OPTIONS
  industryOptions = INDUSTRY_OPTIONS
  statusOptions = STATUS_OPTIONS

  ngOnInit(): void {
    this.projectId = this.route.snapshot.paramMap.get('id')!;
    this.projectService.getProject(this.projectId).subscribe({
      next: (data) => {
        this.formData = {
          title: data.title,
          jobTitle: data.jobTitle,
          companyName: data.companyName,
          industry: data.industry,
          seniorityLevel: data.seniority,
          jobDescription: data.jobDescription,
          status: data.status,
          companyEmail: data.companyEmail
        };
        this.isLoading.set(false);
      },
      error: (err) => {
        this.errorHandler.handle(err);
        this.router.navigate(['/projects']);
      }
    });
  }

  isValid(): boolean {
    return !!(
      this.formData.title.trim() &&
      this.formData.jobTitle.trim() &&
      this.formData.jobDescription.trim() &&
      this.formData.seniorityLevel &&
      this.formData.status
    );
  }

  submit(): void {
    this.touched.set(true);
    if (!this.isValid()) return;

    this.isSaving.set(true);
    this.projectService.updateProject(this.projectId, this.formData).subscribe({
      next: () => {
        this.toastService.success('Project updated successfully.');
        this.router.navigate(['/projects', this.projectId]);
      },
      error: (err) => {
        this.isSaving.set(false);
        this.errorHandler.handle(err);
      }
    });
  }

  cancel(): void {
    this.router.navigate(['/projects', this.projectId]);
  }

  onTextareaInput(event: Event): void {
    const textarea = event.target as HTMLTextAreaElement;
    textarea.style.height = 'auto';
    textarea.style.height = Math.min(textarea.scrollHeight, window.innerHeight * 0.35) + 'px';
  }
}