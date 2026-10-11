import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Page, ReportRecord } from '../models/models';

@Injectable({ providedIn: 'root' })
export class ReportsService {
  constructor(private readonly http: HttpClient) {}
  getReports(page = 1, pageSize = 100, serverId?: number): Observable<Page<ReportRecord>> {
    let params = new HttpParams().set('page', page).set('pageSize', pageSize);
    if (serverId) params = params.set('serverId', serverId);
    return this.http.get<Page<ReportRecord>>('/api/v1/reports', { params });
  }
  createReport(serverId: number, startTime: string, endTime: string): Observable<ReportRecord> {
    return this.http.post<ReportRecord>(`/api/v1/servers/${serverId}/reports`, {
      startTime,
      endTime,
    });
  }
  downloadReport(reportId: number): Observable<Blob> {
    return this.http.get(`/api/v1/reports/${reportId}/download`, { responseType: 'blob' });
  }
}
