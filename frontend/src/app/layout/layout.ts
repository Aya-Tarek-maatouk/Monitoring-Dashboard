import { Component, effect, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { catchError, EMPTY, exhaustMap, timer } from 'rxjs';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { LucideDynamicIcon } from '@lucide/angular';
import { AuthService } from '../core/auth/auth.service';
import { RealtimeService } from '../core/realtime/realtime.service';

@Component({ selector: 'app-layout', imports: [RouterLink, RouterLinkActive, RouterOutlet, LucideDynamicIcon], templateUrl: './layout.html', styleUrl: './layout.scss' })
export class Layout {
  readonly auth = inject(AuthService);
  readonly realtime = inject(RealtimeService);
  readonly router = inject(Router);
  private readonly http = inject(HttpClient);
  readonly navigation = [
    { label: 'Overview', path: '/overview', icon: 'layout-dashboard' },
    { label: 'Servers', path: '/servers', icon: 'server' },
    { label: 'Alerts', path: '/alerts', icon: 'circle-alert' },
    { label: 'Reports', path: '/reports', icon: 'file-text' },
    { label: 'Jobs', path: '/jobs', icon: 'clock-3' },
  ];
  constructor() {
    effect(onCleanup => {
      if (this.auth.isAuthenticated()) {
        void this.realtime.connect();
        onCleanup(() => { void this.realtime.disconnect(); });
      }
    });
    timer(0, 30000).pipe(
      takeUntilDestroyed(),
      exhaustMap(() => this.http.get('/api/v1/users/me').pipe(catchError(() => EMPTY))),
    ).subscribe();
  }
  logout(): void { void this.realtime.disconnect(); this.auth.logout(); void this.router.navigate(['/login']); }
}