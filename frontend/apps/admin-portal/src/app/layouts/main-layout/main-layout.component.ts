import { Component, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet, Router } from '@angular/router';
import { PanelMenuModule } from 'primeng/panelmenu';
import { ButtonModule } from 'primeng/button';
import { AvatarModule } from 'primeng/avatar';
import { MenuModule } from 'primeng/menu';
import { TooltipModule } from 'primeng/tooltip';
import { BadgeModule } from 'primeng/badge';
import { AuthService, MenuService } from '@asset-management/auth';
import { MenuItem } from 'primeng/api';
import { MenuDto } from '@qlts/api-client';
import { LanguageSwitcherComponent } from '../../shared/language-switcher/language-switcher.component';

@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [
    CommonModule,
    RouterOutlet,
    PanelMenuModule,
    ButtonModule,
    AvatarModule,
    MenuModule,
    TooltipModule,
    BadgeModule,
    LanguageSwitcherComponent
  ],
  templateUrl: './main-layout.component.html',
  styleUrl: './main-layout.component.css'
})
export class MainLayoutComponent {
  isSidebarCollapsed = signal(false);

  // User dropdown menu items
  userMenuItems: MenuItem[] = [
    {
      label: 'Profile',
      icon: 'pi pi-user',
      command: () => this.goToProfile()
    },
    {
      label: 'Settings',
      icon: 'pi pi-cog',
      command: () => this.goToSettings()
    },
    {
      separator: true
    },
    {
      label: 'Logout',
      icon: 'pi pi-sign-out',
      styleClass: 'text-danger',
      command: () => this.logout()
    }
  ];

  // Computed signal that converts MenuDto to PrimeNG MenuItem
  menuItems = computed(() => {
    const menus = this.menuService.menus();
    console.log('Raw menus from backend:', menus);
    const items = this.convertToMenuItems(menus);
    console.log('Converted menu items:', items);
    return items;
  });

  constructor(
    public authService: AuthService,
    private menuService: MenuService,
    private router: Router
  ) {}

  /**
   * Convert MenuDto hierarchy to PrimeNG MenuItem format
   */
  private convertToMenuItems(menus: MenuDto[]): MenuItem[] {
    return menus.map(menu => {
      const hasChildren = menu.children && menu.children.length > 0;

      return {
        label: menu.name,
        icon: menu.icon || 'pi pi-circle',
        routerLink: menu.route ? [menu.route] : undefined,
        items: hasChildren ? this.convertToMenuItems(menu.children) : undefined,
        expanded: false,
        title: menu.name // Add tooltip
      };
    });
  }

  toggleSidebar(): void {
    this.isSidebarCollapsed.update(collapsed => !collapsed);
  }

  goToProfile(): void {
    this.router.navigate(['/profile']);
  }

  goToSettings(): void {
    this.router.navigate(['/settings']);
  }

  logout(): void {
    this.authService.logout();
  }
}
