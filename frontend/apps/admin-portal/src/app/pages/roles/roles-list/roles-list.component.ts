import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { InputTextareaModule } from 'primeng/inputtextarea';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { DialogModule } from 'primeng/dialog';
import { CheckboxModule } from 'primeng/checkbox';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ToastModule } from 'primeng/toast';
import { ConfirmationService, MessageService } from 'primeng/api';
import { RolesApiService } from '@qlts/api-client';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'app-roles-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    TableModule,
    ButtonModule,
    InputTextModule,
    InputTextareaModule,
    TagModule,
    TooltipModule,
    DialogModule,
    CheckboxModule,
    ConfirmDialogModule,
    ToastModule,
    TranslateModule
  ],
  providers: [ConfirmationService, MessageService],
  templateUrl: './roles-list.component.html',
  styleUrl: './roles-list.component.css'
})
export class RolesListComponent implements OnInit {
  roles = signal<any[]>([]);
  loading = signal<boolean>(false);

  // Dialog for create/edit
  displayDialog = false;
  dialogMode: 'create' | 'edit' = 'create';
  roleForm!: FormGroup;
  selectedRoleId?: string;

  constructor(
    private rolesApiService: RolesApiService,
    private router: Router,
    private confirmationService: ConfirmationService,
    private messageService: MessageService,
    private fb: FormBuilder,
    private translate: TranslateService
  ) {
    this.roleForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(2)]],
      description: [''],
      isActive: [true]
    });
  }

  ngOnInit(): void {
    this.loadRoles();
  }

  loadRoles(): void {
    this.loading.set(true);
    this.rolesApiService.getRoles().subscribe({
      next: (roles) => {
        this.roles.set(roles);
        this.loading.set(false);
      },
      error: (error) => {
        console.error('Failed to load roles:', error);
        this.messageService.add({
          severity: 'error',
          summary: this.translate.instant('common.error'),
          detail: this.translate.instant('common.error')
        });
        this.loading.set(false);
      }
    });
  }

  showCreateDialog(): void {
    this.dialogMode = 'create';
    this.roleForm.reset({ isActive: true });
    this.selectedRoleId = undefined;
    this.displayDialog = true;
  }

  showEditDialog(role: any): void {
    this.dialogMode = 'edit';
    this.selectedRoleId = role.id;
    this.roleForm.patchValue({
      name: role.name,
      description: role.description,
      isActive: role.isActive
    });
    this.displayDialog = true;
  }

  saveRole(): void {
    if (this.roleForm.invalid) {
      this.messageService.add({
        severity: 'warn',
        summary: this.translate.instant('common.error'),
        detail: this.translate.instant('common.error')
      });
      return;
    }

    const request = {
      name: this.roleForm.value.name,
      description: this.roleForm.value.description || null,
      isActive: this.roleForm.value.isActive
    };

    if (this.dialogMode === 'create') {
      this.rolesApiService.createRole(request).subscribe({
        next: () => {
          this.messageService.add({
            severity: 'success',
            summary: this.translate.instant('common.success'),
            detail: this.translate.instant('roles.created')
          });
          this.displayDialog = false;
          this.loadRoles();
        },
        error: (error) => {
          console.error('Failed to create role:', error);
          this.messageService.add({
            severity: 'error',
            summary: this.translate.instant('common.error'),
            detail: error.error?.message || this.translate.instant('common.error')
          });
        }
      });
    } else {
      this.rolesApiService.updateRole(this.selectedRoleId!, request).subscribe({
        next: () => {
          this.messageService.add({
            severity: 'success',
            summary: this.translate.instant('common.success'),
            detail: this.translate.instant('roles.updated')
          });
          this.displayDialog = false;
          this.loadRoles();
        },
        error: (error) => {
          console.error('Failed to update role:', error);
          this.messageService.add({
            severity: 'error',
            summary: this.translate.instant('common.error'),
            detail: error.error?.message || this.translate.instant('common.error')
          });
        }
      });
    }
  }

  deleteRole(roleId: string, roleName: string): void {
    this.confirmationService.confirm({
      message: this.translate.instant('common.areYouSure'),
      header: this.translate.instant('common.confirmDelete'),
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.rolesApiService.deleteRole(roleId).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: this.translate.instant('common.success'),
              detail: this.translate.instant('roles.deleted')
            });
            this.loadRoles();
          },
          error: (error) => {
            console.error('Failed to delete role:', error);
            this.messageService.add({
              severity: 'error',
              summary: this.translate.instant('common.error'),
              detail: error.error || this.translate.instant('common.error')
            });
          }
        });
      }
    });
  }

  managePermissions(roleId: string): void {
    this.router.navigate(['/roles/permissions', roleId]);
  }

  getStatusSeverity(isActive: boolean): string {
    return isActive ? 'success' : 'danger';
  }

  getStatusLabel(isActive: boolean): string {
    return isActive
      ? this.translate.instant('common.active')
      : this.translate.instant('common.inactive');
  }

  getDialogHeader(): string {
    return this.dialogMode === 'create'
      ? this.translate.instant('roles.createRole')
      : this.translate.instant('roles.editRole');
  }
}
