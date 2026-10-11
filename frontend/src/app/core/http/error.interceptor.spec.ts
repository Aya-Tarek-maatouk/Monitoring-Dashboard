import { HttpClient, HttpErrorResponse, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { vi } from 'vitest';
import { AuthService } from '../auth/auth.service';
import { errorInterceptor, getApiErrorMessage } from './error.interceptor';
import { ToastService } from '../notifications/toast.service';

describe('Global HTTP error handling', () => {
    it('extracts business messages, validation errors and download errors', async () => {
        expect(await getApiErrorMessage(new HttpErrorResponse({
            status: 409,
            error: { title: 'Conflict.', detail: 'The server has monitoring history.' }
        })))
            .toBe('The server has monitoring history.');
        expect(await getApiErrorMessage(new HttpErrorResponse({
            status: 400,
            error: { errors: { Name: ['Name is required.'] } }
        }))).toBe('Name is required.');
        expect(await getApiErrorMessage(new HttpErrorResponse({
            status: 404,
            error: new Blob([JSON.stringify({ detail: 'The report file is no longer available.' })])
        })))
            .toBe('The report file is no longer available.');
    });

    it('uses safe fallbacks for unexpected errors and disconnected requests', async () => {
        expect(await getApiErrorMessage(new HttpErrorResponse({
            status: 500,
            error: { detail: 'Database password' }
        }))).not.toContain('Database password');
        expect(await getApiErrorMessage(new HttpErrorResponse({ status: 0 }))).toContain('Unable to connect');
        expect(await getApiErrorMessage(new HttpErrorResponse({ status: 403 }))).toContain('permission');
    });

    it('shows a toast and rethrows the original HTTP error', async () => {
        TestBed.configureTestingModule({
            providers: [
                provideHttpClient(withInterceptors([errorInterceptor])), provideHttpClientTesting(),
                provideRouter([]),
            ]
        });
        const http = TestBed.inject(HttpClient);
        const requests = TestBed.inject(HttpTestingController);
        const toast = TestBed.inject(ToastService);
        const response = firstValueFrom(http.get('/api/v1/servers/999')).catch(error => error);
        requests.expectOne('/api/v1/servers/999').flush({ detail: 'The server was not found.' },
            { status: 404, statusText: 'Not Found' });
        const error = await response;
        expect(error).toBeInstanceOf(HttpErrorResponse);
        expect(error.status).toBe(404);
        expect(toast.message()).toBe('The server was not found.');
        toast.dismiss();
        requests.verify();
    });

    it.each([
        [401, '/api/v1/users/me', true],
        [403, '/api/v1/servers', false],
        [401, '/api/v1/auth/login', false],
    ])('handles status %s at %s without unwanted sign-outs', async (status, url, shouldLogout) => {
        let session: { accessToken: string } | null = { accessToken: 'test-token' };
        const logout = vi.fn(() => { session = null; });
        TestBed.configureTestingModule({
            providers: [
                provideHttpClient(withInterceptors([errorInterceptor])), provideHttpClientTesting(), provideRouter([]),
                { provide: AuthService, useValue: { session: () => session, logout } },
            ]
        });
        const navigate = vi.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);
        const requests = TestBed.inject(HttpTestingController);
        const response = firstValueFrom(TestBed.inject(HttpClient).get(url)).catch(error => error);
        requests.expectOne(url).flush({}, { status, statusText: 'Rejected' });
        await response;

        expect(logout).toHaveBeenCalledTimes(shouldLogout ? 1 : 0);
        expect(navigate).toHaveBeenCalledTimes(shouldLogout ? 1 : 0);
        if (shouldLogout) {
            expect(session).toBeNull();
            expect(navigate).toHaveBeenCalledWith(['/login']);
            expect(TestBed.inject(ToastService).message()).toContain('access has changed');
        }
        TestBed.inject(ToastService).dismiss();
        requests.verify();
    });
});