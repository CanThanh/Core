import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { InputTextareaModule } from 'primeng/inputtextarea';
import { DropdownModule } from 'primeng/dropdown';
import { MessageService } from 'primeng/api';
import { RolesApiService } from '@qlts/api-client';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'app-role-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    DialogModule,
    ButtonModule,
    InputTextModule,
    InputTextareaModule,
    DropdownModule,
    TranslateModule
  ],
  templateUrl: './role-dialog.component.html',
  styleUrl: './role-dialog.component.css'
})
export class RoleDialogComponent implements OnInit {
  @Input() visible = false;
  @Input() mode: 'create' | 'edit' = 'create';
  @Input() roleId?: string;

  @Output() visibleChange = new EventEmitter<boolean>();
  @Output() saved = new EventEmitter<void>();

  roleForm!: FormGroup;
  statusOptions = [
    { label: 'Active', value: true },
    { label: 'Inactive', value: false }
  ];

  constructor(
    private fb: FormBuilder,
    private rolesApiService: RolesApiService,
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
    if (this.visible && this.roleId && this.mode === 'edit') {
      this.loadRole();
    } else if (this.visible && this.mode === 'create') {
      this.resetForm();
    }
  }

  initForm(): void {
    this.roleForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(2)]],
      description: [''],
      isActive: [true]
    });
  }

  loadRole(): void {
    if (!this.roleId) return;

    this.rolesApiService.getRoleById(this.roleId).subscribe({
      next: (role) => {
        this.roleForm.patchValue({
          name: role.name,
          description: role.description,
          isActive: role.isActive
        });
      }
    });
  }

  resetForm(): void {
    this.roleForm.reset({ isActive: true });
  }

  onSave(): void {
    if (this.roleForm.invalid) {
      this.messageService.add({
        severity: 'warn',
        summary: this.translate.instant('common.validationError'),
        detail: this.translate.instant('common.fillRequired')
      });
      return;
    }

    const request = {
      name: this.roleForm.value.name,
      description: this.roleForm.value.description || null,
      isActive: this.roleForm.value.isActive
    };

    if (this.mode === 'create') {
      this.rolesApiService.createRole(request).subscribe({
        next: () => {
          this.messageService.add({
            severity: 'success',
            summary: this.translate.instant('common.success'),
            detail: this.translate.instant('roles.created')
          });
          this.onClose();
          this.saved.emit();
        },
        error: (error) => {
          this.messageService.add({
            severity: 'error',
            summary: this.translate.instant('common.error'),
            detail: error.error?.message || this.translate.instant('common.createFailed')
          });
        }
      });
    } else {
      this.rolesApiService.updateRole(this.roleId!, request).subscribe({
        next: () => {
          this.messageService.add({
            severity: 'success',
            summary: this.translate.instant('common.success'),
            detail: this.translate.instant('roles.updated')
          });
          this.onClose();
          this.saved.emit();
        },
        error: (error) => {
          this.messageService.add({
            severity: 'error',
            summary: this.translate.instant('common.error'),
            detail: error.error?.message || this.translate.instant('common.updateFailed')
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
    return this.translate.instant(this.mode === 'create' ? 'roles.createRole' : 'roles.editRole');
  }

  setRoleData(role: any): void {
    this.roleForm.patchValue({
      name: role.name,
      description: role.description,
      isActive: role.isActive
    });
  }
}
