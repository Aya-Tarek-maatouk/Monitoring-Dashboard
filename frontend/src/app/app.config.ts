import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import {
  LucideActivity,
  LucideArrowUpRight,
  LucideBell,
  LucideChevronDown,
  LucideChevronRight,
  LucideCircleAlert,
  LucideCircleCheck,
  LucideClock3,
  LucideCpu,
  LucideDownload,
  LucideFileText,
  LucideLayoutDashboard,
  LucideLogOut,
  LucidePencil,
  LucidePlus,
  LucideRefreshCw,
  LucideServer,
  LucideSave,
  LucideShieldCheck,
  LucideUsers,
  LucideTrash2,
  LucideX,
  LucideZap,
  provideLucideIcons,
} from '@lucide/angular';

import { routes } from './app.routes';
import { authInterceptor } from './core/auth/auth.interceptor';
import { errorInterceptor } from './core/http/error.interceptor';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideHttpClient(withInterceptors([errorInterceptor, authInterceptor])),
    provideLucideIcons(LucideActivity, LucideServer, LucideBell, LucideFileText, LucideUsers,
      LucideClock3, LucideLogOut, LucideArrowUpRight, LucideShieldCheck, LucideChevronRight,
      LucideLayoutDashboard, LucideCircleAlert, LucideChevronDown, LucideCircleCheck, LucideCpu,
      LucidePlus, LucidePencil, LucideRefreshCw, LucideTrash2, LucideX, LucideDownload, LucideZap, LucideSave),
  ]
};
