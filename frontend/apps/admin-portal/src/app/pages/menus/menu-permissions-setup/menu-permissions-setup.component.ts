import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CardModule } from 'primeng/card';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { CheckboxModule } from 'primeng/checkbox';
import { DropdownModule } from 'primeng/dropdown';
import { ToastModule } from 'primeng/toast';
import { PaginatorModule } from 'primeng/paginator';
import { MessageService } from 'primeng/api';
import {
  MenusApiService,
  PermissionsApiService,
  PermissionDto,
  MenuPermissionDto,
  PermissionTypeAssignment
} from '@qlts/api-client';

interface PermissionRow {
  permissionId: string;
  permissionName: string;
  permissionModule: string;
  view: boolean;
  create: boolean;
  edit: boolean;
  delete: boolean;
}

@Component({
  selector: 'app-menu-permissions-setup',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    CardModule,
    TableModule,
    ButtonModule,
    CheckboxModule,
    DropdownModule,
    ToastModule,
    PaginatorModule
  ],
  providers: [MessageService],
  templateUrl: './menu-permissions-setup.component.html',
  styleUrl: './menu-permissions-setup.component.css'
})
export class MenuPermissionsSetupComponent implements OnInit {
  menuId: string | null = null;
  menuName = '';
  permissions: PermissionRow[] = [];
  loading = false;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private menusApiService: MenusApiService,
    private permissionsApiService: PermissionsApiService,
    private messageService: MessageService
  ) {}

  ngOnInit(): void {
    this.menuId = this.route.snapshot.paramMap.get('menuId');

    if (!this.menuId) {
      this.messageService.add({
        severity: 'error',
        summary: 'Error',
        detail: 'Menu ID not found'
      });
      this.router.navigate(['/menus']);
      return;
    }

    this.loadData();
  }

  loadData(): void {
    if (!this.menuId) return;

    this.loading = true;

    // Load all permissions
    this.permissionsApiService.getPermissions().subscribe({
      next: (allPermissions) => {
        // Load current menu permissions
        this.menusApiService.getMenuPermissions(this.menuId!).subscribe({
          next: (menuPermissions) => {
            this.permissions = this.buildPermissionRows(allPermissions, menuPermissions);
            this.loading = false;
          },
          error: (error) => {
            this.messageService.add({
              severity: 'error',
              summary: 'Error',
              detail: 'Failed to load menu permissions'
            });
            console.error('Error loading menu permissions:', error);
            this.loading = false;
          }
        });
      },
      error: (error) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Failed to load permissions'
        });
        console.error('Error loading permissions:', error);
        this.loading = false;
      }
    });
  }

  buildPermissionRows(
    allPermissions: PermissionDto[],
    menuPermissions: MenuPermissionDto[]
  ): PermissionRow[] {
    // Create a map of permissionId -> permission types
    const permissionsMap = new Map<string, Set<string>>();
    menuPermissions.forEach(mp => {
      if (!permissionsMap.has(mp.permissionId)) {
        permissionsMap.set(mp.permissionId, new Set());
      }
      permissionsMap.get(mp.permissionId)!.add(mp.permissionType.toLowerCase());
    });

    // Build rows
    return allPermissions.map(p => {
      const types = permissionsMap.get(p.id) || new Set();
      return {
        permissionId: p.id,
        permissionName: p.name,
        permissionModule: p.module,
        view: types.has('view'),
        create: types.has('create'),
        edit: types.has('edit'),
        delete: types.has('delete')
      };
    });
  }

  savePermissions(): void {
    if (!this.menuId) return;

    this.loading = true;

    // Collect all assignments
    const assignments: PermissionTypeAssignment[] = [];
    this.permissions.forEach(row => {
      if (row.view) {
        assignments.push({ permissionId: row.permissionId, permissionType: 'View' });
      }
      if (row.create) {
        assignments.push({ permissionId: row.permissionId, permissionType: 'Create' });
      }
      if (row.edit) {
        assignments.push({ permissionId: row.permissionId, permissionType: 'Edit' });
      }
      if (row.delete) {
        assignments.push({ permissionId: row.permissionId, permissionType: 'Delete' });
      }
    });

    this.menusApiService.assignPermissionsToMenu(this.menuId, assignments).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: 'Success',
          detail: 'Menu permissions saved successfully'
        });
        this.loading = false;
      },
      error: (error) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Failed to save permissions'
        });
        console.error('Error saving permissions:', error);
        this.loading = false;
      }
    });
  }

  cancel(): void {
    this.router.navigate(['/menus']);
  }
}
