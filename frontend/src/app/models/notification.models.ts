export type NotificationItemType = 'calendar' | 'suggestion';
export type CalendarEventType = 'mass_event' | 'service_disruption' | 'weather' | 'trend' | 'other';

export interface NewsItem {
  title: string;
  description: string;
  type: NotificationItemType;
  date: string | null;
  location: string | null;
  link: string | null;
}

export interface NotificationDetail {
  title: string;
  summary: string;
  /** Human-readable Spanish date, e.g. "Viernes a las 9" */
  date: string | null;
  location: string | null;
}

export interface CalendarEvent {
  shouldCreate: boolean;
  eventType: CalendarEventType;
  title: string;
  /** ISO8601 date string, null for non-calendar items */
  date: string | null;
}

export interface ProcessedNotification {
  notification: NotificationDetail;
  calendarEvent: CalendarEvent;
  aiSuggestion: string;
}
