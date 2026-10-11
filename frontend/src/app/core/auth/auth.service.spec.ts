import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { isObservable } from 'rxjs';
import { AuthService } from './auth.service';

describe('AuthService', () => {
  const session = {
    userId: 1, userName: 'demo-user', role: 'User', permissions: ['servers.read'],
    accessToken: 'test-token', expiresAt: '2099-01-01T00:00:00Z',
  };

  beforeEach(() => {
    localStorage.removeItem('monitoring-dashboard-session');
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
  });

  afterEach(() => {
    TestBed.inject(HttpTestingController).verify();
    localStorage.removeItem('monitoring-dashboard-session');
  });

  it.each(['login', 'register'])('%s returns an Observable and saves the successful session', method => {
    const auth = TestBed.inject(AuthService);
    const result = method === 'login'
      ? auth.login('demo-user', 'test-password')
      : auth.register('demo-user', 'demo@example.test', 'test-password');
    expect(isObservable(result)).toBe(true);
    expect(auth.session()).toBeNull();
    let received: unknown;
    result.subscribe(response => { received = response; });
    const request = TestBed.inject(HttpTestingController).expectOne(`/api/v1/auth/${method}`);
    expect(request.request.method).toBe('POST');
    request.flush(session);
    expect(received).toEqual(session);
    expect(auth.session()).toEqual(session);
    expect(JSON.parse(localStorage.getItem('monitoring-dashboard-session')!)).toEqual(session);
  });

  it('propagates errors without saving a session', () => {
    const auth = TestBed.inject(AuthService);
    let status: number | undefined;
    auth.login('demo-user', 'wrong-password').subscribe({ error: error => { status = error.status; } });
    TestBed.inject(HttpTestingController).expectOne('/api/v1/auth/login')
      .flush({}, { status: 401, statusText: 'Unauthorized' });
    expect(status).toBe(401);
    expect(auth.session()).toBeNull();
    expect(localStorage.getItem('monitoring-dashboard-session')).toBeNull();
  });
});