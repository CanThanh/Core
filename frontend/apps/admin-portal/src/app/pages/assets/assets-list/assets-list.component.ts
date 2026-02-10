import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { TableModule } from 'primeng/table';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { SkeletonModule } from 'primeng/skeleton';
import { AssetsApiService } from '@qlts/api-client';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

interface AssetDto {
  assetCode: string;
  name: string;
  categoryName: string;
  categoryType: string;
  purchasePrice: number;
  currentValue: number;
  status: string;
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
    RouterLink,
    TableModule,
    CardModule,
    ButtonModule,
    InputTextModule,
    TagModule,
    TooltipModule,
    SkeletonModule,
    TranslateModule
  ],
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

  constructor(
    private assetsApiService: AssetsApiService,
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
