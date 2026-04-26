import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { Router } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { SkeletonModule } from 'primeng/skeleton';
import { SelectModule } from 'primeng/select';
import { Navbar } from '../../../layout/navbar/navbar';
import { Project } from '../../../core/services/project';
import { ErrorHandler } from '../../../core/services/error-handler';
import { ProjectSummary } from '../../../core/models/project.models';
import { DatePipe, NgClass } from '@angular/common';
import { FormsModule } from '@angular/forms';

const INDUSTRIES = [
  'Backend', 'Frontend', 'Full Stack', 'Mobile', 'AI/ML',
  'DevOps', 'Data', 'QA', 'Security', 'Design', 'Management', 'Other'
];

const STATUSES = ['Draft', 'Sent', 'Accepted', 'Declined'];

const SENIORITIES = ['Intern', 'Junior', 'Mid', 'Senior', 'Lead', 'Principal'];

@Component({
  selector: 'app-project-list',
  imports: [ButtonModule, TagModule, SkeletonModule, SelectModule, FormsModule, NgClass, Navbar, DatePipe],
  templateUrl: './project-list.html',
  styleUrl: './project-list.scss'
})
export class ProjectList implements OnInit {
  private projectService = inject(Project);
  private errorHandler = inject(ErrorHandler);
  private router = inject(Router);

  projects = signal<ProjectSummary[]>([]);
  isLoading = signal<boolean>(true);
  filtersOpen = signal<boolean>(false);

  selectedStatus = signal<string | null>(null);
  selectedIndustry = signal<string | null>(null);
  selectedSeniority = signal<string | null>(null);
  selectedSort = signal<'newest' | 'oldest' | 'best-score'>('newest');

  readonly industries = INDUSTRIES;
  readonly statuses = STATUSES;
  readonly seniorities = SENIORITIES;

  readonly sortOptions = [
    { label: 'Newest first', value: 'newest' },
    { label: 'Oldest first', value: 'oldest' },
    { label: 'Best match score', value: 'best-score' }
  ];

  filteredProjects = computed(() => {
    let result = [...this.projects()];

    if (this.selectedStatus()) {
      result = result.filter(p => p.status === this.selectedStatus());
    }

    if (this.selectedIndustry()) {
      result = result.filter(p => p.industry === this.selectedIndustry());
    }

    if (this.selectedSeniority()) {
      result = result.filter(p => p.seniority === this.selectedSeniority());
    }

    if (this.selectedSort() === 'oldest') {
      result.sort((a, b) => new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime());
    } else if (this.selectedSort() === 'best-score') {
      result.sort((a, b) => (b.bestMatchScore ?? -1) - (a.bestMatchScore ?? -1));
    } else {
      result.sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime());
    }

    return result;
  });

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
      case 'declined': return 'danger';
      default: return 'secondary';
    }
  }

  getScoreColor(score: number): string {
    if (score >= 70) return '#10B981';
    if (score >= 40) return '#F59E0B';
    return '#EF4444';
  }

  toggleFilters(): void {
    this.filtersOpen.update(v => !v);
  }

  clearFilters(): void {
    this.selectedStatus.set(null);
    this.selectedIndustry.set(null);
    this.selectedSeniority.set(null);
    this.selectedSort.set('newest');
  }

  hasActiveFilters = computed(() =>
    this.selectedStatus() !== null || this.selectedIndustry() !== null ||
    this.selectedSeniority() !== null || this.selectedSort() !== 'newest'
  );

  getIndustryKey(industry: string): string {
    return industry.toLowerCase().replace(/[\/\s]/g, '');
  }

  navigateToProject(id: string): void {
    this.router.navigate(['/projects', id]);
  }

  navigateToCreate(): void {
    this.router.navigate(['/projects/create']);
  }
}