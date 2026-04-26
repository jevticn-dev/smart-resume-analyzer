import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { SkeletonModule } from 'primeng/skeleton';
import { TagModule } from 'primeng/tag';
import { NgClass, DatePipe } from '@angular/common';
import { Navbar } from '../../../layout/navbar/navbar';
import { Project } from '../../../core/services/project';
import { ErrorHandler } from '../../../core/services/error-handler';
import { CompareVersions } from '../../../core/models/project.models';

@Component({
  selector: 'app-version-compare',
  imports: [ButtonModule, SkeletonModule, TagModule, NgClass, Navbar, DatePipe],
  templateUrl: './version-compare.html',
  styleUrl: './version-compare.scss'
})
export class VersionCompare implements OnInit {
  private route = inject(ActivatedRoute);
  private projectService = inject(Project);
  private errorHandler = inject(ErrorHandler);
  private router = inject(Router);

  compare = signal<CompareVersions | null>(null);
  isLoading = signal<boolean>(true);
  projectId = '';

  ngOnInit(): void {
    this.projectId = this.route.snapshot.paramMap.get('id')!;
    const versionAId = this.route.snapshot.queryParamMap.get('versionAId')!;
    const versionBId = this.route.snapshot.queryParamMap.get('versionBId')!;

    this.projectService.compareVersions(this.projectId, versionAId, versionBId).subscribe({
      next: (data) => {
        this.compare.set(data);
        this.isLoading.set(false);
      },
      error: (err) => {
        this.errorHandler.handle(err);
        this.router.navigate(['/projects', this.projectId]);
      }
    });
  }

  getScoreColor(score: number): string {
    if (score >= 70) return '#10B981';
    if (score >= 40) return '#F59E0B';
    return '#EF4444';
  }

  getScoreDiff(): number {
    const a = this.compare()?.versionA.analysis?.matchScore ?? 0;
    const b = this.compare()?.versionB.analysis?.matchScore ?? 0;
    return b - a;
  }

  isImproved(valueB: number, valueA: number): boolean {
    return valueB > valueA;
  }

  isWorse(valueB: number, valueA: number): boolean {
    return valueB < valueA;
  }

  goBack(): void {
    this.router.navigate(['/projects', this.projectId]);
  }
}