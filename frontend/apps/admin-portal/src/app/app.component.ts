import { Component, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet],
  template: '<router-outlet />'
})
export class AppComponent implements OnInit {
  title = 'Asset Management System';

  constructor(private translate: TranslateService) {
    // Set default language
    const savedLang = localStorage.getItem('language') || 'vi';
    this.translate.setDefaultLang('vi');
    this.translate.use(savedLang);
  }

  ngOnInit(): void {
    console.log('Current language:', this.translate.currentLang);
    console.log('Default language:', this.translate.defaultLang);
  }
}
