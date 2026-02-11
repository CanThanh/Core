import { Component, EventEmitter, Input, OnInit, Output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { DropdownModule } from 'primeng/dropdown';
import { MessageService } from 'primeng/api';
import { MenusApiService, MenuDto, CreateMenuRequest, UpdateMenuRequest } from '@qlts/api-client';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'app-menu-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    DialogModule,
    ButtonModule,
    InputTextModule,
    InputNumberModule,
    DropdownModule,
    TranslateModule
  ],
  templateUrl: './menu-dialog.component.html',
  styleUrl: './menu-dialog.component.css'
})
export class MenuDialogComponent implements OnInit {
  @Input() visible = false;
  @Input() mode: 'create' | 'edit' = 'create';
  @Input() menuId?: string;
  @Input() menuData?: any;
  @Input() availableParents: { label: string; value: string | null }[] = [];

  @Output() visibleChange = new EventEmitter<boolean>();
  @Output() saved = new EventEmitter<void>();

  menuForm!: FormGroup;
  statusOptions: { label: string; value: boolean }[] = [];

  iconOptions = [
    { label: 'Home', value: 'pi pi-home' },
    { label: 'Briefcase', value: 'pi pi-briefcase' },
    { label: 'Box', value: 'pi pi-box' },
    { label: 'Users', value: 'pi pi-users' },
    { label: 'User', value: 'pi pi-user' },
    { label: 'List', value: 'pi pi-list' },
    { label: 'Sitemap', value: 'pi pi-sitemap' },
    { label: 'Cog', value: 'pi pi-cog' },
    { label: 'Shield', value: 'pi pi-shield' },
    { label: 'Bars', value: 'pi pi-bars' },
    { label: 'Key', value: 'pi pi-key' },
    { label: 'Chart', value: 'pi pi-chart-bar' },
    { label: 'File', value: 'pi pi-file' },
    { label: 'Folder', value: 'pi pi-folder' }
  ];

  constructor(
    private fb: FormBuilder,
    private menusApiService: MenusApiService,
    private messageService: MessageService,
    private translate: TranslateService
  ) {}

  ngOnInit(): void {
    this.initForm();
    this.updateStatusLabels();
    this.translate.onLangChange.subscribe(() => this.updateStatusLabels());
  }

  updateStatusLabels(): void {
    this.statusOptions = [
      { label: this.translate.instant('common.active'), value: true },
      { label: this.translate.instant('common.inactive'), value: false }
    ];
  }

  ngOnChanges(): void {
    if (this.visible && this.mode === 'edit' && this.menuData) {
      this.menuForm.patchValue({
        name: this.menuData.name,
        icon: this.menuData.icon,
        route: this.menuData.route,
        displayOrder: this.menuData.displayOrder,
        isActive: this.menuData.isActive ?? true,
        parentId: this.menuData.parentId ?? null
      });
    } else if (this.visible && this.mode === 'create') {
      this.resetForm();
    }
  }

  initForm(): void {
    this.menuForm = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(100)]],
      icon: [''],
      route: ['', Validators.maxLength(200)],
      displayOrder: [0, [Validators.required, Validators.min(0)]],
      isActive: [true],
      parentId: [null]
    });
  }

  resetForm(): void {
    this.menuForm.reset({ displayOrder: 0, isActive: true, parentId: null });
  }

  onSave(): void {
    if (this.menuForm.invalid) {
      this.messageService.add({
        severity: 'warn',
        summary: this.translate.instant('common.validationError'),
        detail: this.translate.instant('common.fillRequired')
      });
      return;
    }

    const formValue = this.menuForm.value;

    if (this.mode === 'create') {
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
          this.onClose();
          this.saved.emit();
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

      this.menusApiService.updateMenu(this.menuId!, request).subscribe({
        next: () => {
          this.messageService.add({
            severity: 'success',
            summary: this.translate.instant('common.success'),
            detail: this.translate.instant('menus.updateSuccess')
          });
          this.onClose();
          this.saved.emit();
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

  onClose(): void {
    this.visible = false;
    this.visibleChange.emit(false);
  }

  getDialogHeader(): string {
    return this.translate.instant(this.mode === 'create' ? 'menus.createMenu' : 'menus.editMenu');
  }
}
