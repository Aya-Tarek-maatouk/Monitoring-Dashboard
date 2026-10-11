import { Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { LucideDynamicIcon } from '@lucide/angular';
import { ToastService } from './core/notifications/toast.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, LucideDynamicIcon],
  template: `
    <router-outlet />
    <div class="toast-region" aria-live="assertive" aria-atomic="true">
      @if (toast.message()) {
        <div class="error-toast">
          <svg lucideIcon="circle-alert" [size]="20" aria-hidden="true"></svg>
          <span>{{ toast.message() }}</span>
          <button type="button" (click)="toast.dismiss()" aria-label="Dismiss notification" title="Dismiss notification">
            <svg lucideIcon="x" [size]="18" aria-hidden="true"></svg>
          </button>
        </div>
      }
    </div>
  `,
  styleUrl: './app.scss',
})
export class App {
  readonly toast = inject(ToastService);
}
