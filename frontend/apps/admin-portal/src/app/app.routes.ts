import { Routes } from '@angular/router';
import { AuthGuard, MenuPermissionGuard } from '@asset-management/auth';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'dashboard',
    pathMatch: 'full'
  },
  // Public routes (no layout)
  {
    path: 'login',
    loadComponent: () => import('./pages/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: 'register',
    loadComponent: () => import('./pages/register/register.component').then(m => m.RegisterComponent)
  },
  {
    path: 'forgot-password',
    loadComponent: () => import('./pages/forgot-password/forgot-password.component').then(m => m.ForgotPasswordComponent)
  },
  {
    path: 'reset-password',
    loadComponent: () => import('./pages/reset-password/reset-password.component').then(m => m.ResetPasswordComponent)
  },
  {
    path: 'no-access',
    loadComponent: () => import('./pages/no-access/no-access.component').then(m => m.NoAccessComponent)
  },
  // Protected routes (with layout)
  {
    path: '',
    loadComponent: () => import('./layouts/main-layout/main-layout.component').then(m => m.MainLayoutComponent),
    canActivate: [AuthGuard],
    children: [
      {
        path: 'dashboard',
        loadComponent: () => import('./pages/dashboard/dashboard.component').then(m => m.DashboardComponent)
      },
      {
        path: 'assets',
        loadComponent: () => import('./pages/assets/assets-list/assets-list.component').then(m => m.AssetsListComponent),
        canActivate: [MenuPermissionGuard]
      },
      {
        path: 'users',
        loadComponent: () => import('./pages/users/users-list/users-list.component').then(m => m.UsersListComponent),
        canActivate: [MenuPermissionGuard]
      },
      {
        path: 'groups',
        loadComponent: () => import('./pages/groups/groups-list/groups-list.component').then(m => m.GroupsListComponent),
        canActivate: [MenuPermissionGuard]
      },
      {
        path: 'menus',
        loadComponent: () => import('./pages/menus/menus-list/menus-list.component').then(m => m.MenusListComponent)
      },
      {
        path: 'menus/:menuId/permissions',
        loadComponent: () => import('./pages/menus/menu-permissions-setup/menu-permissions-setup.component').then(m => m.MenuPermissionsSetupComponent)
      },
      {
        path: 'roles/menu-permissions',
        loadComponent: () => import('./pages/roles/role-menu-permissions/role-menu-permissions.component').then(m => m.RoleMenuPermissionsComponent)
      }
    ]
  },
  {
    path: '**',
    redirectTo: 'dashboard'
  }
];
