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
    DropdownModule
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
    private messageService: MessageService
  ) {}

  ngOnInit(): void {
    this.initForm();
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
        summary: 'Validation Error',
        detail: 'Please fill all required fields correctly'
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
            summary: 'Success',
            detail: 'Role created successfully'
          });
          this.onClose();
          this.saved.emit();
        },
        error: (error) => {
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: error.error?.message || 'Failed to create role'
          });
        }
      });
    } else {
      this.rolesApiService.updateRole(this.roleId!, request).subscribe({
        next: () => {
          this.messageService.add({
            severity: 'success',
            summary: 'Success',
            detail: 'Role updated successfully'
          });
          this.onClose();
          this.saved.emit();
        },
        error: (error) => {
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: error.error?.message || 'Failed to update role'
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
    return this.mode === 'create' ? 'Create New Role' : 'Edit Role';
  }

  setRoleData(role: any): void {
    this.roleForm.patchValue({
      name: role.name,
      description: role.description,
      isActive: role.isActive
    });
  }
}
