import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { BackgroundJobPage } from '../models/models';

@Injectable({ providedIn: 'root' })
export class JobsService {
    constructor(private readonly http: HttpClient) { }
    getBackgroundJobs(page = 1, pageSize = 100): Observable<BackgroundJobPage> { return this.http.get<BackgroundJobPage>('/api/v1/jobs', { params: { page, pageSize } }); }
    retryBackgroundJob(jobId: string): Observable<void> { return this.http.post<void>(`/api/v1/jobs/${encodeURIComponent(jobId)}/retry`, {}); }
}