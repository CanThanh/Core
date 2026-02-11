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
import { TranslateModule, TranslateService } from '@ngx-translate/core';

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
    DropdownModule,
    TranslateModule
  ],
  templateUrl: './group-dialog.component.html',
  styleUrl: './group-dialog.component.css'
})
export class GroupDialogComponent implements OnInit {
  @Input() visible = false;
  @Input() mode: 'create' | 'edit' = 'create';
  @Input() groupId?: string;
  @Input() groupData?: any;

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
    if (this.visible && this.mode === 'edit' && this.groupData) {
      this.groupForm.patchValue({
        name: this.groupData.name,
        description: this.groupData.description,
        isActive: this.groupData.isActive
      });
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

  resetForm(): void {
    this.groupForm.reset({ isActive: true });
  }

  onSave(): void {
    if (this.groupForm.invalid) {
      this.messageService.add({
        severity: 'warn',
        summary: this.translate.instant('common.validationError'),
        detail: this.translate.instant('common.fillRequired')
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
            summary: this.translate.instant('common.success'),
            detail: this.translate.instant('groups.created')
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
      this.groupsApiService.updateGroup(this.groupId!, request).subscribe({
        next: () => {
          this.messageService.add({
            severity: 'success',
            summary: this.translate.instant('common.success'),
            detail: this.translate.instant('groups.updated')
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
    return this.translate.instant(this.mode === 'create' ? 'groups.createGroup' : 'groups.editGroup');
  }

}
