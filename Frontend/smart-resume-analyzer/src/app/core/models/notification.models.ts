export interface Notification {
  id: string;
  projectId: string;
  message: string;
  type: string;
  isRead: boolean;
  createdAt: string;
}