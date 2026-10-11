import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { vi } from 'vitest';
import { ReportsPage } from './reports-page';

describe('ReportsPage pagination', () => {
  beforeEach(() => {
    vi.useFakeTimers();
    TestBed.configureTestingModule({
      imports: [ReportsPage],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    TestBed.overrideComponent(ReportsPage, { set: { template: '', imports: [] } });
  });
  afterEach(() => {
    TestBed.inject(HttpTestingController).verify({ ignoreCancelled: true });
    vi.useRealTimers();
  });

  it('preserves page and size when polling and resets the page for a server filter', () => {
    const fixture = TestBed.createComponent(ReportsPage);
    const component = fixture.componentInstance;
    const requests = TestBed.inject(HttpTestingController);
    requests
      .expectOne((request) => request.url === '/api/v1/servers')
      .flush({ items: [], page: 1, pageSize: 100, totalCount: 0 });
    requests
      .expectOne((request) => request.url === '/api/v1/reports')
      .flush({ items: [], page: 1, pageSize: 20, totalCount: 80 });
    component.changePage({ page: 3, pageSize: 10 });
    requests
      .expectOne(
        (request) => request.params.get('page') === '3' && request.params.get('pageSize') === '10',
      )
      .flush({ items: [], page: 3, pageSize: 10, totalCount: 80 });
    vi.advanceTimersByTime(5000);
    requests
      .expectOne(
        (request) => request.url === '/api/v1/reports' && request.params.get('page') === '3',
      )
      .flush({ items: [], page: 3, pageSize: 10, totalCount: 80 });
    component.filterServerId = 7;
    component.changeServerFilter();
    requests
      .expectOne(
        (request) => request.params.get('serverId') === '7' && request.params.get('page') === '1',
      )
      .flush({ items: [], page: 1, pageSize: 10, totalCount: 0 });
    expect(component.totalCount()).toBe(0);
    expect(component.loading()).toBe(false);
    fixture.destroy();
  });

  it('cancels stale requests and clamps a page when results shrink', () => {
    const fixture = TestBed.createComponent(ReportsPage);
    const component = fixture.componentInstance;
    const requests = TestBed.inject(HttpTestingController);
    requests
      .expectOne((request) => request.url === '/api/v1/servers')
      .flush({ items: [], page: 1, pageSize: 100, totalCount: 0 });
    const first = requests.expectOne((request) => request.url === '/api/v1/reports');
    component.changePage({ page: 4, pageSize: 20 });
    expect(first.cancelled).toBe(true);
    requests
      .expectOne((request) => request.params.get('page') === '4')
      .flush({ items: [], page: 4, pageSize: 20, totalCount: 21 });
    requests
      .expectOne((request) => request.params.get('page') === '2')
      .flush({ items: [], page: 2, pageSize: 20, totalCount: 21 });
    expect(component.page()).toBe(2);
    fixture.destroy();
  });
});
