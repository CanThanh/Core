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
import { GroupsApiService } from '@qlts/api-client';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { GroupDialogComponent } from '../group-dialog/group-dialog.component';

@Component({
  selector: 'app-groups-list',
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
    GroupDialogComponent
  ],
  providers: [ConfirmationService, MessageService],
  templateUrl: './groups-list.component.html',
  styleUrl: './groups-list.component.css'
})
export class GroupsListComponent implements OnInit {
  groups = signal<any[]>([]);
  totalRecords = signal<number>(0);
  loading = signal<boolean>(false);

  pageNumber = 1;
  pageSize = 10;
  searchTerm = '';

  // Dialog state
  showDialog = false;
  dialogMode: 'create' | 'edit' = 'create';
  selectedGroupId?: string;
  selectedGroupData?: any;

  constructor(
    private groupsApiService: GroupsApiService,
    private confirmationService: ConfirmationService,
    private messageService: MessageService,
    private translate: TranslateService
  ) {}

  ngOnInit(): void {
    this.loadGroups();
  }

  loadGroups(): void {
    this.loading.set(true);
    this.groupsApiService.getGroups(this.pageNumber, this.pageSize, this.searchTerm).subscribe({
      next: (response) => {
        this.groups.set(response.items);
        this.totalRecords.set(response.totalCount);
        this.loading.set(false);
      },
      error: (error) => {
        console.error('Failed to load groups:', error);
        this.messageService.add({
          severity: 'error',
          summary: this.translate.instant('common.error'),
          detail: this.translate.instant('common.error')
        });
        this.loading.set(false);
      }
    });
  }

  onPageChange(event: any): void {
    this.pageNumber = event?.page != null ? event.page + 1 : 1;
    this.pageSize = event?.rows || 10;
    this.loadGroups();
  }

  onSearch(): void {
    this.pageNumber = 1;
    this.loadGroups();
  }

  openCreateDialog(): void {
    this.dialogMode = 'create';
    this.selectedGroupId = undefined;
    this.selectedGroupData = undefined;
    this.showDialog = true;
  }

  openEditDialog(group: any): void {
    this.dialogMode = 'edit';
    this.selectedGroupId = group.id;
    this.selectedGroupData = group;
    this.showDialog = true;
  }

  onGroupSaved(): void {
    this.showDialog = false;
    this.loadGroups();
  }

  deleteGroup(groupId: string, groupName: string): void {
    this.confirmationService.confirm({
      message: this.translate.instant('common.areYouSure', { name: groupName }),
      header: this.translate.instant('common.confirmDelete'),
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.groupsApiService.deleteGroup(groupId).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: this.translate.instant('common.success'),
              detail: this.translate.instant('groups.deleted')
            });
            this.loadGroups();
          },
          error: (error) => {
            console.error('Failed to delete group:', error);
            this.messageService.add({
              severity: 'error',
              summary: this.translate.instant('common.error'),
              detail: this.translate.instant('common.deleteFailed')
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
