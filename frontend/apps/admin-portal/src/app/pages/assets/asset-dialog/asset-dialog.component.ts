import { Component, EventEmitter, Input, OnInit, Output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { InputTextareaModule } from 'primeng/inputtextarea';
import { InputNumberModule } from 'primeng/inputnumber';
import { CalendarModule } from 'primeng/calendar';
import { DropdownModule } from 'primeng/dropdown';
import { MessageService } from 'primeng/api';
import { AssetsApiService } from '@qlts/api-client';

@Component({
  selector: 'app-asset-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    DialogModule,
    ButtonModule,
    InputTextModule,
    InputTextareaModule,
    InputNumberModule,
    CalendarModule,
    DropdownModule
  ],
  templateUrl: './asset-dialog.component.html',
  styleUrl: './asset-dialog.component.css'
})
export class AssetDialogComponent implements OnInit {
  @Input() visible = false;
  @Input() mode: 'create' | 'edit' = 'create';
  @Input() assetId?: string;

  @Output() visibleChange = new EventEmitter<boolean>();
  @Output() saved = new EventEmitter<void>();

  assetForm!: FormGroup;
  loading = signal<boolean>(false);

  categories = signal<any[]>([]);
  assetStatusOptions = [
    { label: 'Đang sử dụng', value: 'InUse' },
    { label: 'Bảo trì', value: 'Maintenance' },
    { label: 'Hỏng', value: 'Broken' },
    { label: 'Đã thanh lý', value: 'Disposed' }
  ];
  activeStatusOptions = [
    { label: 'Active', value: true },
    { label: 'Inactive', value: false }
  ];

  constructor(
    private fb: FormBuilder,
    private assetsApiService: AssetsApiService,
    private messageService: MessageService
  ) {}

  ngOnInit(): void {
    this.initForm();
    this.loadCategories();
  }

  ngOnChanges(): void {
    if (this.visible && this.assetId && this.mode === 'edit') {
      this.loadAsset();
    } else if (this.visible && this.mode === 'create') {
      this.resetForm();
    }
  }

  initForm(): void {
    this.assetForm = this.fb.group({
      code: ['', [Validators.required]],
      name: ['', [Validators.required]],
      categoryId: ['', [Validators.required]],
      manufacturer: [''],
      serialNumber: [''],
      purchasePrice: [0, [Validators.required, Validators.min(0)]],
      purchaseDate: [new Date(), [Validators.required]],
      depreciationRate: [0, [Validators.required, Validators.min(0), Validators.max(100)]],
      location: [''],
      status: ['InUse', [Validators.required]],
      isActive: [true]
    });
  }

  loadCategories(): void {
    // TODO: Implement API call to load categories
    // For now, mock data
    this.categories.set([
      { label: 'Máy tính', value: '1' },
      { label: 'Thiết bị văn phòng', value: '2' },
      { label: 'Xe cộ', value: '3' }
    ]);
  }

  loadAsset(): void {
    if (!this.assetId) return;

    this.loading.set(true);
    this.assetsApiService.getAssetById(this.assetId).subscribe({
      next: (asset) => {
        this.assetForm.patchValue({
          code: asset.code,
          name: asset.name,
          categoryId: asset.categoryId,
          manufacturer: asset.manufacturer,
          serialNumber: asset.serialNumber,
          purchasePrice: asset.purchasePrice,
          purchaseDate: new Date(asset.purchaseDate),
          depreciationRate: asset.depreciationRate,
          location: asset.location,
          status: asset.status,
          isActive: asset.isActive
        });

        // Disable code field in edit mode
        this.assetForm.get('code')?.disable();

        this.loading.set(false);
      },
      error: () => {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Failed to load asset'
        });
        this.loading.set(false);
      }
    });
  }

  resetForm(): void {
    this.assetForm.reset({
      purchaseDate: new Date(),
      status: 'InUse',
      isActive: true,
      purchasePrice: 0,
      depreciationRate: 0
    });

    // Enable code field in create mode
    this.assetForm.get('code')?.enable();
  }

  onSave(): void {
    if (this.assetForm.invalid) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Validation Error',
        detail: 'Please fill all required fields correctly'
      });
      return;
    }

    const formValue = this.assetForm.getRawValue(); // getRawValue to include disabled fields

    if (this.mode === 'create') {
      const createRequest = {
        code: formValue.code,
        name: formValue.name,
        categoryId: formValue.categoryId,
        manufacturer: formValue.manufacturer,
        serialNumber: formValue.serialNumber,
        purchasePrice: formValue.purchasePrice,
        purchaseDate: formValue.purchaseDate,
        depreciationRate: formValue.depreciationRate,
        location: formValue.location
      };

      this.assetsApiService.createAsset(createRequest).subscribe({
        next: () => {
          this.messageService.add({
            severity: 'success',
            summary: 'Success',
            detail: 'Asset created successfully'
          });
          this.onClose();
          this.saved.emit();
        },
        error: (error) => {
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: error.error?.error || 'Failed to create asset'
          });
        }
      });
    } else {
      const updateRequest = {
        name: formValue.name,
        categoryId: formValue.categoryId,
        manufacturer: formValue.manufacturer,
        serialNumber: formValue.serialNumber,
        purchasePrice: formValue.purchasePrice,
        purchaseDate: formValue.purchaseDate,
        depreciationRate: formValue.depreciationRate,
        location: formValue.location,
        status: formValue.status,
        isActive: formValue.isActive
      };

      this.assetsApiService.updateAsset(this.assetId!, updateRequest).subscribe({
        next: () => {
          this.messageService.add({
            severity: 'success',
            summary: 'Success',
            detail: 'Asset updated successfully'
          });
          this.onClose();
          this.saved.emit();
        },
        error: (error) => {
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: error.error?.error || 'Failed to update asset'
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
    return this.mode === 'create' ? 'Thêm Tài sản Mới' : 'Chỉnh sửa Tài sản';
  }
}
