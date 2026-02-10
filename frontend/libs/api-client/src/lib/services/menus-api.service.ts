import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface MenuDto {
  id: string;
  name: string;
  icon: string | null;
  route: string | null;
  displayOrder: number;
  children: MenuDto[];
}

export interface CreateMenuRequest {
  name: string;
  icon: string | null;
  route: string | null;
  displayOrder: number;
  isActive: boolean;
  parentId: string | null;
}

export interface UpdateMenuRequest {
  name: string;
  icon: string | null;
  route: string | null;
  displayOrder: number;
  isActive: boolean;
  parentId: string | null;
}

export interface PermissionTypeAssignment {
  permissionId: string;
  permissionType: string; // "View", "Create", "Edit", "Delete"
}

export interface MenuPermissionDto {
  id: string;
  permissionId: string;
  permissionName: string;
  permissionType: string;
}

@Injectable({
  providedIn: 'root'
})
export class MenusApiService {
  private readonly baseUrl = `${environment.apiUrl}/api/menus`;

  constructor(private http: HttpClient) {}

  /**
   * Get all menus (admin)
   */
  getMenus(): Observable<MenuDto[]> {
    return this.http.get<MenuDto[]>(this.baseUrl);
  }

  /**
   * Get current user's accessible menus
   */
  getUserMenus(): Observable<MenuDto[]> {
    return this.http.get<MenuDto[]>(`${this.baseUrl}/user`);
  }

  /**
   * Create new menu
   */
  createMenu(request: CreateMenuRequest): Observable<any> {
    return this.http.post(this.baseUrl, request);
  }

  /**
   * Update menu
   */
  updateMenu(id: string, request: UpdateMenuRequest): Observable<any> {
    return this.http.put(`${this.baseUrl}/${id}`, request);
  }

  /**
   * Delete menu
   */
  deleteMenu(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  /**
   * Get permissions assigned to a menu
   */
  getMenuPermissions(menuId: string): Observable<MenuPermissionDto[]> {
    return this.http.get<MenuPermissionDto[]>(`${this.baseUrl}/${menuId}/permissions`);
  }

  /**
   * Assign permissions to a menu
   */
  assignPermissionsToMenu(menuId: string, assignments: PermissionTypeAssignment[]): Observable<boolean> {
    return this.http.post<boolean>(`${this.baseUrl}/${menuId}/permissions`, assignments);
  }
}
