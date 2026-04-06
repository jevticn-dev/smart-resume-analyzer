import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth-guard';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./features/landing/landing').then(m => m.Landing)
  },
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login/login').then(m => m.Login)
  },
  {
    path: 'register',
    loadComponent: () => import('./features/auth/register/register').then(m => m.Register)
  },
  {
    path: 'dashboard',
    canActivate: [authGuard],
    loadComponent: () => import('./features/landing/landing').then(m => m.Landing)
  },
  {
    path: 'analyze',
    loadComponent: () => import('./features/analysis/analyze/analyze').then(m => m.Analyze)
  },
  {
    path: 'analysis/result',
    loadComponent: () => import('./features/analysis/result/result').then(m => m.Result)
  },
  {
    path: '**',
    redirectTo: ''
  }
];