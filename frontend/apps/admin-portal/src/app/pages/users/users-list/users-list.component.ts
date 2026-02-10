import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ToastModule } from 'primeng/toast';
import { ConfirmationService, MessageService } from 'primeng/api';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { UsersApiService } from '@qlts/api-client';
import { UserDialogComponent } from '../user-dialog/user-dialog.component';

@Component({
  selector: 'app-users-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    TableModule,
    ButtonModule,
    InputTextModule,
    TagModule,
    TooltipModule,
    ConfirmDialogModule,
    ToastModule,
    TranslateModule,
    UserDialogComponent
  ],
  providers: [ConfirmationService, MessageService],
  templateUrl: './users-list.component.html',
  styleUrl: './users-list.component.css'
})
export class UsersListComponent implements OnInit {
  users = signal<any[]>([]);
  totalRecords = signal<number>(0);
  loading = signal<boolean>(false);

  pageNumber = 1;
  pageSize = 10;
  searchTerm = '';

  // Dialog state
  showDialog = false;
  dialogMode: 'create' | 'edit' = 'create';
  selectedUserId?: string;

  constructor(
    private usersApiService: UsersApiService,
    private confirmationService: ConfirmationService,
    private messageService: MessageService,
    private translate: TranslateService
  ) {}

  ngOnInit(): void {
    this.loadUsers();
  }

  loadUsers(): void {
    this.loading.set(true);
    this.usersApiService.getUsers(this.pageNumber, this.pageSize, this.searchTerm).subscribe({
      next: (response) => {
        this.users.set(response.items);
        this.totalRecords.set(response.totalCount);
        this.loading.set(false);
      },
      error: (error) => {
        console.error('Failed to load users:', error);
        this.messageService.add({
          severity: 'error',
          summary: this.translate.instant('common.error'),
          detail: 'Failed to load users'
        });
        this.loading.set(false);
      }
    });
  }

  onPageChange(event: any): void {
    this.pageNumber = event?.page != null ? event.page + 1 : 1;
    this.pageSize = event?.rows || 10;
    this.loadUsers();
  }

  onSearch(): void {
    this.pageNumber = 1;
    this.loadUsers();
  }

  openCreateDialog(): void {
    this.dialogMode = 'create';
    this.selectedUserId = undefined;
    this.showDialog = true;
  }

  openEditDialog(userId: string): void {
    this.dialogMode = 'edit';
    this.selectedUserId = userId;
    this.showDialog = true;
  }

  onUserSaved(): void {
    this.showDialog = false;
    this.loadUsers();
  }

  deleteUser(userId: string, username: string): void {
    this.confirmationService.confirm({
      message: this.translate.instant('common.areYouSure', { name: username }),
      header: this.translate.instant('common.confirmDelete'),
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.usersApiService.deleteUser(userId).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: this.translate.instant('common.success'),
              detail: this.translate.instant('users.deleted')
            });
            this.loadUsers();
          },
          error: (error) => {
            console.error('Failed to delete user:', error);
            this.messageService.add({
              severity: 'error',
              summary: this.translate.instant('common.error'),
              detail: 'Failed to delete user'
            });
          }
        });
      }
    });
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
