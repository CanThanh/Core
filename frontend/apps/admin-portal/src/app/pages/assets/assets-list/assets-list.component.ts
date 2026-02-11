import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TableModule } from 'primeng/table';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { SkeletonModule } from 'primeng/skeleton';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ToastModule } from 'primeng/toast';
import { ConfirmationService, MessageService } from 'primeng/api';
import { AssetsApiService } from '@qlts/api-client';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { AssetDialogComponent } from '../asset-dialog/asset-dialog.component';

interface AssetDto {
  id: string;
  code: string;
  name: string;
  categoryName: string;
  purchasePrice: number;
  depreciationRate: number;
  status: string;
  purchaseDate: string;
  location: string | null;
}

interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

@Component({
  selector: 'app-assets-list',
  standalone: true,
  imports: [
    CommonModule,
    TableModule,
    CardModule,
    ButtonModule,
    InputTextModule,
    TagModule,
    TooltipModule,
    SkeletonModule,
    ConfirmDialogModule,
    ToastModule,
    TranslateModule,
    AssetDialogComponent
  ],
  providers: [ConfirmationService, MessageService],
  templateUrl: './assets-list.component.html',
  styleUrl: './assets-list.component.css'
})
export class AssetsListComponent implements OnInit {
  assets = signal<PagedResult<AssetDto>>({
    items: [],
    totalCount: 0,
    pageNumber: 1,
    pageSize: 10,
    totalPages: 0,
    hasPreviousPage: false,
    hasNextPage: false
  });
  loading = signal(false);
  pageSize = signal(10);

  showDialog = false;
  dialogMode: 'create' | 'edit' = 'create';
  selectedAssetId?: string;

  constructor(
    private assetsApiService: AssetsApiService,
    private confirmationService: ConfirmationService,
    private messageService: MessageService,
    private translate: TranslateService
  ) {}

  ngOnInit(): void {
    this.loadAssets();
  }

  loadAssets(event?: any): void {
    const page = event?.first != null && event?.rows ? (event.first / event.rows) + 1 : 1;
    const pageSize = event?.rows || 10;

    this.loading.set(true);
    this.assetsApiService.getAssets(page, pageSize).subscribe({
      next: (data) => {
        this.assets.set(data);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
      }
    });
  }

  openCreateDialog(): void {
    this.dialogMode = 'create';
    this.selectedAssetId = undefined;
    this.showDialog = true;
  }

  openEditDialog(assetId: string): void {
    this.dialogMode = 'edit';
    this.selectedAssetId = assetId;
    this.showDialog = true;
  }

  onAssetSaved(): void {
    this.showDialog = false;
    this.loadAssets();
  }

  deleteAsset(assetId: string, assetName: string): void {
    this.confirmationService.confirm({
      message: this.translate.instant('common.areYouSure'),
      header: this.translate.instant('common.confirmDelete'),
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.assetsApiService.deleteAsset(assetId).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: this.translate.instant('common.success'),
              detail: this.translate.instant('assets.deleted')
            });
            this.loadAssets();
          },
          error: (error) => {
            this.messageService.add({
              severity: 'error',
              summary: this.translate.instant('common.error'),
              detail: error.error?.message || this.translate.instant('common.deleteFailed')
            });
          }
        });
      }
    });
  }

  getStatusLabel(status: string): string {
    const statusKey = `assets.status.${status}`;
    const translated = this.translate.instant(statusKey);
    return translated !== statusKey ? translated : status;
  }

  getStatusSeverity(status: string): string {
    const severities: Record<string, string> = {
      InUse: 'success',
      Maintenance: 'warning',
      Broken: 'danger',
      Disposed: 'info'
    };
    return severities[status] || 'info';
  }
}
