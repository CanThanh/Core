import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { PagedResult } from '../models';

@Injectable({
  providedIn: 'root'
})
export class AssetsApiService {
  private readonly baseUrl = `${environment.apiUrl}/api/assets`;

  constructor(private http: HttpClient) {}

  getAssets(
    pageNumber: number = 1,
    pageSize: number = 10,
    searchTerm?: string,
    status?: string,
    categoryId?: string
  ): Observable<PagedResult<any>> {
    let params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());

    if (searchTerm) params = params.set('searchTerm', searchTerm);
    if (status) params = params.set('status', status);
    if (categoryId) params = params.set('categoryId', categoryId);

    return this.http.get<PagedResult<any>>(this.baseUrl, { params });
  }

  getAssetById(id: string): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/${id}`);
  }

  createAsset(request: any): Observable<any> {
    return this.http.post<any>(this.baseUrl, request);
  }

  updateAsset(id: string, request: any): Observable<any> {
    return this.http.put<any>(`${this.baseUrl}/${id}`, request);
  }

  deleteAsset(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
