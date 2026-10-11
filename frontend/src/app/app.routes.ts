import { Routes } from '@angular/router';
import { adminGuard, authGuard, loginGuard } from './core/auth/auth.guard';

export const routes: Routes = [
	{ path: 'login', canActivate: [loginGuard], loadComponent: () => import('./pages/login/login-page').then((module) => module.LoginPage) },
	{
		path: '', canActivate: [authGuard],
		loadComponent: () => import('./layout/layout').then((module) => module.Layout),
		children: [
			{ path: '', pathMatch: 'full', redirectTo: 'overview' },
			{ path: 'overview', loadComponent: () => import('./pages/overview/overview-page').then((module) => module.OverviewPage) },
			{ path: 'servers', loadComponent: () => import('./pages/servers/servers-page').then((module) => module.ServersPage) },
			{ path: 'servers/:serverId', loadComponent: () => import('./pages/server-detail/server-detail-page').then((module) => module.ServerDetailPage) },
			{ path: 'alerts', loadComponent: () => import('./pages/alerts/alerts-page').then((module) => module.AlertsPage) },
			{ path: 'reports', loadComponent: () => import('./pages/reports/reports-page').then((module) => module.ReportsPage) },
			{ path: 'jobs', loadComponent: () => import('./pages/jobs/jobs-page').then((module) => module.JobsPage) },
			{ path: 'users', canActivate: [adminGuard], loadComponent: () => import('./pages/users/users-page').then((module) => module.UsersPage) },
		],
	},
	{ path: '**', redirectTo: '' },
];
