import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CardModule } from 'primeng/card';
import { DropdownModule } from 'primeng/dropdown';
import { TreeTableModule } from 'primeng/treetable';
import { ButtonModule } from 'primeng/button';
import { CheckboxModule } from 'primeng/checkbox';
import { MessageService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';
import { TreeNode } from 'primeng/api';
import { MenusApiService, MenuDto, RolesApiService, MenuPermissionAssignment, RoleMenuPermissionDto } from '@qlts/api-client';

interface PermissionState {
  view: boolean;
  create: boolean;
  edit: boolean;
  delete: boolean;
}

interface MenuPermissionNode extends TreeNode {
  data: {
    menuId: string;
    name: string;
    permissions: PermissionState;
  };
}

@Component({
  selector: 'app-role-menu-permissions',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    CardModule,
    DropdownModule,
    TreeTableModule,
    ButtonModule,
    CheckboxModule,
    ToastModule
  ],
  providers: [MessageService],
  templateUrl: './role-menu-permissions.component.html',
  styleUrl: './role-menu-permissions.component.css'
})
export class RoleMenuPermissionsComponent implements OnInit {
  roles: any[] = [];
  selectedRoleId: string | null = null;
  menuNodes: MenuPermissionNode[] = [];
  loading = false;

  constructor(
    private rolesApiService: RolesApiService,
    private menusApiService: MenusApiService,
    private messageService: MessageService
  ) {}

  ngOnInit(): void {
    this.loadRoles();
  }

  loadRoles(): void {
    this.rolesApiService.getRoles().subscribe({
      next: (roles) => {
        this.roles = roles.map(role => ({
          label: role.name,
          value: role.id
        }));
      },
      error: (error) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Failed to load roles'
        });
        console.error('Error loading roles:', error);
      }
    });
  }

  onRoleChange(): void {
    if (!this.selectedRoleId) {
      this.menuNodes = [];
      return;
    }

    this.loading = true;

    // Load all menus
    this.menusApiService.getMenus().subscribe({
      next: (menus) => {
        // Load role's current menu permissions
        this.rolesApiService.getRoleMenuPermissions(this.selectedRoleId!).subscribe({
          next: (rolePermissions) => {
            this.menuNodes = this.convertToMenuPermissionNodes(menus, rolePermissions);
            this.loading = false;
          },
          error: (error) => {
            this.messageService.add({
              severity: 'error',
              summary: 'Error',
              detail: 'Failed to load role permissions'
            });
            console.error('Error loading role permissions:', error);
            this.loading = false;
          }
        });
      },
      error: (error) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Failed to load menus'
        });
        console.error('Error loading menus:', error);
        this.loading = false;
      }
    });
  }

  convertToMenuPermissionNodes(
    menus: MenuDto[],
    rolePermissions: RoleMenuPermissionDto[]
  ): MenuPermissionNode[] {
    // Create a map of menuId -> permission types
    const permissionsMap = new Map<string, Set<string>>();
    rolePermissions.forEach(rp => {
      if (!permissionsMap.has(rp.menuId)) {
        permissionsMap.set(rp.menuId, new Set());
      }
      permissionsMap.get(rp.menuId)!.add(rp.permissionType.toLowerCase());
    });

    // Convert menus to tree nodes with permission state
    const convertMenu = (menu: MenuDto): MenuPermissionNode => {
      const menuPermissions = permissionsMap.get(menu.id) || new Set();

      const node: MenuPermissionNode = {
        data: {
          menuId: menu.id,
          name: menu.name,
          permissions: {
            view: menuPermissions.has('view'),
            create: menuPermissions.has('create'),
            edit: menuPermissions.has('edit'),
            delete: menuPermissions.has('delete')
          }
        },
        children: menu.children?.map(child => convertMenu(child))
      };

      return node;
    };

    return menus.map(menu => convertMenu(menu));
  }

  savePermissions(): void {
    if (!this.selectedRoleId) {
      return;
    }

    this.loading = true;

    // Flatten all nodes and collect assignments
    const assignments: MenuPermissionAssignment[] = [];
    const collectAssignments = (nodes: MenuPermissionNode[]) => {
      nodes.forEach(node => {
        const perms = node.data.permissions;
        const menuId = node.data.menuId;

        if (perms.view) {
          assignments.push({ menuId, permissionType: 'View' });
        }
        if (perms.create) {
          assignments.push({ menuId, permissionType: 'Create' });
        }
        if (perms.edit) {
          assignments.push({ menuId, permissionType: 'Edit' });
        }
        if (perms.delete) {
          assignments.push({ menuId, permissionType: 'Delete' });
        }

        if (node.children) {
          collectAssignments(node.children as MenuPermissionNode[]);
        }
      });
    };

    collectAssignments(this.menuNodes);

    this.rolesApiService.assignMenuPermissionsToRole(this.selectedRoleId, assignments).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: 'Success',
          detail: 'Menu permissions assigned successfully'
        });
        this.loading = false;
      },
      error: (error) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Failed to assign permissions'
        });
        console.error('Error assigning permissions:', error);
        this.loading = false;
      }
    });
  }
}
