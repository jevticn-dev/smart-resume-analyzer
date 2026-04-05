import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { FloatLabelModule } from 'primeng/floatlabel';
import { Auth } from '../../../core/services/auth';
import { ErrorHandler } from '../../../core/services/error-handler';
import { LoginRequest } from '../../../core/models/auth.models';

@Component({
  selector: 'app-login',
  imports: [RouterLink, FormsModule, ButtonModule, InputTextModule, PasswordModule, FloatLabelModule],
  templateUrl: './login.html',
  styleUrl: './login.scss'
})
export class Login {
  private auth = inject(Auth);
  private errorHandler = inject(ErrorHandler);

  form: LoginRequest = {
    email: '',
    password: ''
  };

  loading = false;

  submit(): void {
    this.loading = true;

    this.auth.login(this.form).subscribe({
      next: () => this.loading = false,
      error: (err) => {
        this.loading = false;
        this.errorHandler.handle(err, 'Login failed');
      }
    });
  }
}