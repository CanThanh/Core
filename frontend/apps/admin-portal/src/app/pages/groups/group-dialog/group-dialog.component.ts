import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { InputTextareaModule } from 'primeng/inputtextarea';
import { DropdownModule } from 'primeng/dropdown';
import { MessageService } from 'primeng/api';
import { GroupsApiService } from '@qlts/api-client';

@Component({
  selector: 'app-group-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    DialogModule,
    ButtonModule,
    InputTextModule,
    InputTextareaModule,
    DropdownModule
  ],
  templateUrl: './group-dialog.component.html',
  styleUrl: './group-dialog.component.css'
})
export class GroupDialogComponent implements OnInit {
  @Input() visible = false;
  @Input() mode: 'create' | 'edit' = 'create';
  @Input() groupId?: string;

  @Output() visibleChange = new EventEmitter<boolean>();
  @Output() saved = new EventEmitter<void>();

  groupForm!: FormGroup;
  statusOptions = [
    { label: 'Active', value: true },
    { label: 'Inactive', value: false }
  ];

  constructor(
    private fb: FormBuilder,
    private groupsApiService: GroupsApiService,
    private messageService: MessageService
  ) {}

  ngOnInit(): void {
    this.initForm();
  }

  ngOnChanges(): void {
    if (this.visible && this.groupId && this.mode === 'edit') {
      this.loadGroup();
    } else if (this.visible && this.mode === 'create') {
      this.resetForm();
    }
  }

  initForm(): void {
    this.groupForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(2)]],
      description: [''],
      isActive: [true]
    });
  }

  loadGroup(): void {
    // Note: We'll load from the list data passed in, or implement getGroupById in API
    // For now, we'll need to pass the group data from parent
  }

  resetForm(): void {
    this.groupForm.reset({ isActive: true });
  }

  onSave(): void {
    if (this.groupForm.invalid) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Validation Error',
        detail: 'Please fill all required fields correctly'
      });
      return;
    }

    const request = {
      name: this.groupForm.value.name,
      description: this.groupForm.value.description || null,
      isActive: this.groupForm.value.isActive
    };

    if (this.mode === 'create') {
      this.groupsApiService.createGroup(request).subscribe({
        next: () => {
          this.messageService.add({
            severity: 'success',
            summary: 'Success',
            detail: 'Group created successfully'
          });
          this.onClose();
          this.saved.emit();
        },
        error: (error) => {
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: error.error?.message || 'Failed to create group'
          });
        }
      });
    } else {
      this.groupsApiService.updateGroup(this.groupId!, request).subscribe({
        next: () => {
          this.messageService.add({
            severity: 'success',
            summary: 'Success',
            detail: 'Group updated successfully'
          });
          this.onClose();
          this.saved.emit();
        },
        error: (error) => {
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: error.error?.message || 'Failed to update group'
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
    return this.mode === 'create' ? 'Create New Group' : 'Edit Group';
  }

  // Method to populate form with group data from parent
  setGroupData(group: any): void {
    this.groupForm.patchValue({
      name: group.name,
      description: group.description,
      isActive: group.isActive
    });
  }
}
