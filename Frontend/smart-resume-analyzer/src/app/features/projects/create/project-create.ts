import { Component, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { TextareaModule } from 'primeng/textarea';
import { SelectModule } from 'primeng/select';
import { FloatLabelModule } from 'primeng/floatlabel';
import { Navbar } from '../../../layout/navbar/navbar';
import { Project } from '../../../core/services/project';
import { Toast } from '../../../core/services/toast';
import { ErrorHandler } from '../../../core/services/error-handler';
import { CreateProject } from '../../../core/models/project.models';
import { INDUSTRY_OPTIONS, SENIORITY_OPTIONS } from '../../../core/constants/project.constants';

@Component({
  selector: 'app-project-create',
  imports: [
    FormsModule,
    ButtonModule,
    InputTextModule,
    TextareaModule,
    SelectModule,
    FloatLabelModule,
    Navbar
  ],
  templateUrl: './project-create.html',
  styleUrl: './project-create.scss'
})
export class ProjectCreate {
  private projectService = inject(Project);
  private toastService = inject(Toast);
  private errorHandler = inject(ErrorHandler);
  private router = inject(Router);

  isLoading = signal<boolean>(false);
  touched = signal<boolean>(false);

  formData: CreateProject = {
    title: '',
    jobTitle: '',
    companyName: '',
    industry: '',
    jobDescription: '',
    companyEmail: '',
    seniorityLevel: ''
  };

  seniorityOptions = SENIORITY_OPTIONS
  industryOptions = INDUSTRY_OPTIONS

  isValid(): boolean {
    return !!(
      this.formData.title.trim() &&
      this.formData.jobTitle.trim() &&
      this.formData.jobDescription.trim() &&
      this.formData.seniorityLevel
    );
  }

  submit(): void {
    this.touched.set(true);
    if (!this.isValid()) return;

    this.isLoading.set(true);
    this.projectService.createProject(this.formData).subscribe({
      next: (project) => {
        this.toastService.success('Project created successfully.');
        this.router.navigate(['/projects', project.id]);
      },
      error: (err) => {
        this.isLoading.set(false);
        this.errorHandler.handle(err);
      }
    });
  }

  cancel(): void {
    this.router.navigate(['/projects']);
  }

  onTextareaInput(event: Event): void {
    const textarea = event.target as HTMLTextAreaElement;
    textarea.style.height = 'auto';
    textarea.style.height = Math.min(textarea.scrollHeight, window.innerHeight * 0.35) + 'px';
  }
}