import { Component, computed, input } from '@angular/core';
import { TagModule } from 'primeng/tag';
import { CardModule } from 'primeng/card';
import { DividerModule } from 'primeng/divider';
import { AnalysisResult } from '../../core/models/analysis.models';

@Component({
  selector: 'app-analysis-result-panel',
  imports: [TagModule, CardModule, DividerModule],
  templateUrl: './analysis-result-panel.html',
  styleUrl: './analysis-result-panel.scss'
})
export class AnalysisResultPanel {
  result = input.required<AnalysisResult>();

  highSuggestions = computed(() =>
    this.result().suggestions.filter(s => s.priority === 'high')
  );
  mediumSuggestions = computed(() =>
    this.result().suggestions.filter(s => s.priority === 'medium')
  );
  lowSuggestions = computed(() =>
    this.result().suggestions.filter(s => s.priority === 'low')
  );

  scoreColor = computed(() => {
    const score = this.result().matchScore;
    if (score >= 75) return '#10B981';
    if (score >= 50) return '#F59E0B';
    return '#EF4444';
  });

  scoreDashOffset = computed(() => {
    const score = this.result().matchScore;
    const circumference = 2 * Math.PI * 54;
    return circumference * (1 - score / 100);
  });

  getSeverity(priority: string): 'danger' | 'warn' | 'success' {
    switch (priority) {
      case 'high': return 'danger';
      case 'medium': return 'warn';
      case 'low': return 'success';
      default: return 'warn';
    }
  }
}