import { computed, Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { AuthenticationResponse } from '../models/models';

const SESSION_KEY = 'monitoring-dashboard-session';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly state = signal<AuthenticationResponse | null>(this.restoreSession());
  readonly session = this.state.asReadonly();
  readonly isAuthenticated = computed(() => this.state() !== null);
  readonly isAdmin = computed(() => this.state()?.permissions.includes('users.manage') ?? false);

  constructor(private readonly http: HttpClient) { }

  login(userNameOrEmail: string, password: string): Observable<AuthenticationResponse> {
    return this.http.post<AuthenticationResponse>('/api/v1/auth/login', { userNameOrEmail, password }).pipe(
      tap(session => this.saveSession(session)),
    );
  }

  register(userName: string, email: string, password: string): Observable<AuthenticationResponse> {
    return this.http.post<AuthenticationResponse>('/api/v1/auth/register', { userName, email, password }).pipe(
      tap(session => this.saveSession(session)),
    );
  }

  logout(): void {
    localStorage.removeItem(SESSION_KEY);
    this.state.set(null);
  }

  private saveSession(session: AuthenticationResponse): void {
    localStorage.setItem(SESSION_KEY, JSON.stringify(session));
    this.state.set(session);
  }

  private restoreSession(): AuthenticationResponse | null {
    const stored = localStorage.getItem(SESSION_KEY);
    if (!stored) return null;
    try {
      const session = JSON.parse(stored) as AuthenticationResponse;
      if (new Date(session.expiresAt).getTime() <= Date.now()) {
        localStorage.removeItem(SESSION_KEY);
        return null;
      }
      return session;
    } catch {
      localStorage.removeItem(SESSION_KEY);
      return null;
    }
  }
}