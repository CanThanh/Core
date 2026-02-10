import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { PagedResult } from '../models';

@Injectable({
  providedIn: 'root'
})
export class UsersApiService {
  private readonly baseUrl = `${environment.apiUrl}/api/users`;

  constructor(private http: HttpClient) {}

  getUsers(
    pageNumber: number = 1,
    pageSize: number = 10,
    searchTerm?: string,
    isActive?: boolean,
    groupId?: string
  ): Observable<PagedResult<any>> {
    let params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());

    if (searchTerm) params = params.set('searchTerm', searchTerm);
    if (isActive !== undefined) params = params.set('isActive', isActive.toString());
    if (groupId) params = params.set('groupId', groupId);

    return this.http.get<PagedResult<any>>(this.baseUrl, { params });
  }

  getUserById(id: string): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/${id}`);
  }

  createUser(request: any): Observable<any> {
    return this.http.post<any>(this.baseUrl, request);
  }

  updateUser(id: string, request: any): Observable<any> {
    return this.http.put<any>(`${this.baseUrl}/${id}`, request);
  }

  deleteUser(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  assignRolesToUser(userId: string, roleIds: string[]): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${userId}/roles`, { roleIds });
  }

  addUserToGroup(userId: string, groupId: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${userId}/groups/${groupId}`, {});
  }

  removeUserFromGroup(userId: string, groupId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${userId}/groups/${groupId}`);
  }
}
