import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { App } from './app';
import { LucideCircleAlert, LucideX, provideLucideIcons } from '@lucide/angular';
import { ToastService } from './core/notifications/toast.service';

describe('App', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [App],
      providers: [provideRouter([]), provideLucideIcons(LucideCircleAlert, LucideX)],
    }).compileComponents();
  });

  it('should create the app', () => {
    const fixture = TestBed.createComponent(App);
    const app = fixture.componentInstance;
    expect(app).toBeTruthy();
  });

  it('should render the router outlet', async () => {
    const fixture = TestBed.createComponent(App);
    await fixture.whenStable();
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('router-outlet')).not.toBeNull();
  });

  it('renders and dismisses a global error toast', async () => {
    const fixture = TestBed.createComponent(App);
    const toast = TestBed.inject(ToastService);
    toast.showError('The server was not found.');
    await fixture.whenStable();
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('.error-toast')?.textContent).toContain('The server was not found.');
    compiled.querySelector<HTMLButtonElement>('.error-toast button')?.click();
    await fixture.whenStable();
    expect(compiled.querySelector('.error-toast')).toBeNull();
  });
});
