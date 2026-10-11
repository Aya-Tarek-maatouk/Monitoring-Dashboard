import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { MetricRecord, Page } from '../models/models';

@Injectable({ providedIn: 'root' })
export class MetricsService {
    constructor(private readonly http: HttpClient) { }
    getMetrics(serverId: number, page = 1, pageSize = 30, from?: string, to?: string): Observable<Page<MetricRecord>> {
        let params = new HttpParams().set('page', page).set('pageSize', pageSize);
        if (from) params = params.set('from', from);
        if (to) params = params.set('to', to);
        return this.http.get<Page<MetricRecord>>(`/api/v1/servers/${serverId}/metrics`, { params });
    }
}