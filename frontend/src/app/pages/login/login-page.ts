import { Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize } from 'rxjs';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { LucideDynamicIcon } from '@lucide/angular';
import { AuthService } from '../../core/auth/auth.service';

@Component({ selector: 'app-login-page', imports: [FormsModule, LucideDynamicIcon], templateUrl: './login-page.html', styleUrl: './login-page.scss' })
export class LoginPage {
  private readonly destroyRef = inject(DestroyRef);
  readonly isRegister = signal(false);
  readonly busy = signal(false);
  userName = '';
  email = '';
  password = '';
  constructor(private readonly auth: AuthService, private readonly router: Router) { }
  submit(): void {
    if (this.busy()) return;
    this.busy.set(true);
    const request = this.isRegister()
      ? this.auth.register(this.userName, this.email, this.password)
      : this.auth.login(this.userName, this.password);
    request.pipe(
      takeUntilDestroyed(this.destroyRef),
      finalize(() => this.busy.set(false)),
    ).subscribe({
      next: () => { void this.router.navigate(['/overview']); },
      error: () => { this.busy.set(false); },
    });
  }
}