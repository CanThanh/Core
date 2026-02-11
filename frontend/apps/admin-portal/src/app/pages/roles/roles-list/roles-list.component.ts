import { Component, OnInit, signal, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ToastModule } from 'primeng/toast';
import { ConfirmationService, MessageService } from 'primeng/api';
import { RolesApiService } from '@qlts/api-client';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { RoleDialogComponent } from '../role-dialog/role-dialog.component';

@Component({
  selector: 'app-roles-list',
  standalone: true,
  imports: [
    CommonModule,
    TableModule,
    ButtonModule,
    TagModule,
    TooltipModule,
    ConfirmDialogModule,
    ToastModule,
    TranslateModule,
    RoleDialogComponent
  ],
  providers: [ConfirmationService, MessageService],
  templateUrl: './roles-list.component.html',
  styleUrl: './roles-list.component.css'
})
export class RolesListComponent implements OnInit {
  roles = signal<any[]>([]);
  loading = signal<boolean>(false);

  showDialog = false;
  dialogMode: 'create' | 'edit' = 'create';
  selectedRoleId?: string;

  constructor(
    private rolesApiService: RolesApiService,
    private router: Router,
    private confirmationService: ConfirmationService,
    private messageService: MessageService,
    private translate: TranslateService
  ) {}

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
      error: () => {
        this.messageService.add({
          severity: 'error',
          summary: this.translate.instant('common.error'),
          detail: this.translate.instant('roles.loadFailed')
        });
        this.loading.set(false);
      }
    });
  }

  openCreateDialog(): void {
    this.dialogMode = 'create';
    this.selectedRoleId = undefined;
    this.showDialog = true;
  }

  openEditDialog(role: any): void {
    this.dialogMode = 'edit';
    this.selectedRoleId = role.id;
    this.showDialog = true;
  }

  onRoleSaved(): void {
    this.showDialog = false;
    this.loadRoles();
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
            this.messageService.add({
              severity: 'error',
              summary: this.translate.instant('common.error'),
              detail: error.error || this.translate.instant('common.deleteFailed')
            });
          }
        });
      }
    });
  }

  managePermissions(): void {
    this.router.navigate(['/roles/menu-permissions']);
  }

  getStatusSeverity(isActive: boolean): string {
    return isActive ? 'success' : 'danger';
  }

  getStatusLabel(isActive: boolean): string {
    return isActive
      ? this.translate.instant('common.active')
      : this.translate.instant('common.inactive');
  }
}
