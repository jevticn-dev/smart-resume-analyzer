export interface EmailDraft {
  subject: string;
  body: string;
}

export interface GenerateEmailDraftRequest {
  projectId: string;
  versionId: string;
}

export interface SendEmailRequest {
  projectId: string;
  versionId: string;
  toEmail: string;
  subject: string;
  body: string;
}