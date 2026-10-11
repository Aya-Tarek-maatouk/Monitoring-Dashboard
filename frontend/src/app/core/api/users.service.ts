import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Page, UserRecord } from '../models/models';

@Injectable({ providedIn: 'root' })
export class UsersService {
    constructor(private readonly http: HttpClient) { }
    getUsers(page = 1, pageSize = 20): Observable<Page<UserRecord>> { return this.http.get<Page<UserRecord>>('/api/v1/users', { params: { page, pageSize } }); }
    changeUserRole(userId: number, roleId: number): Observable<UserRecord> {
        return this.http.put<UserRecord>(`/api/v1/users/${userId}/role`, { roleId });
    }
}