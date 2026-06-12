import {
  Component,
  OnInit,
  OnDestroy,
  computed,
  signal,
} from '@angular/core';
import { Router, NavigationEnd } from '@angular/router';
import { Subscription, filter, take } from 'rxjs';

import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { AuthService } from '../../auth/services/auth.service';
import { NotificationService } from '../../services/notification.service';
import { ProcessedNotification } from '../../models/notification.models';

const DISMISSED_KEY = 'assistant_dismissed_date';
const VER_MAS_TARDE_DELAY_MS = 30 * 60 * 1000;

@Component({
  selector: 'app-assistant-bubble',
  standalone: true,
  imports: [MatButtonModule, MatIconModule, MatProgressSpinnerModule],
  templateUrl: './assistant-bubble.html',
  styleUrl: './assistant-bubble.css',
})
export class AssistantBubbleComponent implements OnInit, OnDestroy {
  readonly isVisible = signal(false);
  readonly isExpanded = signal(false);
  readonly isLoading = signal(false);

  readonly notifications = signal<ProcessedNotification[]>([]);

  readonly suggestions = computed(() =>
    this.notifications().filter((n) => !n.calendarEvent.shouldCreate)
  );

  readonly hasContent = computed(() => this.suggestions().length > 0);

  private authSub?: Subscription;
  private verMasTardeTimerId?: ReturnType<typeof setTimeout>;
  private navSub?: Subscription;

  constructor(
    private readonly authService: AuthService,
    private readonly notificationService: NotificationService,
    private readonly router: Router
  ) {}

  ngOnInit(): void {
    this.authSub = this.authService.loggedIn$.subscribe((loggedIn) => {
      if (loggedIn) {
        this.isVisible.set(true);
        this.loadNotifications();
        this.maybeAutoExpand();
      } else {
        this.isVisible.set(false);
        this.isExpanded.set(false);
      }
    });
  }

  ngOnDestroy(): void {
    this.authSub?.unsubscribe();
    this.navSub?.unsubscribe();
    if (this.verMasTardeTimerId !== undefined) {
      clearTimeout(this.verMasTardeTimerId);
    }
  }

  toggleExpanded(): void {
    this.isExpanded.update((v) => !v);
  }

  omitir(): void {
    const today = new Date().toISOString().split('T')[0];
    localStorage.setItem(DISMISSED_KEY, today);
    this.isExpanded.set(false);
  }

  verMasTarde(): void {
    this.isExpanded.set(false);
    this.navSub?.unsubscribe();

    const reopen = () => {
      if (!this.isDismissedToday()) this.isExpanded.set(true);
    };

    this.verMasTardeTimerId = setTimeout(reopen, VER_MAS_TARDE_DELAY_MS);

    this.navSub = this.router.events
      .pipe(filter((e) => e instanceof NavigationEnd), take(1))
      .subscribe(() => {
        clearTimeout(this.verMasTardeTimerId);
        this.verMasTardeTimerId = undefined;
        reopen();
      });
  }

  private maybeAutoExpand(): void {
    if (this.isDismissedToday()) return;
    this.isExpanded.set(true);
  }

  private isDismissedToday(): boolean {
    const dismissedDate = localStorage.getItem(DISMISSED_KEY);
    if (!dismissedDate) return false;
    return dismissedDate === new Date().toISOString().split('T')[0];
  }

  private loadNotifications(): void {
    this.isLoading.set(true);
    this.notificationService.getNotifications().subscribe({
      next: (data) => {
        this.notifications.set(data);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false),
    });
  }
}
