import { Component, OnDestroy, OnInit, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { finalize, Subscription } from 'rxjs';

import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';

import { AuthService } from '../../auth/services/auth.service';
import { BusinessService } from '../../auth/services/business.service';
import { NotificationService } from '../../services/notification.service';

import { ProcessedNotification, CalendarEventType } from '../../models/notification.models';
import { DeleteAccountDialogComponent } from '../delete-account-dialog/delete-account-dialog';

// ── Calendar helpers ──────────────────────────────────────────────────────────

export interface CalendarDay {
  date: number | null;
  isoDate: string | null;
}

const MONTH_NAMES = [
  'Enero', 'Febrero', 'Marzo', 'Abril', 'Mayo', 'Junio',
  'Julio', 'Agosto', 'Septiembre', 'Octubre', 'Noviembre', 'Diciembre',
];

function buildMonthGrid(year: number, month: number): CalendarDay[] {
  const firstDay = new Date(year, month - 1, 1);
  const lastDay = new Date(year, month, 0);
  const startOffset = (firstDay.getDay() + 6) % 7;
  const days: CalendarDay[] = [];

  for (let i = 0; i < startOffset; i++) days.push({ date: null, isoDate: null });
  for (let d = 1; d <= lastDay.getDate(); d++) {
    const isoDate = `${year}-${String(month).padStart(2, '0')}-${String(d).padStart(2, '0')}`;
    days.push({ date: d, isoDate });
  }
  while (days.length % 7 !== 0) days.push({ date: null, isoDate: null });
  return days;
}

// ── Event display type ────────────────────────────────────────────────────────

export interface EventOutcome {
  topProduct: string;
  topProductQuantity: number;
  salesIncreasePercent: number | null;
}

export interface EventDisplay {
  isoDate: string;
  title: string;
  location: string | null;
  eventDate: string | null;
  humanDate: string | null;
  eventType: CalendarEventType;
  aiSuggestion: string | null;
  isPending: boolean;
  outcome: EventOutcome | null;
}

function buildEventDisplay(isoDate: string, n: ProcessedNotification): EventDisplay {
  return {
    isoDate,
    title: n.notification.title,
    location: n.notification.location ?? null,
    eventDate: n.calendarEvent.date ?? null,
    humanDate: n.notification.date ?? null,
    eventType: (n.calendarEvent.eventType ?? 'other') as CalendarEventType,
    aiSuggestion: n.aiSuggestion ?? null,
    isPending: true,
    outcome: null,
  };
}

// ─────────────────────────────────────────────────────────────────────────────

@Component({
  selector: 'app-dashboard',
  imports: [
    CommonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatDialogModule,
  ],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css',
})
export class DashboardComponent implements OnInit, OnDestroy {

  readonly businessName = signal<string | null>(null);

  // ── Manager modal ─────────────────────────────────────────────────────────
  readonly isManagerModalOpen = signal(false);

  // ── Calendar state ────────────────────────────────────────────────────────
  readonly displayMonth = signal<{ year: number; month: number }>({
    year: new Date().getFullYear(),
    month: new Date().getMonth() + 1,
  });

  readonly selectedDate = signal<string | null>(null);
  readonly notifications = signal<ProcessedNotification[]>([]);

  readonly calendarDays = computed(() => {
    const { year, month } = this.displayMonth();
    return buildMonthGrid(year, month);
  });

  readonly eventsByDate = computed(() => {
    const map = new Map<string, ProcessedNotification>();
    for (const n of this.notifications()) {
      if (!n.calendarEvent.shouldCreate || !n.calendarEvent.date) continue;
      const key = n.calendarEvent.date.split('T')[0];
      map.set(key, n);
    }
    return map;
  });

  readonly suggestions = computed(() =>
    this.notifications().filter((n) => !n.calendarEvent.shouldCreate)
  );

  readonly displayMonthLabel = computed(() => {
    const { year, month } = this.displayMonth();
    return `${MONTH_NAMES[month - 1]} ${year}`;
  });

  readonly selectedEventDisplay = computed<EventDisplay | null>(() => {
    const date = this.selectedDate();
    if (!date) return null;
    const data = this.eventsByDate().get(date);
    if (!data) return null;
    return buildEventDisplay(date, data);
  });

  readonly allEventDisplays = computed<EventDisplay[]>(() => {
    const result: EventDisplay[] = [];
    for (const [isoDate, data] of this.eventsByDate()) {
      result.push(buildEventDisplay(isoDate, data));
    }
    return result.sort((a, b) => (a.isoDate > b.isoDate ? 1 : -1));
  });

  isDeletingAccount = false;
  private authSub?: Subscription;

  constructor(
    private readonly authService: AuthService,
    private readonly businessService: BusinessService,
    private readonly notificationService: NotificationService,
    private readonly router: Router,
    private readonly dialog: MatDialog,
  ) {}

  ngOnInit(): void {
    this.businessService.getMyBusiness().subscribe({
      next: business => this.businessName.set(business.name),
      error: err => console.error('Error loading business', err),
    });
    this.loadNotificationsData();
  }

  ngOnDestroy(): void {
    this.authSub?.unsubscribe();
  }

  // ── Navigation ────────────────────────────────────────────────────────────

  navigateToOrdenar(): void {
    this.router.navigate(['/ordenar']);
  }

  navigateToRegistro(): void {
    this.router.navigate(['/registro']);
  }

  navigateToAnalisis(): void {
    this.router.navigate(['/analisis']);
  }

  // ── Calendar ──────────────────────────────────────────────────────────────

  prevMonth(): void {
    this.displayMonth.update(({ year, month }) => ({
      year: month === 1 ? year - 1 : year,
      month: month === 1 ? 12 : month - 1,
    }));
    this.selectedDate.set(null);
  }

  nextMonth(): void {
    this.displayMonth.update(({ year, month }) => ({
      year: month === 12 ? year + 1 : year,
      month: month === 12 ? 1 : month + 1,
    }));
    this.selectedDate.set(null);
  }

  selectDay(isoDate: string): void {
    this.selectedDate.set(this.selectedDate() === isoDate ? null : isoDate);
  }

  isToday(isoDate: string | null): boolean {
    if (!isoDate) return false;
    return isoDate === new Date().toISOString().split('T')[0];
  }

  hasEvent(isoDate: string | null): boolean {
    return isoDate != null && this.eventsByDate().has(isoDate);
  }

  eventTypeLabel(type: CalendarEventType): string {
    const labels: Record<CalendarEventType, string> = {
      mass_event: 'Evento masivo',
      service_disruption: 'Corte de servicio',
      weather: 'Clima',
      trend: 'Tendencia',
      other: 'Noticia',
    };
    return labels[type] ?? 'Noticia';
  }

  // ── Manager modal ─────────────────────────────────────────────────────────

  openManagerModal(): void {
    this.isManagerModalOpen.set(true);
  }

  closeManagerModal(): void {
    this.isManagerModalOpen.set(false);
  }

  // ── Account ───────────────────────────────────────────────────────────────

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  openDeleteAccountDialog(): void {
    const dialogRef = this.dialog.open(DeleteAccountDialogComponent, {
      width: '420px',
      maxWidth: 'calc(100vw - 32px)',
      disableClose: true,
    });

    dialogRef.afterClosed().subscribe((confirmed: boolean) => {
      if (!confirmed) return;
      this.deleteAccount();
    });
  }

  // ── Private helpers ───────────────────────────────────────────────────────

  private deleteAccount(): void {
    this.isDeletingAccount = true;

    this.authService
      .deleteMyAccount()
      .pipe(finalize(() => (this.isDeletingAccount = false)))
      .subscribe({
        next: () => {
          this.authService.logout();
          this.router.navigate(['/login']);
        },
        error: (err) => console.error('Error deleting account', err),
      });
  }

  private loadNotificationsData(): void {
    this.notificationService.getNotifications().subscribe({
      next: (data) => {
        if (data.length === 0) {
          this.refreshNotifications();
          return;
        }
        this.notifications.set(data);
        this.autoSelectNearestEvent();
      },
      error: (err) => console.error('Error loading notifications', err),
    });
  }

  refreshNotifications(): void {
    this.notificationService.updateNotifications().subscribe({
      next: (data) => {
        this.notifications.set(data);
        this.autoSelectNearestEvent();
      },
      error: (err) => console.error('Error updating notifications', err),
    });
  }

  private autoSelectNearestEvent(): void {
    if (this.selectedDate()) return;
    const today = new Date().toISOString().split('T')[0];
    const dates = [...this.eventsByDate().keys()].sort();
    const nearest = dates.find((d) => d >= today) ?? dates[dates.length - 1];
    if (nearest) this.selectedDate.set(nearest);
  }
}
