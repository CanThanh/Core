import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface MenuPermissionAssignment {
  menuId: string;
  permissionType: string;
}

export interface RoleMenuPermissionDto {
  menuId: string;
  menuName: string;
  permissionType: string;
  permissionId: string;
}

@Injectable({
  providedIn: 'root'
})
export class RolesApiService {
  private readonly baseUrl = `${environment.apiUrl}/api/roles`;

  constructor(private http: HttpClient) {}

  getRoles(): Observable<any[]> {
    return this.http.get<any[]>(this.baseUrl);
  }

  getRoleById(id: string): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/${id}`);
  }

  createRole(request: any): Observable<any> {
    return this.http.post<any>(this.baseUrl, request);
  }

  updateRole(id: string, request: any): Observable<any> {
    return this.http.put<any>(`${this.baseUrl}/${id}`, request);
  }

  deleteRole(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  assignMenuPermissionsToRole(roleId: string, assignments: MenuPermissionAssignment[]): Observable<boolean> {
    return this.http.post<boolean>(`${this.baseUrl}/${roleId}/menu-permissions`, assignments);
  }

  getRoleMenuPermissions(roleId: string): Observable<RoleMenuPermissionDto[]> {
    return this.http.get<RoleMenuPermissionDto[]>(`${this.baseUrl}/${roleId}/menu-permissions`);
  }
}
