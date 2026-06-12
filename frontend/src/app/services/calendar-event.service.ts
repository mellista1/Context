import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { environment } from '../../environments/enviroment';
import {
  CalendarEventResponse,
  CreateCalendarEventRequest,
} from '../models/calendar-event.models';

@Injectable({
  providedIn: 'root',
})
export class CalendarEventService {
  private readonly apiUrl = `${environment.apiUrl}/calendar-events`;

  constructor(private readonly http: HttpClient) {}

  createEvent(dto: CreateCalendarEventRequest): Observable<CalendarEventResponse> {
    return this.http.post<CalendarEventResponse>(this.apiUrl, dto);
  }

  getEventsForMonth(year: number, month: number): Observable<CalendarEventResponse[]> {
    return this.http.get<CalendarEventResponse[]>(
      `${this.apiUrl}?year=${year}&month=${month}`
    );
  }
}
