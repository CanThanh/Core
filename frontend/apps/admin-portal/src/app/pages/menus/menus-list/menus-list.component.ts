import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { TreeTableModule } from 'primeng/treetable';
import { TreeNode } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { CheckboxModule } from 'primeng/checkbox';
import { DropdownModule } from 'primeng/dropdown';
import { ToastModule } from 'primeng/toast';
import { TooltipModule } from 'primeng/tooltip';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService, MessageService } from 'primeng/api';
import { MenusApiService, MenuDto, CreateMenuRequest, UpdateMenuRequest } from '@qlts/api-client';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'app-menus-list',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    ReactiveFormsModule,
    TreeTableModule,
    ButtonModule,
    DialogModule,
    InputTextModule,
    InputNumberModule,
    CheckboxModule,
    DropdownModule,
    ToastModule,
    TooltipModule,
    ConfirmDialogModule,
    TranslateModule
  ],
  providers: [ConfirmationService, MessageService],
  templateUrl: './menus-list.component.html',
  styleUrl: './menus-list.component.css'
})
export class MenusListComponent implements OnInit {
  menuNodes = signal<TreeNode[]>([]);
  loading = signal<boolean>(false);

  displayDialog = false;
  dialogMode: 'create' | 'edit' = 'create';
  menuForm!: FormGroup;
  selectedMenuId?: string;

  availableParents = signal<{ label: string; value: string | null }[]>([]);

  // Common PrimeNG icons for dropdown
  iconOptions = [
    { label: 'Home', value: 'pi pi-home' },
    { label: 'Box', value: 'pi pi-box' },
    { label: 'Users', value: 'pi pi-users' },
    { label: 'User', value: 'pi pi-user' },
    { label: 'List', value: 'pi pi-list' },
    { label: 'Sitemap', value: 'pi pi-sitemap' },
    { label: 'Cog', value: 'pi pi-cog' },
    { label: 'Chart', value: 'pi pi-chart-bar' },
    { label: 'File', value: 'pi pi-file' },
    { label: 'Folder', value: 'pi pi-folder' }
  ];

  constructor(
    private menusApiService: MenusApiService,
    private fb: FormBuilder,
    private confirmationService: ConfirmationService,
    private messageService: MessageService,
    private translate: TranslateService
  ) {
    this.menuForm = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(100)]],
      icon: [''],
      route: ['', Validators.maxLength(200)],
      displayOrder: [0, [Validators.required, Validators.min(0)]],
      isActive: [true],
      parentId: [null]
    });
  }

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

  showCreateDialog(): void {
    this.dialogMode = 'create';
    this.menuForm.reset({ displayOrder: 0, isActive: true, parentId: null });
    this.selectedMenuId = undefined;
    this.displayDialog = true;
  }

  showEditDialog(menuId: string): void {
    // Find menu data in tree
    const menu = this.findMenuById(menuId);
    if (!menu) return;

    this.dialogMode = 'edit';
    this.selectedMenuId = menuId;
    this.menuForm.patchValue({
      name: menu.name,
      icon: menu.icon,
      route: menu.route,
      displayOrder: menu.displayOrder,
      isActive: true, // We don't store this in tree, assume true
      parentId: null // We don't store parent in current tree structure
    });
    this.displayDialog = true;
  }

  findMenuById(id: string): any {
    const search = (nodes: TreeNode[]): any => {
      for (const node of nodes) {
        if (node.data.id === id) return node.data;
        if (node.children) {
          const found = search(node.children);
          if (found) return found;
        }
      }
      return null;
    };
    return search(this.menuNodes());
  }

  saveMenu(): void {
    if (this.menuForm.invalid) {
      this.messageService.add({
        severity: 'warn',
        summary: this.translate.instant('common.validationError'),
        detail: this.translate.instant('common.fillRequired')
      });
      return;
    }

    const formValue = this.menuForm.value;

    if (this.dialogMode === 'create') {
      const request: CreateMenuRequest = {
        name: formValue.name,
        icon: formValue.icon || null,
        route: formValue.route || null,
        displayOrder: formValue.displayOrder,
        isActive: formValue.isActive,
        parentId: formValue.parentId
      };

      this.menusApiService.createMenu(request).subscribe({
        next: () => {
          this.messageService.add({
            severity: 'success',
            summary: this.translate.instant('common.success'),
            detail: this.translate.instant('menus.createSuccess')
          });
          this.displayDialog = false;
          this.loadMenus();
        },
        error: (error) => {
          this.messageService.add({
            severity: 'error',
            summary: this.translate.instant('common.error'),
            detail: error.error?.error || this.translate.instant('menus.createFailed')
          });
        }
      });
    } else {
      const request: UpdateMenuRequest = {
        name: formValue.name,
        icon: formValue.icon || null,
        route: formValue.route || null,
        displayOrder: formValue.displayOrder,
        isActive: formValue.isActive,
        parentId: formValue.parentId
      };

      this.menusApiService.updateMenu(this.selectedMenuId!, request).subscribe({
        next: () => {
          this.messageService.add({
            severity: 'success',
            summary: this.translate.instant('common.success'),
            detail: this.translate.instant('menus.updateSuccess')
          });
          this.displayDialog = false;
          this.loadMenus();
        },
        error: (error) => {
          this.messageService.add({
            severity: 'error',
            summary: this.translate.instant('common.error'),
            detail: error.error?.error || this.translate.instant('menus.updateFailed')
          });
        }
      });
    }
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

  getDialogHeader(): string {
    return this.dialogMode === 'create' ? this.translate.instant('menus.createMenu') : this.translate.instant('menus.editMenu');
  }
}
