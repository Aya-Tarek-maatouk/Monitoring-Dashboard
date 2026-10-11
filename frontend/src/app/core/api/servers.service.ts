import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { EMPTY, expand, Observable, reduce } from 'rxjs';
import { CreateServerPayload, Page, ServerRecord } from '../models/models';

@Injectable({ providedIn: 'root' })
export class ServersService {
  constructor(private readonly http: HttpClient) {}
  getServers(page = 1, pageSize = 100): Observable<Page<ServerRecord>> {
    return this.http.get<Page<ServerRecord>>('/api/v1/servers', { params: { page, pageSize } });
  }
  getServer(serverId: number): Observable<ServerRecord> {
    return this.http.get<ServerRecord>(`/api/v1/servers/${serverId}`);
  }
  getAllServers(): Observable<ServerRecord[]> {
    return this.getServers(1, 100).pipe(
      expand((result) =>
        result.items.length > 0 && result.page * result.pageSize < result.totalCount
          ? this.getServers(result.page + 1, 100)
          : EMPTY,
      ),
      reduce((servers, result) => [...servers, ...result.items], [] as ServerRecord[]),
    );
  }
  createServer(server: CreateServerPayload): Observable<ServerRecord> {
    return this.http.post<ServerRecord>('/api/v1/servers', server);
  }
  updateServer(serverId: number, server: CreateServerPayload): Observable<ServerRecord> {
    return this.http.put<ServerRecord>(`/api/v1/servers/${serverId}`, server);
  }
  deleteServer(serverId: number): Observable<void> {
    return this.http.delete<void>(`/api/v1/servers/${serverId}`);
  }
}
