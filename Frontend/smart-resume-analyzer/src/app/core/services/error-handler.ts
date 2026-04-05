import { Injectable, inject } from '@angular/core';
import { Toast } from './toast';

@Injectable({
  providedIn: 'root'
})
export class ErrorHandler {
  private toast = inject(Toast);

  handle(err: any, summary = 'Error'): void {
    if (err.error?.errors) {
      const errors = Object.values(err.error.errors).flat() as string[];
      this.toast.validationError(errors, summary);
    } else {
      const detail = err.error?.message ?? 'Something went wrong. Please try again.';
      this.toast.error(detail, summary);
    }
  }
}