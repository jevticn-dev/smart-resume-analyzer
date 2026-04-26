export interface AnalysisRequest {
  jobTitle: string;
  companyName: string;
  jobDescription: string;
  seniorityLevel: string;
  industry?: string
}

export interface SuggestionItem {
  text: string;
  priority: 'high' | 'medium' | 'low';
}

export interface AnalysisResult {
  matchScore: number;
  strengths: string[];
  weaknesses: string[];
  missingKeywords: string[];
  suggestions: SuggestionItem[];
  summary: string;
  analysisLogId?: string;
  projectId?: string;
}