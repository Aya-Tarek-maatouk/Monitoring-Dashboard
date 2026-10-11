import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, DestroyRef, inject, signal } from '@angular/core';
import { LucideDynamicIcon } from '@lucide/angular';
import { interval, Subscription } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { JobsService } from '../../core/api/jobs.service';
import { AuthService } from '../../core/auth/auth.service';
import { BackgroundJobCounts, BackgroundJobRecord } from '../../core/models/models';
import { Pagination, PageSelection } from '../../shared/pagination/pagination';

@Component({
  selector: 'app-jobs-page',
  imports: [DatePipe, LucideDynamicIcon, Pagination],
  templateUrl: './jobs-page.html',
  styleUrl: './jobs-page.scss',
})
export class JobsPage {
  private readonly jobsApi = inject(JobsService);
  readonly auth = inject(AuthService);
  readonly jobs = signal<BackgroundJobRecord[]>([]);
  readonly counts = signal<BackgroundJobCounts>({
    enqueued: 0,
    scheduled: 0,
    processing: 0,
    succeeded: 0,
    failed: 0,
  });
  readonly lastRun = signal<Date | null>(null);
  readonly error = signal('');
  readonly retryingJobId = signal<string | null>(null);
  readonly message = signal('');
  readonly page = signal(1);
  readonly pageSize = signal(20);
  readonly totalCount = signal(0);
  readonly loading = signal(false);
  private readonly destroyRef = inject(DestroyRef);
  private listRequest?: Subscription;
  constructor() {
    this.refresh();
    interval(5000)
      .pipe(takeUntilDestroyed())
      .subscribe(() => this.refresh());
  }
  refresh(): void {
    this.listRequest?.unsubscribe();
    this.loading.set(true);
    this.listRequest = this.jobsApi
      .getBackgroundJobs(this.page(), this.pageSize())
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (result) => {
          this.totalCount.set(result.totalCount);
          const lastPage = Math.max(1, Math.ceil(result.totalCount / this.pageSize()));
          if (this.page() > lastPage) {
            this.page.set(lastPage);
            this.refresh();
            return;
          }
          this.jobs.set(result.items);
          this.counts.set(result.counts);
          this.lastRun.set(new Date(result.sampledAt));
          this.error.set('');
          this.loading.set(false);
        },
        error: () => {
          this.loading.set(false);
          this.error.set('Could not load Hangfire job history. Check the jobs.read permission.');
        },
      });
  }
  changePage(selection: PageSelection): void {
    this.page.set(selection.page);
    this.pageSize.set(selection.pageSize);
    this.refresh();
  }
  retry(job: BackgroundJobRecord): void {
    this.retryingJobId.set(job.jobId);
    this.error.set('');
    this.message.set('');
    this.jobsApi.retryBackgroundJob(job.jobId).subscribe({
      next: () => {
        this.message.set(`Job ${job.jobId} was requeued.`);
        this.retryingJobId.set(null);
        this.refresh();
      },
      error: (error: HttpErrorResponse) => {
        this.retryingJobId.set(null);
        this.error.set(
          error.status === 409
            ? 'That job is no longer in a failed state.'
            : 'Retry failed. Check the jobs.retry permission.',
        );
      },
    });
  }
}
