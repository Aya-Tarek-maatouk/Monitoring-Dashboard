import { AfterViewInit, Component, DestroyRef, ElementRef, OnDestroy, ViewChild, computed, effect, inject, signal } from '@angular/core';
import { DatePipe, DecimalPipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { LucideDynamicIcon } from '@lucide/angular';
import { Chart, registerables } from 'chart.js';
import { interval } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ServersService } from '../../core/api/servers.service';
import { AlertsService } from '../../core/api/alerts.service';
import { MetricsService } from '../../core/api/metrics.service';
import { RealtimeService } from '../../core/realtime/realtime.service';
import { AlertRecord, MetricRecord, ServerRecord } from '../../core/models/models';

Chart.register(...registerables);

@Component({ selector: 'app-overview-page', imports: [DatePipe, DecimalPipe, RouterLink, LucideDynamicIcon], templateUrl: './overview-page.html', styleUrl: './overview-page.scss' })
export class OverviewPage implements AfterViewInit, OnDestroy {
  @ViewChild('metricCanvas') private canvas!: ElementRef<HTMLCanvasElement>;
  readonly servers = signal<ServerRecord[]>([]);
  readonly metrics = signal<MetricRecord[]>([]);
  readonly alerts = signal<AlertRecord[]>([]);
  readonly selectedServerId = signal<number | null>(null);
  readonly loading = signal(true);
  readonly error = signal('');
  readonly latest = computed(() => this.metrics().at(0) ?? null);
  readonly upCount = computed(() => this.servers().filter((server) => server.status === 'Up').length);
  readonly downCount = computed(() => this.servers().filter((server) => server.status === 'Down').length);
  private chart?: Chart;
  private joinedServerId: number | null = null;
  private readonly serversApi = inject(ServersService);
  private readonly alertsApi = inject(AlertsService);
  private readonly metricsApi = inject(MetricsService);
  private readonly realtime = inject(RealtimeService);
  private readonly destroyRef = inject(DestroyRef);

  constructor() {
    effect(() => {
      const serverId = this.selectedServerId();
      if (serverId === this.joinedServerId) return;
      if (this.joinedServerId !== null) void this.realtime.leaveServer(this.joinedServerId);
      this.joinedServerId = serverId;
      if (serverId !== null) void this.realtime.joinServer(serverId);
    });
    interval(15000).pipe(takeUntilDestroyed()).subscribe(() => this.refresh());
    effect(() => {
      const event = this.realtime.latestMetric();
      if (event && event.serverId === this.selectedServerId()) this.loadMetrics(event.serverId);
    });
    effect(() => {
      const alert = this.realtime.latestAlert();
      if (alert && !this.alerts().some((item) => item.alertId === alert.alertId)) this.refreshAlerts();
    });
  }

  ngAfterViewInit(): void {
    this.chart = new Chart(this.canvas.nativeElement, {
      type: 'line',
      data: {
        labels: [], datasets: [
          { label: 'CPU', data: [], borderColor: '#739d39', backgroundColor: '#a5cf5c25', fill: true, tension: .36, pointRadius: 2, borderWidth: 2 },
          { label: 'Memory', data: [], borderColor: '#48a99e', backgroundColor: 'transparent', tension: .36, pointRadius: 1, borderWidth: 1.7 },
        ]
      },
      options: { responsive: true, maintainAspectRatio: false, animation: { duration: 250 }, plugins: { legend: { display: false } }, scales: { x: { grid: { display: false }, ticks: { maxTicksLimit: 6, color: '#89958d', font: { family: 'DM Mono', size: 10 } } }, y: { min: 0, max: 100, grid: { color: '#e8ede7' }, ticks: { stepSize: 25, color: '#89958d', font: { family: 'DM Mono', size: 10 }, callback: (value) => `${value}%` } } } },
    });
    this.refresh();
  }

  ngOnDestroy(): void {
    this.chart?.destroy();
    if (this.joinedServerId !== null) void this.realtime.leaveServer(this.joinedServerId);
  }

  selectServer(serverId: number): void { this.selectedServerId.set(serverId); this.loadMetrics(serverId); }
  refresh(): void {
    this.serversApi.getAllServers().subscribe({
      next: (servers) => {
        this.servers.set(servers);
        const selected = this.selectedServerId();
        const serverId = selected && servers.some((server) => server.serverId === selected) ? selected : servers[0]?.serverId ?? null;
        if (serverId !== selected) this.selectedServerId.set(serverId);
        if (serverId !== null) this.loadMetrics(serverId); else { this.metrics.set([]); this.updateChart(); }
        this.loading.set(false);
        this.error.set('');
      },
      error: () => { this.loading.set(false); this.error.set('Could not load monitoring data. Check the API connection and sign in again.'); },
    });
    this.refreshAlerts();
  }

  private refreshAlerts(): void {
    this.alertsApi.getAlerts(1, 100, 'Triggered').subscribe({ next: (page) => this.alerts.set(page.items.slice(0, 5)), error: () => this.alerts.set([]) });
  }

  private loadMetrics(serverId: number): void {
    this.metricsApi.getMetrics(serverId, 1, 24).subscribe({ next: (page) => { this.metrics.set(page.items); this.updateChart(); }, error: () => this.metrics.set([]) });
  }

  private updateChart(): void {
    if (!this.chart) return;
    const ordered = [...this.metrics()].reverse();
    this.chart.data.labels = ordered.map((metric) => new Date(metric.timestamp).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }));
    this.chart.data.datasets[0].data = ordered.map((metric) => metric.cpuUsage);
    this.chart.data.datasets[1].data = ordered.map((metric) => metric.memoryUsage);
    this.chart.update();
  }
}