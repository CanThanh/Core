import { Injectable, signal, effect, computed } from '@angular/core';
import { MenusApiService, MenuDto } from '@qlts/api-client';
import { AuthService } from './auth.service';

@Injectable({
  providedIn: 'root'
})
export class MenuService {
  // Private writable signals
  private userMenus = signal<MenuDto[]>([]);
  private loading = signal<boolean>(false);

  // Public readonly signals
  public readonly menus = this.userMenus.asReadonly();
  public readonly isLoading = this.loading.asReadonly();

  // Computed signal - flat menu list for route checking
  public readonly flatMenus = computed(() => {
    return this.flattenMenus(this.userMenus());
  });

  constructor(
    private menusApiService: MenusApiService,
    private authService: AuthService
  ) {
    // Automatically load menus when user logs in
    effect(() => {
      if (this.authService.isAuthenticated()) {
        this.loadUserMenus();
      } else {
        this.userMenus.set([]);
      }
    }, { allowSignalWrites: true });
  }

  /**
   * Load user's accessible menus from API
   */
  loadUserMenus(): void {
    this.loading.set(true);
    this.menusApiService.getUserMenus().subscribe({
      next: (menus) => {
        this.userMenus.set(menus);
        this.loading.set(false);
      },
      error: (error) => {
        console.error('Failed to load user menus:', error);
        this.userMenus.set([]);
        this.loading.set(false);
      }
    });
  }

  /**
   * Check if user has access to a specific route
   */
  hasMenuAccess(routePath: string): boolean {
    const normalizedPath = this.normalizePath(routePath);
    return this.flatMenus().some(menu =>
      menu.route && this.normalizePath(menu.route) === normalizedPath
    );
  }

  /**
   * Find menu by route path
   */
  findMenuByRoute(routePath: string): MenuDto | null {
    const normalizedPath = this.normalizePath(routePath);
    return this.flatMenus().find(menu =>
      menu.route && this.normalizePath(menu.route) === normalizedPath
    ) || null;
  }

  /**
   * Flatten hierarchical menu structure for easier searching
   */
  private flattenMenus(menus: MenuDto[]): MenuDto[] {
    const result: MenuDto[] = [];

    const flatten = (items: MenuDto[]) => {
      for (const item of items) {
        result.push(item);
        if (item.children && item.children.length > 0) {
          flatten(item.children);
        }
      }
    };

    flatten(menus);
    return result;
  }

  /**
   * Normalize route path for comparison
   */
  private normalizePath(path: string): string {
    // Remove leading/trailing slashes and normalize
    return path.replace(/^\/+|\/+$/g, '').toLowerCase();
  }
}
