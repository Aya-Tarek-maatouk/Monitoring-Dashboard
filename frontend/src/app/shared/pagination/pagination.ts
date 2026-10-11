import { Component, computed, input, output } from '@angular/core';

export interface PageSelection {
  page: number;
  pageSize: number;
}

@Component({
  selector: 'app-pagination',
  templateUrl: './pagination.html',
  styleUrl: './pagination.scss',
})
export class Pagination {
  readonly page = input(1);
  readonly pageSize = input(20);
  readonly totalCount = input(0);
  readonly loading = input(false);
  readonly pageChange = output<PageSelection>();
  readonly pageCount = computed(() => Math.max(1, Math.ceil(this.totalCount() / this.pageSize())));
  readonly first = computed(() =>
    this.totalCount() === 0 ? 0 : (this.page() - 1) * this.pageSize() + 1,
  );
  readonly last = computed(() => Math.min(this.page() * this.pageSize(), this.totalCount()));

  move(page: number): void {
    if (this.loading() || page < 1 || page > this.pageCount()) return;
    this.pageChange.emit({ page, pageSize: this.pageSize() });
  }

  resize(event: Event): void {
    this.pageChange.emit({ page: 1, pageSize: Number((event.target as HTMLSelectElement).value) });
  }
}
