import { Component, inject, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Auth } from './core/services/auth';
import { ToastModule } from 'primeng/toast';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, ToastModule],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App implements OnInit {
  private auth = inject(Auth);

  ngOnInit(): void {
    const token = this.auth.getToken();
    if (token) {
      this.auth.isAuthenticated.set(true);
      this.auth.loadCurrentUser().subscribe({
        error: () => {
          this.auth.logout();
        }
      });
    }
  }
}