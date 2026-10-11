import { provideHttpClient } from '@angular/common/http';
import { ServerRecord } from '../models/models';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { AlertsService } from './alerts.service';
import { JobsService } from './jobs.service';
import { MetricsService } from './metrics.service';
import { ReportsService } from './reports.service';
import { ServersService } from './servers.service';

describe('API pagination', () => {
  it('loads every server page instead of truncating dropdowns at 100 hosts', () => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    const requests = TestBed.inject(HttpTestingController);
    let servers: ServerRecord[] = [];
    TestBed.inject(ServersService)
      .getAllServers()
      .subscribe((result) => {
        servers = result;
      });
    const first = requests.expectOne(
      (request) => request.url === '/api/v1/servers' && request.params.get('page') === '1',
    );
    first.flush({
      items: Array.from({ length: 100 }, (_, index) => ({ serverId: index + 1 })),
      page: 1,
      pageSize: 100,
      totalCount: 101,
    });
    const second = requests.expectOne(
      (request) => request.url === '/api/v1/servers' && request.params.get('page') === '2',
    );
    second.flush({ items: [{ serverId: 101 }], page: 2, pageSize: 100, totalCount: 101 });
    expect(servers.length).toBe(101);
    expect(servers[100].serverId).toBe(101);
    requests.verify();
  });

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
  });

  it.each(['jobs', 'servers', 'reports', 'alerts', 'metrics'])(
    '%s sends the requested page and page size',
    (resource) => {
      switch (resource) {
        case 'jobs':
          TestBed.inject(JobsService).getBackgroundJobs(2, 20).subscribe();
          break;
        case 'servers':
          TestBed.inject(ServersService).getServers(2, 20).subscribe();
          break;
        case 'reports':
          TestBed.inject(ReportsService).getReports(2, 20).subscribe();
          break;
        case 'alerts':
          TestBed.inject(AlertsService).getAlerts(2, 20, 'Triggered').subscribe();
          break;
        case 'metrics':
          TestBed.inject(MetricsService)
            .getMetrics(7, 2, 20, '2026-01-01', '2026-01-02')
            .subscribe();
          break;
      }
      const requests = TestBed.inject(HttpTestingController);
      const request = requests.expectOne(
        (request) =>
          request.url ===
          (resource === 'metrics' ? '/api/v1/servers/7/metrics' : `/api/v1/${resource}`),
      );
      expect(request.request.params.get('page')).toBe('2');
      expect(request.request.params.get('pageSize')).toBe('20');
      if (resource === 'alerts') expect(request.request.params.get('status')).toBe('Triggered');
      if (resource === 'metrics') {
        expect(request.request.params.get('from')).toBe('2026-01-01');
        expect(request.request.params.get('to')).toBe('2026-01-02');
      }
      request.flush({ items: [], page: 2, pageSize: 20, totalCount: 0 });
      requests.verify();
    },
  );
});
