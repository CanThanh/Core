import { Component, EventEmitter, Input, OnInit, Output, signal, OnChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, FormsModule, Validators } from '@angular/forms';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { DropdownModule } from 'primeng/dropdown';
import { MultiSelectModule } from 'primeng/multiselect';
import { MessageService } from 'primeng/api';
import { UsersApiService, RolesApiService, GroupsApiService } from '@qlts/api-client';

@Component({
  selector: 'app-user-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    DialogModule,
    ButtonModule,
    InputTextModule,
    PasswordModule,
    DropdownModule,
    MultiSelectModule
  ],
  templateUrl: './user-dialog.component.html',
  styleUrl: './user-dialog.component.css'
})
export class UserDialogComponent implements OnInit, OnChanges {
  @Input() visible = false;
  @Input() mode: 'create' | 'edit' = 'create';
  @Input() userId?: string;

  @Output() visibleChange = new EventEmitter<boolean>();
  @Output() saved = new EventEmitter<void>();

  userForm!: FormGroup;
  loading = signal<boolean>(false);

  availableRoles = signal<any[]>([]);
  availableGroups = signal<any[]>([]);

  selectedRoleIds: string[] = [];
  selectedGroupIds: string[] = [];
  statusOptions = [
    { label: 'Active', value: true },
    { label: 'Inactive', value: false }
  ];

  constructor(
    private fb: FormBuilder,
    private usersApiService: UsersApiService,
    private rolesApiService: RolesApiService,
    private groupsApiService: GroupsApiService,
    private messageService: MessageService
  ) {}

  ngOnInit(): void {
    this.initForm();
    this.loadRolesAndGroups();
  }

  ngOnChanges(): void {
    if (this.visible && this.userId && this.mode === 'edit') {
      this.loadUser();
    } else if (this.visible && this.mode === 'create') {
      this.resetForm();
    }
  }

  initForm(): void {
    this.userForm = this.fb.group({
      username: ['', [Validators.required, Validators.minLength(3)]],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      fullName: ['', [Validators.required, Validators.minLength(2)]],
      phoneNumber: [''],
      isActive: [true]
    });
  }

  loadRolesAndGroups(): void {
    this.rolesApiService.getRoles().subscribe({
      next: (roles) => {
        this.availableRoles.set(roles.map(r => ({ label: r.name, value: r.id })));
      }
    });

    this.groupsApiService.getAllGroups().subscribe({
      next: (groups) => {
        this.availableGroups.set(groups.map(g => ({ label: g.name, value: g.id })));
      }
    });
  }

  loadUser(): void {
    if (!this.userId) return;

    this.loading.set(true);
    this.usersApiService.getUserById(this.userId).subscribe({
      next: (user) => {
        this.userForm.patchValue({
          username: user.username,
          email: user.email,
          fullName: user.fullName,
          phoneNumber: user.phoneNumber,
          isActive: user.isActive
        });

        // Map role names to IDs
        const roleIds = this.availableRoles().filter(r =>
          user.roles.includes(r.label)
        ).map(r => r.value);
        this.selectedRoleIds = roleIds;

        // Map group names to IDs
        const groupIds = this.availableGroups().filter(g =>
          user.groups.includes(g.label)
        ).map(g => g.value);
        this.selectedGroupIds = groupIds;

        // For edit mode, password is optional
        this.userForm.get('password')?.clearValidators();
        this.userForm.get('password')?.updateValueAndValidity();

        this.loading.set(false);
      },
      error: () => {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Failed to load user'
        });
        this.loading.set(false);
      }
    });
  }

  resetForm(): void {
    this.userForm.reset({ isActive: true });
    this.selectedRoleIds = [];
    this.selectedGroupIds = [];

    // For create mode, password is required
    this.userForm.get('password')?.setValidators([Validators.required, Validators.minLength(6)]);
    this.userForm.get('password')?.updateValueAndValidity();
  }

  onSave(): void {
    if (this.userForm.invalid) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Validation Error',
        detail: 'Please fill all required fields correctly'
      });
      return;
    }

    if (this.mode === 'create') {
      this.createUser();
    } else {
      this.updateUser();
    }
  }

  createUser(): void {
    const createRequest = {
      username: this.userForm.value.username,
      email: this.userForm.value.email,
      password: this.userForm.value.password,
      fullName: this.userForm.value.fullName,
      phoneNumber: this.userForm.value.phoneNumber || null,
      isActive: this.userForm.value.isActive
    };

    this.usersApiService.createUser(createRequest).subscribe({
      next: (response) => {
        if (this.selectedRoleIds.length > 0) {
          this.usersApiService.assignRolesToUser(response.id, this.selectedRoleIds).subscribe({
            next: () => {
              this.messageService.add({
                severity: 'success',
                summary: 'Success',
                detail: 'User created successfully'
              });
              this.onClose();
              this.saved.emit();
            }
          });
        } else {
          this.messageService.add({
            severity: 'success',
            summary: 'Success',
            detail: 'User created successfully'
          });
          this.onClose();
          this.saved.emit();
        }
      },
      error: (error) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: error.error?.message || 'Failed to create user'
        });
      }
    });
  }

  updateUser(): void {
    const updateRequest = {
      email: this.userForm.value.email,
      fullName: this.userForm.value.fullName,
      phoneNumber: this.userForm.value.phoneNumber || null,
      isActive: this.userForm.value.isActive
    };

    this.usersApiService.updateUser(this.userId!, updateRequest).subscribe({
      next: () => {
        this.usersApiService.assignRolesToUser(this.userId!, this.selectedRoleIds).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: 'Success',
              detail: 'User updated successfully'
            });
            this.onClose();
            this.saved.emit();
          }
        });
      },
      error: (error) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: error.error?.message || 'Failed to update user'
        });
      }
    });
  }

  onClose(): void {
    this.visible = false;
    this.visibleChange.emit(false);
  }

  getDialogHeader(): string {
    return this.mode === 'create' ? 'Create New User' : 'Edit User';
  }
}
