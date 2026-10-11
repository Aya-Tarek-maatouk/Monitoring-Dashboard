import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { LucidePlus, LucideSave, provideLucideIcons } from '@lucide/angular';
import { of, throwError } from 'rxjs';
import { vi } from 'vitest';
import { UsersService } from '../../core/api/users.service';
import { RolesService } from '../../core/api/roles.service';
import { AuthService } from '../../core/auth/auth.service';
import { UsersPage } from './users-page';

describe('UsersPage', () => {
    const permission = { permissionId: 5, name: 'servers.read', description: 'View servers' };
    const role = { roleId: 17, name: 'Observer', description: null, permissions: [permission] };
    const user = { userId: 2, userName: 'demo-user', email: 'demo@example.test', roleId: 3, role: 'User', permissions: [], createdAt: '2026-10-10T00:00:00Z' };
    const api = { getUsers: vi.fn(), getRoles: vi.fn(), getPermissions: vi.fn(), changeUserRole: vi.fn(), createRole: vi.fn() };
    const auth = { session: () => ({ userId: 1 }), logout: vi.fn() };

    beforeEach(async () => {
        vi.clearAllMocks();
        api.getUsers.mockReturnValue(of({ items: [user], totalCount: 1, page: 1, pageSize: 20 }));
        api.getRoles.mockReturnValue(of([role, { ...role, roleId: 3, name: 'User' }]));
        api.getPermissions.mockReturnValue(of([permission]));
        await TestBed.configureTestingModule({
            imports: [UsersPage], providers: [
                provideRouter([]), provideLucideIcons(LucidePlus, LucideSave),
                { provide: UsersService, useValue: api }, { provide: RolesService, useValue: api }, { provide: AuthService, useValue: auth },
            ]
        }).compileComponents();
    });

    it('loads dynamic roles with IDs as option values', async () => {
        const fixture = TestBed.createComponent(UsersPage);
        await fixture.whenStable();
        const option = fixture.nativeElement.querySelector('option[value="17"]');
        expect(option.textContent).toContain('Observer');
        expect(fixture.nativeElement.querySelector('.row-action').disabled).toBe(true);
    });

    it('assigns a custom role by ID', async () => {
        const fixture = TestBed.createComponent(UsersPage);
        await fixture.whenStable();
        const component = fixture.componentInstance;
        api.changeUserRole.mockReturnValue(of({ ...user, roleId: 17, role: 'Observer' }));
        component.drafts.set({ 2: 17 });
        component.pendingUser.set(user);
        component.confirmChange();
        expect(api.changeUserRole).toHaveBeenCalledWith(2, 17);
        expect(component.users()[0].roleId).toBe(17);
        expect(auth.logout).not.toHaveBeenCalled();
    });

    it('creates a role using selected permission IDs', async () => {
        const fixture = TestBed.createComponent(UsersPage);
        await fixture.whenStable();
        const component = fixture.componentInstance;
        const dialog = fixture.nativeElement.querySelector('dialog[aria-labelledby="create-role-title"]');
        dialog.close = vi.fn();
        component.roleName = ' Auditor ';
        component.roleDescription = 'Read-only';
        component.selectedPermissions.set([5]);
        api.createRole.mockReturnValue(of({ ...role, roleId: 20, name: 'Auditor' }));
        component.createRole();
        expect(api.createRole).toHaveBeenCalledWith('Auditor', 'Read-only', [5]);
        expect(component.roles().some(role => role.roleId === 20)).toBe(true);
    });

    it('preserves the account when assignment fails', async () => {
        const fixture = TestBed.createComponent(UsersPage);
        await fixture.whenStable();
        const component = fixture.componentInstance;
        api.changeUserRole.mockReturnValue(throwError(() => new Error('Conflict')));
        component.drafts.set({ 2: 17 });
        component.pendingUser.set(user);
        component.confirmChange();
        expect(component.users()[0].roleId).toBe(3);
        expect(component.saving()).toBe(false);
    });
});