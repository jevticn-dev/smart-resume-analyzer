import { Component, OnInit, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { Navbar } from '../../../layout/navbar/navbar';
import { AnalysisResult } from '../../../core/models/analysis.models';

@Component({
  selector: 'app-result',
  imports: [ButtonModule, TagModule, Navbar],
  templateUrl: './result.html',
  styleUrl: './result.scss'
})
export class Result implements OnInit {
  private router = inject(Router);

  result = signal<AnalysisResult | null>(null);

  ngOnInit(): void {
    const state = history.state as { result: AnalysisResult };

    if (state?.result) {
      this.result.set(state.result);
    } else {
      this.router.navigate(['/analyze']);
    }
  }

  getSeverity(priority: string): 'danger' | 'warn' | 'success' {
    switch (priority) {
      case 'high': return 'danger';
      case 'medium': return 'warn';
      case 'low': return 'success';
      default: return 'warn';
    }
  }

  analyzeAnother(): void {
    this.router.navigate(['/analyze']);
  }
}