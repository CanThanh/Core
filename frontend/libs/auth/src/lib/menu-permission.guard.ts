import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, Router, UrlTree } from '@angular/router';
import { MenuService } from './menu.service';
import { TokenService } from './token.service';

@Injectable({
  providedIn: 'root'
})
export class MenuPermissionGuard {
  constructor(
    private menuService: MenuService,
    private router: Router,
    private tokenService: TokenService
  ) {}

  canActivate(route: ActivatedRouteSnapshot): boolean | UrlTree {
    // First check authentication
    if (!this.tokenService.isAuthenticated()) {
      return this.router.createUrlTree(['/login']);
    }

    // Get the route path
    const routePath = '/' + (route.routeConfig?.path || '');

    // Check if user has menu access
    if (this.menuService.hasMenuAccess(routePath)) {
      return true;
    }

    // No permission - redirect to dashboard
    console.warn(`Access denied to route: ${routePath}`);
    return this.router.createUrlTree(['/dashboard']);
  }
}
