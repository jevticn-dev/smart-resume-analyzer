import { Component, OnInit, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { SkeletonModule } from 'primeng/skeleton';
import { Navbar } from '../../../layout/navbar/navbar';
import { Project } from '../../../core/services/project';
import { ErrorHandler } from '../../../core/services/error-handler';
import { ProjectSummary } from '../../../core/models/project.models';
import { DatePipe } from '@angular/common';

@Component({
  selector: 'app-project-list',
  imports: [ButtonModule, TagModule, SkeletonModule, Navbar, DatePipe],
  templateUrl: './project-list.html',
  styleUrl: './project-list.scss'
})
export class ProjectList implements OnInit {
  private projectService = inject(Project);
  private errorHandler = inject(ErrorHandler);
  private router = inject(Router);

  projects = signal<ProjectSummary[]>([]);
  isLoading = signal<boolean>(true);

  ngOnInit(): void {
    this.projectService.getProjects().subscribe({
      next: (data) => {
        this.projects.set(data);
        this.isLoading.set(false);
      },
      error: (err) => {
        this.errorHandler.handle(err);
        this.isLoading.set(false);
      }
    });
  }

  getStatusSeverity(status: string): 'info' | 'success' | 'danger' | 'warn' | 'secondary' {
    switch (status.toLowerCase()) {
      case 'sent': return 'info';
      case 'accepted': return 'success';
      case 'rejected': return 'danger';
      case 'waiting': return 'warn';
      default: return 'secondary';
    }
  }

  getScoreColor(score: number): string {
    if (score >= 75) return '#10B981';
    if (score >= 50) return '#F59E0B';
    return '#EF4444';
  }

  getScoreDashOffset(score: number): number {
    const circumference = 2 * Math.PI * 34;
    return circumference * (1 - score / 100);
  }

  navigateToProject(id: string): void {
    this.router.navigate(['/projects', id]);
  }

  navigateToCreate(): void {
    this.router.navigate(['/projects/create']);
  }
}