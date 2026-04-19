import { AnalysisResult } from './analysis.models';

export interface ProjectSummary {
  id: string;
  title: string;
  jobTitle: string;
  companyName: string;
  seniority: string;
  status: string;
  latestMatchScore: number | null;
  strengthsCount: number | null;
  weaknessesCount: number | null;
  missingKeywordsCount: number | null;
  highSuggestionsCount: number | null;
  mediumSuggestionsCount: number | null;
  lowSuggestionsCount: number | null;
  createdAt: string;
}

export interface CvVersion {
  id: string;
  originalFileName: string;
  versionNumber: number;
  createdAt: string;
}

export interface ProjectDetail {
  id: string;
  title: string;
  jobTitle: string;
  companyName: string;
  jobDescription: string;
  seniority: string;
  status: string;
  createdAt: string;
  cvVersion: CvVersion | null;
  analysis: AnalysisResult | null;
}

export interface CreateProject {
  title: string;
  jobTitle: string;
  companyName: string;
  jobDescription: string;
  seniorityLevel: string;
}

export interface ConvertGuestAnalysis {
  analysisLogId: string;
}