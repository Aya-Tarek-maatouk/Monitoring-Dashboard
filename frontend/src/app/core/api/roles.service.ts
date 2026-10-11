import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { PermissionRecord, RoleRecord } from '../models/models';

@Injectable({ providedIn: 'root' })
export class RolesService {
    constructor(private readonly http: HttpClient) { }
    getRoles(): Observable<RoleRecord[]> { return this.http.get<RoleRecord[]>('/api/v1/roles'); }
    getPermissions(): Observable<PermissionRecord[]> { return this.http.get<PermissionRecord[]>('/api/v1/roles/permissions'); }
    createRole(name: string, description: string, permissionIds: number[]): Observable<RoleRecord> {
        return this.http.post<RoleRecord>('/api/v1/roles', { name, description, permissionIds });
    }
}