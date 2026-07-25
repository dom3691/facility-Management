import { computed, inject, Injectable, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { BreakpointObserver } from '@angular/cdk/layout';
import { map } from 'rxjs/operators';

/**
 * Cross-component UI state for the application shell: responsive breakpoint,
 * desktop "rail" collapse, and the mobile drawer. Kept here (not in a component)
 * so the top navigation's toggle and the sidebar stay in sync.
 */
@Injectable({ providedIn: 'root' })
export class LayoutService {
  private readonly breakpoints = inject(BreakpointObserver);

  /** Phones / small tablets → the sidebar becomes an over-mode drawer. */
  readonly isMobile = toSignal(
    this.breakpoints.observe('(max-width: 768px)').pipe(map((r) => r.matches)),
    { initialValue: false },
  );

  /** User-controlled collapse of the desktop sidebar to an icon rail. */
  readonly collapsed = signal(false);

  /** Mobile drawer open/closed. */
  readonly mobileOpen = signal(false);

  /** True only when the desktop sidebar is showing as a narrow icon rail. */
  readonly rail = computed(() => !this.isMobile() && this.collapsed());

  /** Menu button: collapses the rail on desktop, opens the drawer on mobile. */
  toggleSidebar(): void {
    if (this.isMobile()) {
      this.mobileOpen.update((open) => !open);
    } else {
      this.collapsed.update((collapsed) => !collapsed);
    }
  }

  setMobileOpen(open: boolean): void {
    this.mobileOpen.set(open);
  }

  /** Called after navigating so the mobile drawer auto-closes. */
  closeOnNavigate(): void {
    if (this.isMobile()) {
      this.mobileOpen.set(false);
    }
  }
}
