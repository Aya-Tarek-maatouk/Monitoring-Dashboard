import { DatePipe } from '@angular/common';
import { Component, DestroyRef, ElementRef, inject, signal, viewChild } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { forkJoin, Subscription } from 'rxjs';
import { LucideDynamicIcon } from '@lucide/angular';
import { RolesService } from '../../core/api/roles.service';
import { UsersService } from '../../core/api/users.service';
import { AuthService } from '../../core/auth/auth.service';
import { ToastService } from '../../core/notifications/toast.service';
import { PermissionRecord, RoleRecord, UserRecord } from '../../core/models/models';
import { Pagination, PageSelection } from '../../shared/pagination/pagination';

@Component({
  selector: 'app-users-page',
  imports: [DatePipe, FormsModule, LucideDynamicIcon, Pagination],
  templateUrl: './users-page.html',
  styleUrl: './users-page.scss',
})
export class UsersPage {
  private readonly rolesApi = inject(RolesService);
  private readonly usersApi = inject(UsersService);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);
  readonly assignmentDialog = viewChild<ElementRef<HTMLDialogElement>>('assignmentDialog');
  readonly createDialog = viewChild<ElementRef<HTMLDialogElement>>('createDialog');
  readonly users = signal<UserRecord[]>([]);
  readonly roles = signal<RoleRecord[]>([]);
  readonly permissions = signal<PermissionRecord[]>([]);
  readonly loading = signal(true);
  readonly error = signal('');
  readonly message = signal('');
  readonly page = signal(1);
  readonly totalCount = signal(0);
  readonly pageSize = signal(20);
  private readonly destroyRef = inject(DestroyRef);
  private listRequest?: Subscription;
  readonly drafts = signal<Partial<Record<number, number>>>({});
  readonly pendingUser = signal<UserRecord | null>(null);
  readonly saving = signal(false);
  readonly creating = signal(false);
  readonly selectedPermissions = signal<number[]>([]);
  roleName = '';
  roleDescription = '';
  constructor() {
    this.refresh();
  }
  refresh(): void {
    this.listRequest?.unsubscribe();
    this.loading.set(true);
    this.error.set('');
    this.listRequest = forkJoin({
      users: this.usersApi.getUsers(this.page(), this.pageSize()),
      roles: this.rolesApi.getRoles(),
      permissions: this.rolesApi.getPermissions(),
    })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (result) => {
          const lastPage = Math.max(1, Math.ceil(result.users.totalCount / this.pageSize()));
          if (this.page() > lastPage) {
            this.page.set(lastPage);
            this.refresh();
            return;
          }
          this.users.set(result.users.items);
          this.totalCount.set(result.users.totalCount);
          this.roles.set(result.roles);
          this.permissions.set(result.permissions);
          this.drafts.set({});
          this.loading.set(false);
        },
        error: () => {
          this.error.set('Accounts and roles could not be loaded.');
          this.loading.set(false);
        },
      });
  }
  changePage(selection: PageSelection): void {
    this.page.set(selection.page);
    this.pageSize.set(selection.pageSize);
    this.refresh();
  }
  selectRole(userId: number, event: Event): void {
    this.drafts.update((drafts) => ({
      ...drafts,
      [userId]: Number((event.target as HTMLSelectElement).value),
    }));
  }
  roleLabel(roleId: number | undefined): string {
    return this.roles().find((role) => role.roleId === roleId)?.name ?? '';
  }
  requestChange(user: UserRecord): void {
    this.pendingUser.set(user);
    this.assignmentDialog()?.nativeElement.showModal();
  }
  cancelChange(): void {
    if (this.saving()) return;
    const dialog = this.assignmentDialog()?.nativeElement;
    if (dialog?.open) dialog.close();
    this.pendingUser.set(null);
  }
  confirmChange(): void {
    const user = this.pendingUser();
    if (!user || this.saving()) return;
    const roleId = this.drafts()[user.userId];
    if (!roleId || roleId === user.roleId) return;
    this.saving.set(true);
    this.usersApi.changeUserRole(user.userId, roleId).subscribe({
      next: (updated) => {
        this.users.update((users) =>
          users.map((account) => (account.userId === updated.userId ? updated : account)),
        );
        this.saving.set(false);
        this.cancelChange();
        this.message.set(`${updated.userName} is now ${updated.role}.`);
        if (this.auth.session()?.userId === updated.userId) {
          this.auth.logout();
          this.toast.showError('Your role has changed. Please sign in again.');
          void this.router.navigate(['/login']);
        }
      },
      error: () => {
        this.saving.set(false);
        this.cancelChange();
      },
    });
  }
  openCreate(): void {
    this.roleName = '';
    this.roleDescription = '';
    this.selectedPermissions.set([]);
    this.createDialog()?.nativeElement.showModal();
  }
  closeCreate(): void {
    if (!this.creating()) this.createDialog()?.nativeElement.close();
  }
  togglePermission(permissionId: number, event: Event): void {
    const checked = (event.target as HTMLInputElement).checked;
    this.selectedPermissions.update((ids) =>
      checked ? [...ids, permissionId] : ids.filter((id) => id !== permissionId),
    );
  }
  createRole(): void {
    if (!this.roleName.trim() || this.creating()) return;
    this.creating.set(true);
    this.rolesApi
      .createRole(this.roleName.trim(), this.roleDescription.trim(), this.selectedPermissions())
      .subscribe({
        next: (role) => {
          this.roles.update((roles) =>
            [...roles, role].sort((first, second) => first.name.localeCompare(second.name)),
          );
          this.creating.set(false);
          this.closeCreate();
          this.message.set(`Role ${role.name} created.`);
        },
        error: () => this.creating.set(false),
      });
  }
}
