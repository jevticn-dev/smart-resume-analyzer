export interface AnalysisRequest {
  jobTitle: string;
  jobDescription: string;
  seniorityLevel: string;
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
}