import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { TreeTableModule } from 'primeng/treetable';
import { TreeNode } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { ToastModule } from 'primeng/toast';
import { TooltipModule } from 'primeng/tooltip';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService, MessageService } from 'primeng/api';
import { MenusApiService, MenuDto } from '@qlts/api-client';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { MenuDialogComponent } from '../menu-dialog/menu-dialog.component';

@Component({
  selector: 'app-menus-list',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    TreeTableModule,
    ButtonModule,
    ToastModule,
    TooltipModule,
    ConfirmDialogModule,
    TranslateModule,
    MenuDialogComponent
  ],
  providers: [ConfirmationService, MessageService],
  templateUrl: './menus-list.component.html',
  styleUrl: './menus-list.component.css'
})
export class MenusListComponent implements OnInit {
  menuNodes = signal<TreeNode[]>([]);
  loading = signal<boolean>(false);

  // Dialog state
  showDialog = false;
  dialogMode: 'create' | 'edit' = 'create';
  selectedMenuId?: string;
  selectedMenuData?: any;
  availableParents = signal<{ label: string; value: string | null }[]>([]);

  constructor(
    private menusApiService: MenusApiService,
    private confirmationService: ConfirmationService,
    private messageService: MessageService,
    private translate: TranslateService
  ) {}

  ngOnInit(): void {
    this.loadMenus();
  }

  loadMenus(): void {
    this.loading.set(true);
    this.menusApiService.getMenus().subscribe({
      next: (menus) => {
        this.menuNodes.set(this.convertToTreeNodes(menus));
        this.updateAvailableParents(menus);
        this.loading.set(false);
      },
      error: (error) => {
        console.error('Failed to load menus:', error);
        this.messageService.add({
          severity: 'error',
          summary: this.translate.instant('common.error'),
          detail: this.translate.instant('menus.loadFailed')
        });
        this.loading.set(false);
      }
    });
  }

  convertToTreeNodes(menus: MenuDto[]): TreeNode[] {
    return menus.map(menu => ({
      data: {
        id: menu.id,
        name: menu.name,
        icon: menu.icon,
        route: menu.route,
        displayOrder: menu.displayOrder
      },
      children: menu.children && menu.children.length > 0
        ? this.convertToTreeNodes(menu.children)
        : [],
      expanded: true
    }));
  }

  updateAvailableParents(menus: MenuDto[]): void {
    const parents: { label: string; value: string | null }[] = [
      { label: this.translate.instant('menus.noParent'), value: null }
    ];

    const flatten = (items: MenuDto[], level: number = 0) => {
      for (const item of items) {
        const prefix = '—'.repeat(level);
        parents.push({
          label: `${prefix} ${item.name}`,
          value: item.id
        });
        if (item.children && item.children.length > 0) {
          flatten(item.children, level + 1);
        }
      }
    };

    flatten(menus);
    this.availableParents.set(parents);
  }

  openCreateDialog(): void {
    this.dialogMode = 'create';
    this.selectedMenuId = undefined;
    this.selectedMenuData = undefined;
    this.showDialog = true;
  }

  openEditDialog(menuData: any): void {
    this.dialogMode = 'edit';
    this.selectedMenuId = menuData.id;
    this.selectedMenuData = menuData;
    this.showDialog = true;
  }

  onMenuSaved(): void {
    this.showDialog = false;
    this.loadMenus();
  }

  deleteMenu(menuId: string, menuName: string): void {
    this.confirmationService.confirm({
      message: this.translate.instant('menus.deleteConfirm', { name: menuName }),
      header: this.translate.instant('menus.confirmDelete'),
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.menusApiService.deleteMenu(menuId).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: this.translate.instant('common.success'),
              detail: this.translate.instant('menus.deleteSuccess')
            });
            this.loadMenus();
          },
          error: (error) => {
            this.messageService.add({
              severity: 'error',
              summary: this.translate.instant('common.error'),
              detail: error.error?.error || this.translate.instant('menus.deleteFailed')
            });
          }
        });
      }
    });
  }
}
