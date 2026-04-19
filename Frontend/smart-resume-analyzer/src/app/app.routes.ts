import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth-guard';
import { guestGuard } from './core/guards/guest-guard';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./features/landing/landing').then(m => m.Landing)
  },
  {
    path: 'login',
    canActivate: [guestGuard],
    loadComponent: () => import('./features/auth/login/login').then(m => m.Login)
  },
  {
    path: 'register',
    canActivate: [guestGuard],
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
    path: 'projects',
    canActivate: [authGuard],
    loadComponent: () => import('./features/projects/list/project-list').then(m => m.ProjectList)
  },
  {
    path: 'projects/create',
    canActivate: [authGuard],
    loadComponent: () => import('./features/projects/create/project-create').then(m => m.ProjectCreate)
  },
  {
    path: 'projects/:id',
    canActivate: [authGuard],
    loadComponent: () => import('./features/projects/detail/project-detail').then(m => m.ProjectDetail)
  },
  {
    path: '**',
    redirectTo: ''
  }
];