import { TestBed } from '@angular/core/testing';
import { Pagination } from './pagination';

describe('Pagination', () => {
  it('emits next-page requests and resets to page one when page size changes', () => {
    const fixture = TestBed.createComponent(Pagination);
    fixture.componentRef.setInput('totalCount', 45);
    fixture.detectChanges();
    const events: unknown[] = [];
    fixture.componentInstance.pageChange.subscribe((event) => events.push(event));
    fixture.nativeElement.querySelector('[aria-label="Next page"]').click();
    expect(events[0]).toEqual({ page: 2, pageSize: 20 });
    fixture.componentRef.setInput('page', 2);
    fixture.detectChanges();
    const select = fixture.nativeElement.querySelector('select') as HTMLSelectElement;
    select.value = '50';
    select.dispatchEvent(new Event('change'));
    expect(events[1]).toEqual({ page: 1, pageSize: 50 });
  });

  it('disables invalid navigation for empty results, loading and the last page', () => {
    const fixture = TestBed.createComponent(Pagination);
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('0-0 of 0');
    expect(fixture.nativeElement.querySelector('[aria-label="Next page"]').disabled).toBe(true);
    fixture.componentRef.setInput('totalCount', 21);
    fixture.componentRef.setInput('page', 2);
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('21-21 of 21');
    expect(fixture.nativeElement.querySelector('[aria-label="Next page"]').disabled).toBe(true);
    fixture.componentRef.setInput('loading', true);
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('[aria-label="Previous page"]').disabled).toBe(true);
  });
});
