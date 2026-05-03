import { AnalysisResult } from './analysis.models';

export interface CvVersionDetail {
  id: string;
  versionNumber: number;
  originalFileName: string;
  notes: string;
  createdAt: string;
  analysis: AnalysisResult | null;
}

export interface ProjectSummary {
  id: string;
  title: string;
  jobTitle: string;
  companyName: string;
  industry: string;
  seniority: string;
  status: string;
  versionCount: number;
  bestMatchScore: number | null;
  createdAt: string;
}

export interface ProjectDetail {
  id: string;
  title: string;
  jobTitle: string;
  companyName: string;
  industry: string;
  jobDescription: string;
  seniority: string;
  status: string;
  createdAt: string;
  companyEmail: string;
  cvVersions: CvVersionDetail[];
}

export interface CreateProject {
  title: string;
  jobTitle: string;
  companyName: string;
  industry: string;
  jobDescription: string;
  seniorityLevel: string;
  companyEmail: string;
}

export interface UpdateProject {
  title: string;
  jobTitle: string;
  companyName: string;
  industry: string;
  seniorityLevel: string;
  jobDescription: string;
  status: string;
  companyEmail: string;
}

export interface CompareVersions {
  versionA: CvVersionDetail;
  versionB: CvVersionDetail;
}

export interface ConvertGuestAnalysis {
  analysisLogId: string;
}