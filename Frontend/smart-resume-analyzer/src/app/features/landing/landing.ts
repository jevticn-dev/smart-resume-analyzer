import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { Navbar } from '../../layout/navbar/navbar';

@Component({
  selector: 'app-landing',
  imports: [ButtonModule, Navbar],
  templateUrl: './landing.html',
  styleUrl: './landing.scss'
})
export class Landing {
  private router = inject(Router);

  navigateTo(path: string): void {
    this.router.navigate([path]);
  }
}