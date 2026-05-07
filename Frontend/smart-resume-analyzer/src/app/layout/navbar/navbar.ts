import { Component, inject, OnInit, OnDestroy } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { BadgeModule } from 'primeng/badge';
import { Auth } from '../../core/services/auth';
import { NotificationService } from '../../core/services/notification';
import { Notification } from '../../core/models/notification.models';
import { signal, computed } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';

@Component({
  selector: 'app-navbar',
  imports: [RouterLink, ButtonModule, BadgeModule, CommonModule, DatePipe],
  templateUrl: './navbar.html',
  styleUrl: './navbar.scss'
})
export class Navbar implements OnInit, OnDestroy {
  auth = inject(Auth);
  private router = inject(Router);
  private notificationService = inject(NotificationService);

  notifications = signal<Notification[]>([]);
  unreadCount = computed(() => this.notifications().filter(n => !n.isRead).length);
  notificationPanelOpen = signal(false);
  profilePopoutOpen = signal(false);

  private pollingInterval: ReturnType<typeof setInterval> | null = null;

  ngOnInit(): void {
    if (this.auth.isAuthenticated()) {
      this.loadNotifications();
      this.pollingInterval = setInterval(() => this.loadNotifications(), 60000);
    }
  }

  ngOnDestroy(): void {
    if (this.pollingInterval) {
      clearInterval(this.pollingInterval);
    }
  }

  loadNotifications(): void {
    this.notificationService.getNotifications().subscribe({
      next: (data) => this.notifications.set(data),
      error: () => {}
    });
  }

  toggleNotificationPanel(): void {
    this.notificationPanelOpen.update(v => !v);
    if (this.profilePopoutOpen()) this.profilePopoutOpen.set(false);
  }

  toggleProfilePopout(): void {
    this.profilePopoutOpen.update(v => !v);
    if (this.notificationPanelOpen()) this.notificationPanelOpen.set(false);
  }

  markAsRead(id: string): void {
    this.notificationService.markAsRead(id).subscribe({
      next: () => {
        this.notifications.update(list =>
          list.map(n => n.id === id ? { ...n, isRead: true } : n)
        );
      },
      error: () => {}
    });
  }

  markAllAsRead(): void {
    this.notificationService.markAllAsRead().subscribe({
      next: () => {
        this.notifications.update(list => list.map(n => ({ ...n, isRead: true })));
      },
      error: () => {}
    });
  }

  goToProject(notification: Notification): void {
    if (!notification.isRead) {
      this.markAsRead(notification.id);
    }
    this.notificationPanelOpen.set(false);
    this.router.navigate(['/projects', notification.projectId]);
  }

  getInitials(): string {
    const user = this.auth.currentUser();
    if (!user) return '';
    return `${user.firstName.charAt(0)}${user.lastName.charAt(0)}`.toUpperCase();
  }

  goToProfile(): void {
    this.profilePopoutOpen.set(false);
    this.router.navigate(['/profile']);
  }

logout(): void {
  if (this.pollingInterval) {
    clearInterval(this.pollingInterval);
    this.pollingInterval = null;
  }
  this.auth.logout();
}

  navigateTo(path: string): void {
    this.router.navigate([path]);
  }
}