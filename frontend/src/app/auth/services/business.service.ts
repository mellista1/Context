import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import {
  BusinessResponse,
  CreateBusinessRequest,
} from '../../models/business.models';
import { environment } from '../../../environments/enviroment';

@Injectable({
  providedIn: 'root',
})
export class BusinessService {
    private readonly apiUrl = `${environment.apiUrl}/businesses`;

    constructor(private readonly http: HttpClient) {}

    createBusiness(request: CreateBusinessRequest): Observable<BusinessResponse> {
        return this.http.post<BusinessResponse>(this.apiUrl, request);
    }

    getMyBusiness(): Observable<BusinessResponse> {
        return this.http.get<BusinessResponse>(`${this.apiUrl}/me`);
    }
}