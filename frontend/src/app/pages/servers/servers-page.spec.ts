import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { ServersService } from '../../core/api/servers.service';
import { AuthService } from '../../core/auth/auth.service';
import { ServersPage } from './servers-page';

describe('Server inventory pagination', () => {
  it('filters the complete inventory before paging and resets on health-filter changes', () => {
    const servers = Array.from({ length: 101 }, (_, index) => ({
      serverId: index + 1,
      name: `host-${index + 1}`,
      ipAddress: null,
      description: null,
      status: index === 100 ? 'Down' : 'Up',
      createdAt: '2026-01-01T00:00:00Z',
    }));
    TestBed.configureTestingModule({
      imports: [ServersPage],
      providers: [
        { provide: ServersService, useValue: { getAllServers: () => of(servers) } },
        { provide: AuthService, useValue: {} },
      ],
    });
    TestBed.overrideComponent(ServersPage, { set: { template: '', imports: [] } });
    const fixture = TestBed.createComponent(ServersPage);
    const component = fixture.componentInstance;
    component.changePage({ page: 2, pageSize: 20 });
    expect(component.visibleServers[0].serverId).toBe(21);
    component.changeFilter('Down');
    expect(component.page()).toBe(1);
    expect(component.filteredServers.length).toBe(1);
    expect(component.visibleServers[0].serverId).toBe(101);
    component.changeFilter('All');
    component.search = 'host-101';
    expect(component.filteredServers.length).toBe(1);
    expect(component.visibleServers[0].serverId).toBe(101);
    fixture.destroy();
  });
});
