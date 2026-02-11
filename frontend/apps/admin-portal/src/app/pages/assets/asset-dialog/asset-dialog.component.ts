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
import { TranslateModule, TranslateService } from '@ngx-translate/core';

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
    DropdownModule,
    TranslateModule
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
  assetStatusOptions: any[] = [];
  activeStatusOptions: any[] = [];

  constructor(
    private fb: FormBuilder,
    private assetsApiService: AssetsApiService,
    private messageService: MessageService,
    private translate: TranslateService
  ) {}

  ngOnInit(): void {
    this.initForm();
    this.loadCategories();
    this.updateStatusLabels();
    this.translate.onLangChange.subscribe(() => this.updateStatusLabels());
  }

  updateStatusLabels(): void {
    this.assetStatusOptions = [
      { label: this.translate.instant('assets.status.InUse'), value: 'InUse' },
      { label: this.translate.instant('assets.status.Maintenance'), value: 'Maintenance' },
      { label: this.translate.instant('assets.status.Broken'), value: 'Broken' },
      { label: this.translate.instant('assets.status.Disposed'), value: 'Disposed' }
    ];
    this.activeStatusOptions = [
      { label: this.translate.instant('common.active'), value: true },
      { label: this.translate.instant('common.inactive'), value: false }
    ];
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
    this.assetsApiService.getCategories().subscribe({
      next: (data) => {
        this.categories.set(data.map((c: any) => ({ label: c.name, value: c.id })));
      },
      error: () => {
        this.messageService.add({
          severity: 'error',
          summary: this.translate.instant('common.error'),
          detail: this.translate.instant('assets.loadCategoriesFailed')
        });
      }
    });
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

        this.assetForm.get('code')?.disable();
        this.loading.set(false);
      },
      error: () => {
        this.messageService.add({
          severity: 'error',
          summary: this.translate.instant('common.error'),
          detail: this.translate.instant('users.loadFailed')
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

    this.assetForm.get('code')?.enable();
  }

  onSave(): void {
    if (this.assetForm.invalid) {
      this.messageService.add({
        severity: 'warn',
        summary: this.translate.instant('common.validationError'),
        detail: this.translate.instant('common.fillRequired')
      });
      return;
    }

    const formValue = this.assetForm.getRawValue();

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
            summary: this.translate.instant('common.success'),
            detail: this.translate.instant('assets.created')
          });
          this.onClose();
          this.saved.emit();
        },
        error: (error) => {
          this.messageService.add({
            severity: 'error',
            summary: this.translate.instant('common.error'),
            detail: error.error?.error || this.translate.instant('common.createFailed')
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
            summary: this.translate.instant('common.success'),
            detail: this.translate.instant('assets.updated')
          });
          this.onClose();
          this.saved.emit();
        },
        error: (error) => {
          this.messageService.add({
            severity: 'error',
            summary: this.translate.instant('common.error'),
            detail: error.error?.error || this.translate.instant('common.updateFailed')
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
    return this.translate.instant(this.mode === 'create' ? 'assets.createAsset' : 'assets.editAsset');
  }
}
