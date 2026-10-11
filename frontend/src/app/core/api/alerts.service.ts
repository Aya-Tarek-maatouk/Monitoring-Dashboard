import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { AlertRecord, AlertStatus, Page } from '../models/models';

@Injectable({ providedIn: 'root' })
export class AlertsService {
    constructor(private readonly http: HttpClient) { }
    getAlerts(page = 1, pageSize = 100, status?: AlertStatus): Observable<Page<AlertRecord>> {
        let params = new HttpParams().set('page', page).set('pageSize', pageSize);
        if (status) params = params.set('status', status);
        return this.http.get<Page<AlertRecord>>('/api/v1/alerts', { params });
    }
}