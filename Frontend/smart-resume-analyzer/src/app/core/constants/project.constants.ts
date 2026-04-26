export const INDUSTRIES = [
  'Backend', 'Frontend', 'Full Stack', 'Mobile', 'AI/ML',
  'DevOps', 'Data', 'QA', 'Security', 'Design', 'Management', 'Other'
] as const;

export const SENIORITIES = [
  'Intern', 'Junior', 'Mid', 'Senior', 'Lead', 'Principal'
] as const;

export const STATUSES = [
  'Draft', 'Sent', 'Accepted', 'Declined'
] as const;

export const INDUSTRY_OPTIONS = INDUSTRIES.map(i => ({ label: i, value: i }));

export const SENIORITY_OPTIONS = SENIORITIES.map(s => ({ label: s, value: s }));

export const STATUS_OPTIONS = STATUSES.map(s => ({ label: s, value: s }));