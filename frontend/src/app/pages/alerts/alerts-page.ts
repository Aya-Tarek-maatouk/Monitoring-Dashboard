import { DatePipe, DecimalPipe } from '@angular/common';
import { Component, DestroyRef, effect, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Subscription } from 'rxjs';
import { FormsModule } from '@angular/forms';
import { LucideDynamicIcon } from '@lucide/angular';
import { AlertsService } from '../../core/api/alerts.service';
import { AlertRecord, AlertStatus } from '../../core/models/models';
import { RealtimeService } from '../../core/realtime/realtime.service';
import { Pagination, PageSelection } from '../../shared/pagination/pagination';

@Component({
  selector: 'app-alerts-page',
  imports: [DatePipe, DecimalPipe, FormsModule, LucideDynamicIcon, Pagination],
  templateUrl: './alerts-page.html',
  styleUrl: './alerts-page.scss',
})
export class AlertsPage {
  private readonly alertsApi = inject(AlertsService);
  private readonly realtime = inject(RealtimeService);
  readonly alerts = signal<AlertRecord[]>([]);
  readonly loading = signal(true);
  readonly error = signal('');
  readonly page = signal(1);
  readonly pageSize = signal(20);
  readonly totalCount = signal(0);
  private readonly destroyRef = inject(DestroyRef);
  private listRequest?: Subscription;
  status: 'All' | AlertStatus = 'Triggered';
  constructor() {
    this.refresh();
    effect(() => {
      if (this.realtime.latestAlert()) this.refresh();
    });
  }
  refresh(): void {
    this.listRequest?.unsubscribe();
    this.loading.set(true);
    this.listRequest = this.alertsApi
      .getAlerts(this.page(), this.pageSize(), this.status === 'All' ? undefined : this.status)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (result) => {
          this.totalCount.set(result.totalCount);
          const lastPage = Math.max(1, Math.ceil(result.totalCount / this.pageSize()));
          if (this.page() > lastPage) {
            this.page.set(lastPage);
            this.refresh();
            return;
          }
          this.alerts.set(result.items);
          this.loading.set(false);
          this.error.set('');
        },
        error: () => {
          this.loading.set(false);
          this.error.set('Could not load alert history.');
        },
      });
  }
  changeStatus(status: 'All' | AlertStatus): void {
    this.status = status;
    this.page.set(1);
    this.refresh();
  }
  changePage(selection: PageSelection): void {
    this.page.set(selection.page);
    this.pageSize.set(selection.pageSize);
    this.refresh();
  }
}
