import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { environment } from '../../environments/enviroment';
import { ProcessedNotification } from '../models/notification.models';

@Injectable({
  providedIn: 'root',
})
export class NotificationService {
  private readonly apiUrl = `${environment.apiUrl}/notifications`;

  constructor(private readonly http: HttpClient) {}

  getNotifications(): Observable<ProcessedNotification[]> {
    return this.http.get<ProcessedNotification[]>(this.apiUrl);
  }

  updateNotifications(): Observable<ProcessedNotification[]> {
    return this.http.post<ProcessedNotification[]>(`${this.apiUrl}/update-notifications`, null);
  }
}
