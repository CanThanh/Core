import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { PagedResult } from '../models';

@Injectable({
  providedIn: 'root'
})
export class GroupsApiService {
  private readonly baseUrl = `${environment.apiUrl}/api/groups`;

  constructor(private http: HttpClient) {}

  getGroups(
    pageNumber: number = 1,
    pageSize: number = 10,
    searchTerm?: string,
    isActive?: boolean
  ): Observable<PagedResult<any>> {
    let params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());

    if (searchTerm) params = params.set('searchTerm', searchTerm);
    if (isActive !== undefined) params = params.set('isActive', isActive.toString());

    return this.http.get<PagedResult<any>>(this.baseUrl, { params });
  }

  getAllGroups(): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/all`);
  }

  createGroup(request: any): Observable<any> {
    return this.http.post<any>(this.baseUrl, request);
  }

  updateGroup(id: string, request: any): Observable<any> {
    return this.http.put<any>(`${this.baseUrl}/${id}`, request);
  }

  deleteGroup(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
