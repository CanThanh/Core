import { Component, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet, Router } from '@angular/router';
import { PanelMenuModule } from 'primeng/panelmenu';
import { ButtonModule } from 'primeng/button';
import { AvatarModule } from 'primeng/avatar';
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
    LanguageSwitcherComponent
  ],
  templateUrl: './main-layout.component.html',
  styleUrl: './main-layout.component.css'
})
export class MainLayoutComponent {
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
        expanded: false
      };
    });
  }

  logout(): void {
    this.authService.logout();
  }
}
