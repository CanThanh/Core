import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface PermissionDto {
  id: string;
  name: string;
  description: string;
  module: string;
}

@Injectable({
  providedIn: 'root'
})
export class PermissionsApiService {
  private readonly baseUrl = `${environment.apiUrl}/api/permissions`;

  constructor(private http: HttpClient) {}

  /**
   * Get all permissions
   */
  getPermissions(): Observable<PermissionDto[]> {
    return this.http.get<PermissionDto[]>(this.baseUrl);
  }
}
