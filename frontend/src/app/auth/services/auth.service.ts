import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';

import { environment } from '../../../environments/enviroment';
import {
  AuthResponse,
  LoginRequest,
  RegisterRequest,
} from '../../models/auth.models';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private readonly apiUrl = `${environment.apiUrl}/auth`;
  private readonly tokenKey = 'access_token';
  private readonly currentFullnameKey = 'full_name';

  private readonly loggedInSubject = new BehaviorSubject<boolean>(this.hasToken());
  private readonly fullNameSubject = new BehaviorSubject<string | null>(this.getStoredFullName());

  readonly loggedIn$ = this.loggedInSubject.asObservable();
  readonly fullName$ = this.fullNameSubject.asObservable();

  constructor(private http: HttpClient) { }

  login(request: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/login`, request).pipe(
      tap((response) => {
        this.saveAuthData(response);
      })
    );
  }

  register(request: RegisterRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/register`, request).pipe(
      tap((response) => {
        this.saveAuthData(response);
      })
    );
  }

  deleteMyAccount(): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/me`).pipe(
      tap(() => {
        this.logout();
      })
    );
  }

  logout(): void {
    localStorage.removeItem(this.tokenKey);
    localStorage.removeItem(this.currentFullnameKey);

    this.loggedInSubject.next(false);
    this.fullNameSubject.next(null);
  }

  getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  getFullName(): string | null {
    return this.getStoredFullName();
  }

  isLoggedIn(): boolean {
    return this.hasToken();
  }

  private saveAuthData(response: AuthResponse): void {
    localStorage.setItem(this.tokenKey, response.token);
    localStorage.setItem(this.currentFullnameKey, response.user.fullName);

    this.loggedInSubject.next(true);
    this.fullNameSubject.next(response.user.fullName);
  }

  private hasToken(): boolean {
    return localStorage.getItem(this.tokenKey) !== null;
  }

  private getStoredFullName(): string | null {
    return localStorage.getItem(this.currentFullnameKey);
  }
}