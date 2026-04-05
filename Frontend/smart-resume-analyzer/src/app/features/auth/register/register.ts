import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { FloatLabelModule } from 'primeng/floatlabel';
import { Auth } from '../../../core/services/auth';
import { ErrorHandler } from '../../../core/services/error-handler';
import { RegisterRequest } from '../../../core/models/auth.models';

@Component({
  selector: 'app-register',
  imports: [RouterLink, FormsModule, ButtonModule, InputTextModule, PasswordModule, FloatLabelModule],
  templateUrl: './register.html',
  styleUrl: './register.scss'
})
export class Register {
  private auth = inject(Auth);
  private errorHandler = inject(ErrorHandler);

  passwordRegex = '^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d).{8,}$';

  form: RegisterRequest = {
    email: '',
    password: '',
    firstName: '',
    lastName: ''
  };

  loading = false;

  submit(): void {
    this.loading = true;

    this.auth.register(this.form).subscribe({
      next: () => this.loading = false,
      error: (err) => {
        this.loading = false;
        this.errorHandler.handle(err, 'Registration failed');
      }
    });
  }
}