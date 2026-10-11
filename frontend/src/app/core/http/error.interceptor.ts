import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, from, mergeMap, throwError } from 'rxjs';
import { AuthService } from '../auth/auth.service';
import { ToastService } from '../notifications/toast.service';

export async function getApiErrorMessage(error: HttpErrorResponse): Promise<string> {
    if (error.status === 0) return 'Unable to connect to the server. Please try again.';
    if (error.status >= 500) return 'An unexpected error occurred. Please try again later.';

    let body: unknown = error.error;
    if (body instanceof Blob) {
        try { body = JSON.parse(await body.text()); }
        catch { body = null; }
    }
    if (body && typeof body === 'object') {
        const problem = body as Record<string, unknown>;
        if (problem['errors'] && typeof problem['errors'] === 'object') {
            const messages = Object.values(problem['errors']).flat().filter(
                (message): message is string => typeof message === 'string' && message.trim().length > 0);
            if (messages.length) return messages.join(' ');
        }
        if (typeof problem['detail'] === 'string' && problem['detail'].trim()) return problem['detail'];
        if (typeof problem['title'] === 'string' && problem['title'].trim()) return problem['title'];
    }
    switch (error.status) {
        case 400: return 'The request is invalid. Check the entered details.';
        case 401: return 'Authentication failed. Please sign in again.';
        case 403: return 'You do not have permission to perform this action.';
        case 404: return 'The requested resource was not found.';
        case 409: return 'The action conflicts with the current resource state.';
        case 429: return 'Too many requests. Please try again shortly.';
        default: return 'The request could not be completed. Please try again.';
    }
}

export const errorInterceptor: HttpInterceptorFn = (request, next) => {
    const toast = inject(ToastService);
    const auth = inject(AuthService);
    const router = inject(Router);
    const accessToken = auth.session()?.accessToken;
    return next(request).pipe(catchError((error: unknown) => {
        if (!(error instanceof HttpErrorResponse) || !request.url.startsWith('/api/')) {
            return throwError(() => error);
        }
        if (error.status === 401 && accessToken && auth.session()?.accessToken === accessToken
            && !request.url.startsWith('/api/v1/auth/')) {
            auth.logout();
            toast.showError('Your session has expired or your access has changed. Please sign in again.');
            void router.navigate(['/login']);
            return throwError(() => error);
        }
        return from(getApiErrorMessage(error)).pipe(mergeMap(message => {
            toast.showError(message);
            return throwError(() => error);
        }));
    }));
};