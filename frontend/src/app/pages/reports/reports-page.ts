import { DatePipe } from '@angular/common';
import { Component, DestroyRef, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { LucideDynamicIcon } from '@lucide/angular';
import { interval, Subscription } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ServersService } from '../../core/api/servers.service';
import { ReportsService } from '../../core/api/reports.service';
import { ReportRecord, ServerRecord } from '../../core/models/models';
import { Pagination, PageSelection } from '../../shared/pagination/pagination';

@Component({
  selector: 'app-reports-page',
  imports: [DatePipe, FormsModule, LucideDynamicIcon, Pagination],
  templateUrl: './reports-page.html',
  styleUrl: './reports-page.scss',
})
export class ReportsPage {
  private readonly serversApi = inject(ServersService);
  private readonly reportsApi = inject(ReportsService);
  private readonly destroyRef = inject(DestroyRef);
  readonly servers = signal<ServerRecord[]>([]);
  readonly reports = signal<ReportRecord[]>([]);
  readonly page = signal(1);
  readonly pageSize = signal(20);
  readonly totalCount = signal(0);
  readonly loading = signal(false);
  private listRequest?: Subscription;
  readonly busy = signal(false);
  readonly error = signal('');
  readonly downloading = signal<number | null>(null);
  serverId = 0;
  filterServerId = 0;
  startTime = this.localInputValue(new Date(Date.now() - 24 * 60 * 60 * 1000));
  endTime = this.localInputValue(new Date());

  constructor() {
    this.refresh();
    this.serversApi
      .getAllServers()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (servers) => {
          this.servers.set(servers);
          this.serverId ||= servers[0]?.serverId ?? 0;
        },
        error: () => this.error.set('Could not load servers.'),
      });
    interval(5000)
      .pipe(takeUntilDestroyed())
      .subscribe(() => this.refreshReports());
  }

  refresh(): void {
    this.refreshReports();
  }
  createReport(): void {
    this.error.set('');
    if (!this.serverId || !this.startTime || !this.endTime) {
      this.error.set('Select a server and report time range.');
      return;
    }
    this.busy.set(true);
    this.reportsApi
      .createReport(
        this.serverId,
        new Date(this.startTime).toISOString(),
        new Date(this.endTime).toISOString(),
      )
      .subscribe({
        next: () => {
          this.busy.set(false);
          this.page.set(1);
          this.refreshReports();
        },
        error: () => {
          this.busy.set(false);
          this.error.set('Report could not be queued. Confirm the time range and try again.');
        },
      });
  }

  download(report: ReportRecord): void {
    this.downloading.set(report.reportId);
    this.reportsApi.downloadReport(report.reportId).subscribe({
      next: (blob) => {
        const url = URL.createObjectURL(blob);
        const anchor = document.createElement('a');
        anchor.href = url;
        anchor.download = `report-${report.reportId}.csv`;
        anchor.click();
        URL.revokeObjectURL(url);
        this.downloading.set(null);
      },
      error: () => {
        this.error.set('Report download failed.');
        this.downloading.set(null);
      },
    });
  }

  changePage(selection: PageSelection): void {
    this.page.set(selection.page);
    this.pageSize.set(selection.pageSize);
    this.refreshReports();
  }
  changeServerFilter(): void {
    this.page.set(1);
    this.refreshReports();
  }
  private refreshReports(): void {
    this.listRequest?.unsubscribe();
    this.loading.set(true);
    this.listRequest = this.reportsApi
      .getReports(this.page(), this.pageSize(), this.filterServerId || undefined)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (result) => {
          this.totalCount.set(result.totalCount);
          const lastPage = Math.max(1, Math.ceil(result.totalCount / this.pageSize()));
          if (this.page() > lastPage) {
            this.page.set(lastPage);
            this.refreshReports();
            return;
          }
          this.reports.set(result.items);
          this.loading.set(false);
          this.error.set('');
        },
        error: () => {
          this.loading.set(false);
          this.error.set('Could not load report status.');
        },
      });
  }
  private localInputValue(date: Date): string {
    const shifted = new Date(date.getTime() - date.getTimezoneOffset() * 60000);
    return shifted.toISOString().slice(0, 16);
  }
}
