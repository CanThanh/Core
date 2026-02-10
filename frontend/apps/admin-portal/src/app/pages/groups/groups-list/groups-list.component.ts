import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { DialogModule } from 'primeng/dialog';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ToastModule } from 'primeng/toast';
import { ConfirmationService, MessageService } from 'primeng/api';
import { GroupsApiService } from '@qlts/api-client';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { InputTextareaModule } from 'primeng/inputtextarea';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'app-groups-list',
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
    ConfirmDialogModule,
    ToastModule,
    TranslateModule
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

  // Dialog for create/edit
  displayDialog = false;
  dialogMode: 'create' | 'edit' = 'create';
  groupForm!: FormGroup;
  selectedGroupId?: string;

  constructor(
    private groupsApiService: GroupsApiService,
    private router: Router,
    private confirmationService: ConfirmationService,
    private messageService: MessageService,
    private fb: FormBuilder,
    private translate: TranslateService
  ) {
    this.groupForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(2)]],
      description: [''],
      isActive: [true]
    });
  }

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

  showCreateDialog(): void {
    this.dialogMode = 'create';
    this.groupForm.reset({ isActive: true });
    this.selectedGroupId = undefined;
    this.displayDialog = true;
  }

  showEditDialog(group: any): void {
    this.dialogMode = 'edit';
    this.selectedGroupId = group.id;
    this.groupForm.patchValue({
      name: group.name,
      description: group.description,
      isActive: group.isActive
    });
    this.displayDialog = true;
  }

  saveGroup(): void {
    if (this.groupForm.invalid) {
      this.messageService.add({
        severity: 'warn',
        summary: this.translate.instant('common.error'),
        detail: this.translate.instant('common.error')
      });
      return;
    }

    const request = {
      name: this.groupForm.value.name,
      description: this.groupForm.value.description || null,
      isActive: this.groupForm.value.isActive
    };

    if (this.dialogMode === 'create') {
      this.groupsApiService.createGroup(request).subscribe({
        next: () => {
          this.messageService.add({
            severity: 'success',
            summary: this.translate.instant('common.success'),
            detail: this.translate.instant('groups.created')
          });
          this.displayDialog = false;
          this.loadGroups();
        },
        error: (error) => {
          console.error('Failed to create group:', error);
          this.messageService.add({
            severity: 'error',
            summary: this.translate.instant('common.error'),
            detail: error.error?.message || this.translate.instant('common.error')
          });
        }
      });
    } else {
      this.groupsApiService.updateGroup(this.selectedGroupId!, request).subscribe({
        next: () => {
          this.messageService.add({
            severity: 'success',
            summary: this.translate.instant('common.success'),
            detail: this.translate.instant('groups.updated')
          });
          this.displayDialog = false;
          this.loadGroups();
        },
        error: (error) => {
          console.error('Failed to update group:', error);
          this.messageService.add({
            severity: 'error',
            summary: this.translate.instant('common.error'),
            detail: error.error?.message || this.translate.instant('common.error')
          });
        }
      });
    }
  }

  deleteGroup(groupId: string, groupName: string): void {
    this.confirmationService.confirm({
      message: this.translate.instant('common.areYouSure'),
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
              detail: this.translate.instant('common.error')
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

  getDialogHeader(): string {
    return this.dialogMode === 'create'
      ? this.translate.instant('groups.createGroup')
      : this.translate.instant('groups.editGroup');
  }
}
