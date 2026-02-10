import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateService, TranslateModule } from '@ngx-translate/core';
import { DropdownModule } from 'primeng/dropdown';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-language-switcher',
  standalone: true,
  imports: [CommonModule, TranslateModule, DropdownModule, FormsModule],
  template: `
    <p-dropdown
      [(ngModel)]="currentLang"
      [options]="languages"
      (onChange)="switchLanguage($event.value)"
      [style]="{'width': '140px'}"
      placeholder="Language">
    </p-dropdown>
  `,
  styles: []
})
export class LanguageSwitcherComponent {
  currentLang: string;
  languages = [
    { label: 'English', value: 'en' },
    { label: 'Tiếng Việt', value: 'vi' }
  ];

  constructor(private translate: TranslateService) {
    // Get saved language or default to Vietnamese
    const savedLang = localStorage.getItem('language') || 'vi';
    this.currentLang = savedLang;
    this.translate.use(savedLang);
  }

  switchLanguage(lang: string) {
    this.translate.use(lang);
    localStorage.setItem('language', lang);
  }
}
