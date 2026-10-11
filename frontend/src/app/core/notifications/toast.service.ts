import { Injectable, signal } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class ToastService {
    readonly message = signal('');
    private timer?: ReturnType<typeof setTimeout>;

    showError(message: string): void {
        if (this.message() === message) return;
        this.dismiss();
        this.message.set(message);
        this.timer = setTimeout(() => this.dismiss(), 8000);
    }

    dismiss(): void {
        clearTimeout(this.timer);
        this.timer = undefined;
        this.message.set('');
    }
}