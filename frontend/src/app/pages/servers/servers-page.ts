import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Subscription } from 'rxjs';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { LucideDynamicIcon } from '@lucide/angular';
import { ServersService } from '../../core/api/servers.service';
import { AuthService } from '../../core/auth/auth.service';
import { ServerRecord, ServerStatus } from '../../core/models/models';
import { Pagination, PageSelection } from '../../shared/pagination/pagination';

@Component({
  selector: 'app-servers-page',
  imports: [DatePipe, FormsModule, RouterLink, LucideDynamicIcon, Pagination],
  templateUrl: './servers-page.html',
  styleUrl: './servers-page.scss',
})
export class ServersPage {
  private readonly serversApi = inject(ServersService);
  readonly auth = inject(AuthService);
  readonly servers = signal<ServerRecord[]>([]);
  readonly loading = signal(true);
  readonly showCreate = signal(false);
  readonly editingServer = signal<ServerRecord | null>(null);
  readonly deleteTarget = signal<ServerRecord | null>(null);
  readonly error = signal('');
  readonly page = signal(1);
  readonly pageSize = signal(20);
  search = '';
  private readonly destroyRef = inject(DestroyRef);
  private listRequest?: Subscription;
  name = '';
  ipAddress = '';
  description = '';
  filter: 'All' | ServerStatus = 'All';
  constructor() {
    this.refresh();
  }
  get filteredServers(): ServerRecord[] {
    const query = this.search.trim().toLowerCase();
    return this.servers().filter(
      (server) =>
        (this.filter === 'All' || server.status === this.filter) &&
        (!query ||
          [server.name, server.ipAddress, server.description].some((value) =>
            value?.toLowerCase().includes(query),
          )),
    );
  }
  get visibleServers(): ServerRecord[] {
    return this.filteredServers.slice(
      (this.page() - 1) * this.pageSize(),
      this.page() * this.pageSize(),
    );
  }
  changePage(selection: PageSelection): void {
    this.page.set(selection.page);
    this.pageSize.set(selection.pageSize);
  }
  changeFilter(filter: 'All' | ServerStatus): void {
    this.filter = filter;
    this.page.set(1);
  }
  refresh(): void {
    this.listRequest?.unsubscribe();
    this.loading.set(true);
    this.listRequest = this.serversApi
      .getAllServers()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (servers) => {
          this.servers.set(servers);
          this.page.set(
            Math.min(
              this.page(),
              Math.max(1, Math.ceil(this.filteredServers.length / this.pageSize())),
            ),
          );
          this.loading.set(false);
          this.error.set('');
        },
        error: () => {
          this.error.set('Server inventory could not be loaded.');
          this.loading.set(false);
        },
      });
  }
  openCreate(): void {
    this.name = '';
    this.ipAddress = '';
    this.description = '';
    this.editingServer.set(null);
    this.error.set('');
    this.showCreate.set(true);
  }

  openEdit(server: ServerRecord): void {
    this.name = server.name;
    this.ipAddress = server.ipAddress ?? '';
    this.description = server.description ?? '';
    this.editingServer.set(server);
    this.error.set('');
    this.showCreate.set(true);
  }

  closeEditor(): void {
    this.showCreate.set(false);
    this.editingServer.set(null);
  }

  saveServer(): void {
    this.error.set('');
    const editing = this.editingServer();
    const payload = {
      name: this.name.trim(),
      ipAddress: this.ipAddress.trim() || null,
      description: this.description.trim() || null,
      status: editing?.status ?? ('Up' as ServerStatus),
    };
    const save = editing
      ? this.serversApi.updateServer(editing.serverId, payload)
      : this.serversApi.createServer(payload);
    save.subscribe({
      next: () => {
        this.closeEditor();
        this.refresh();
      },
      error: () =>
        this.error.set(
          `Could not ${editing ? 'update' : 'create'} server. Check the servers.manage permission.`,
        ),
    });
  }

  confirmDelete(): void {
    const target = this.deleteTarget();
    if (!target) return;
    this.error.set('');
    this.serversApi.deleteServer(target.serverId).subscribe({
      next: () => {
        this.deleteTarget.set(null);
        this.refresh();
      },
      error: (error: HttpErrorResponse) => {
        this.deleteTarget.set(null);
        this.error.set(
          error.status === 409
            ? 'This server has monitoring history and cannot be deleted.'
            : 'Could not delete server. Check the servers.manage permission.',
        );
      },
    });
  }
}
