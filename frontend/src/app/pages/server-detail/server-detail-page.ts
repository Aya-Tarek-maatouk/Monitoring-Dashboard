import { DatePipe, DecimalPipe } from '@angular/common';
import {
  Component,
  DestroyRef,
  ElementRef,
  OnDestroy,
  ViewChild,
  inject,
  signal,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Subscription } from 'rxjs';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { LucideDynamicIcon } from '@lucide/angular';
import { Chart, registerables } from 'chart.js';
import { ServersService } from '../../core/api/servers.service';
import { MetricsService } from '../../core/api/metrics.service';
import { MetricRecord, ServerRecord } from '../../core/models/models';
import { RealtimeService } from '../../core/realtime/realtime.service';
import { Pagination, PageSelection } from '../../shared/pagination/pagination';

Chart.register(...registerables);

@Component({
  selector: 'app-server-detail-page',
  imports: [DatePipe, DecimalPipe, RouterLink, LucideDynamicIcon, FormsModule, Pagination],
  templateUrl: './server-detail-page.html',
  styleUrl: './server-detail-page.scss',
})
export class ServerDetailPage implements OnDestroy {
  private chart?: Chart;
  @ViewChild('historyCanvas')
  set historyCanvas(canvas: ElementRef<HTMLCanvasElement> | undefined) {
    if (canvas && !this.chart) {
      this.createChart(canvas);
      this.updateChart();
    }
  }
  private readonly serversApi = inject(ServersService);
  private readonly metricsApi = inject(MetricsService);
  private readonly realtime = inject(RealtimeService);
  readonly server = signal<ServerRecord | null>(null);
  readonly metrics = signal<MetricRecord[]>([]);
  readonly loading = signal(true);
  readonly error = signal('');
  readonly page = signal(1);
  readonly pageSize = signal(20);
  readonly totalCount = signal(0);
  from = '';
  to = '';
  private readonly destroyRef = inject(DestroyRef);
  private listRequest?: Subscription;
  private serverId = Number(inject(ActivatedRoute).snapshot.paramMap.get('serverId'));

  constructor() {
    void this.realtime.joinServer(this.serverId);
    this.refresh();
  }

  ngOnDestroy(): void {
    this.chart?.destroy();
    void this.realtime.leaveServer(this.serverId);
  }

  private createChart(canvas: ElementRef<HTMLCanvasElement>): void {
    this.chart = new Chart(canvas.nativeElement, {
      type: 'line',
      data: {
        labels: [],
        datasets: [
          {
            label: 'CPU',
            data: [],
            borderColor: '#739d39',
            backgroundColor: '#a5cf5c25',
            fill: true,
            tension: 0.32,
            pointRadius: 2,
            borderWidth: 2,
          },
          {
            label: 'Memory',
            data: [],
            borderColor: '#48a99e',
            fill: false,
            tension: 0.32,
            pointRadius: 2,
            borderWidth: 1.7,
          },
          {
            label: 'Disk',
            data: [],
            borderColor: '#d68c56',
            fill: false,
            tension: 0.32,
            pointRadius: 1,
            borderWidth: 1.5,
          },
        ],
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: { legend: { display: false } },
        scales: {
          x: { grid: { display: false } },
          y: { min: 0, max: 100, grid: { color: '#e8ede7' } },
        },
      },
    });
  }

  refresh(): void {
    this.serversApi
      .getServer(this.serverId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (server) => this.server.set(server),
        error: () => this.error.set('Could not load server details.'),
      });
    this.loadMetrics();
  }
  changePage(selection: PageSelection): void {
    this.page.set(selection.page);
    this.pageSize.set(selection.pageSize);
    this.loadMetrics();
  }
  applyRange(): void {
    if (this.from && this.to && new Date(this.from) > new Date(this.to)) {
      this.error.set('From must be earlier than To.');
      return;
    }
    this.page.set(1);
    this.loadMetrics();
  }
  private loadMetrics(): void {
    this.listRequest?.unsubscribe();
    this.loading.set(true);
    this.listRequest = this.metricsApi
      .getMetrics(
        this.serverId,
        this.page(),
        this.pageSize(),
        this.from ? new Date(this.from).toISOString() : undefined,
        this.to ? new Date(this.to).toISOString() : undefined,
      )
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (result) => {
          this.totalCount.set(result.totalCount);
          const lastPage = Math.max(1, Math.ceil(result.totalCount / this.pageSize()));
          if (this.page() > lastPage) {
            this.page.set(lastPage);
            this.loadMetrics();
            return;
          }
          this.metrics.set(result.items);
          this.updateChart();
          this.loading.set(false);
          this.error.set('');
        },
        error: () => {
          this.error.set('Could not load metric history.');
          this.loading.set(false);
        },
      });
  }

  private updateChart(): void {
    if (!this.chart) return;
    const ordered = [...this.metrics()].reverse();
    this.chart.data.labels = ordered.map((metric) =>
      new Date(metric.timestamp).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }),
    );
    this.chart.data.datasets[0].data = ordered.map((metric) => metric.cpuUsage);
    this.chart.data.datasets[1].data = ordered.map((metric) => metric.memoryUsage);
    this.chart.data.datasets[2].data = ordered.map((metric) => metric.diskUsage);
    this.chart.update();
  }
}
