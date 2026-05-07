import { Component, inject, OnInit, signal, computed } from '@angular/core';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { FloatLabelModule } from 'primeng/floatlabel';
import { ChipModule } from 'primeng/chip';
import { ChartModule } from 'primeng/chart';
import { Auth } from '../../core/services/auth';
import { Toast } from '../../core/services/toast';
import { UserProfile, UpdateProfile, ChangePassword } from '../../core/models/auth.models';
import { Navbar } from '../../layout/navbar/navbar';
import { ErrorHandler } from '../../core/services/error-handler';

@Component({
  selector: 'app-profile',
  imports: [
    RouterLink,
    FormsModule,
    ButtonModule,
    InputTextModule,
    SelectModule,
    FloatLabelModule,
    ChipModule,
    ChartModule,
    Navbar
  ],
  templateUrl: './profile.html',
  styleUrl: './profile.scss'
})
export class Profile implements OnInit {
  private auth = inject(Auth);
  private toast = inject(Toast);
  private errorHandler = inject(ErrorHandler);

  userProfile = signal<UserProfile | null>(null);
  isEditMode = signal(false);
  isLoading = signal(true);
  isSavingProfile = signal(false);
  isSavingPassword = signal(false);
  showPasswordSection = signal(false);

  formData = {
    firstName: '',
    lastName: '',
    reminderIntervalDays: 7
  };

  passwordData = {
    currentPassword: '',
    newPassword: '',
    confirmPassword: ''
  };

  reminderOptions = [
    { label: '1 minute (demo)', value: 1 },
    { label: '3 days', value: 3 },
    { label: '7 days', value: 7 },
    { label: '14 days', value: 14 }
  ];

  chartData: any = null;
  chartOptions: any = null;

  ngOnInit(): void {
    this.auth.loadCurrentUser().subscribe({
      next: (profile) => {
        this.userProfile.set(profile);
        this.buildChart(profile);
        this.isLoading.set(false);
      },
      error: () => {
        this.toast.error('Failed to load profile.');
        this.isLoading.set(false);
      }
    });
  }

  getInitials(): string {
    const user = this.userProfile();
    if (!user) return '';
    return `${user.firstName.charAt(0)}${user.lastName.charAt(0)}`.toUpperCase();
  }

  enterEditMode(): void {
    const profile = this.userProfile();
    if (!profile) return;
    this.formData = {
      firstName: profile.firstName,
      lastName: profile.lastName,
      reminderIntervalDays: profile.reminderIntervalDays || 7
    };
    this.isEditMode.set(true);
  }

  cancelEdit(): void {
    this.isEditMode.set(false);
  }

  saveProfile(): void {
    if (!this.formData.firstName.trim() || !this.formData.lastName.trim()) {
      this.toast.error('First name and last name are required.');
      return;
    }

    this.isSavingProfile.set(true);
    const dto: UpdateProfile = {
      firstName: this.formData.firstName.trim(),
      lastName: this.formData.lastName.trim(),
      reminderIntervalDays: this.formData.reminderIntervalDays
    };

    this.auth.updateProfile(dto).subscribe({
      next: (profile) => {
        this.userProfile.set(profile);
        this.buildChart(profile);
        this.isEditMode.set(false);
        this.isSavingProfile.set(false);
        this.toast.success('Profile updated successfully.');
      },
      error: () => {
        this.isSavingProfile.set(false);
        this.toast.error('Failed to update profile.');
      }
    });
  }

  savePassword(): void {
    if (!this.passwordData.currentPassword || !this.passwordData.newPassword) {
      this.toast.error('All password fields are required.');
      return;
    }

    if (this.passwordData.newPassword !== this.passwordData.confirmPassword) {
      this.toast.error('New passwords do not match.');
      return;
    }

    this.isSavingPassword.set(true);
    const dto: ChangePassword = {
      currentPassword: this.passwordData.currentPassword,
      newPassword: this.passwordData.newPassword
    };

    this.auth.changePassword(dto).subscribe({
      next: () => {
        this.isSavingPassword.set(false);
        this.showPasswordSection.set(false);
        this.passwordData = { currentPassword: '', newPassword: '', confirmPassword: '' };
        this.toast.success('Password changed successfully.');
      },
      error: (err) => {
        this.isSavingPassword.set(false);
        this.errorHandler.handle(err);
      }
    });
  }

  private buildChart(profile: UserProfile): void {
    const s = profile.stats;
    const draft = s.totalProjects - s.sentApplications - s.acceptedApplications - s.declinedApplications;

    this.chartData = {
      labels: ['Draft', 'Sent', 'Accepted', 'Declined'],
      datasets: [{
        data: [
          Math.max(0, draft),
          s.sentApplications,
          s.acceptedApplications,
          s.declinedApplications
        ],
        backgroundColor: ['#94A3B8', '#0D9488', '#10B981', '#EF4444'],
        borderWidth: 0
      }]
    };

    this.chartOptions = {
      plugins: {
        legend: {
          position: 'bottom',
          labels: {
            usePointStyle: true,
            padding: 16,
            font: { size: 13 }
          }
        }
      },
      cutout: '65%'
    };
  }
}