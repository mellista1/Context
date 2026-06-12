import { CalendarEventType } from './notification.models';

export interface SalesOutcome {
  topProduct: string;
  topProductQuantity: number;
  salesIncreasePercent: number | null;
}

export interface CalendarEventResponse {
  id: number;
  eventType: CalendarEventType;
  title: string;
  description: string;
  location: string | null;
  eventDate: string; // ISO8601
  /** null when the event is upcoming or no orders data exists for that date yet */
  outcome: SalesOutcome | null;
}

export interface CreateCalendarEventRequest {
  eventType: string;
  title: string;
  description: string;
  location: string | null;
  eventDate: string; // ISO8601
}
