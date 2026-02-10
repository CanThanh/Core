import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-no-access',
  standalone: true,
  imports: [CommonModule, RouterLink, ButtonModule, CardModule, TranslateModule],
  templateUrl: './no-access.component.html',
  styleUrl: './no-access.component.css'
})
export class NoAccessComponent {
  constructor(private router: Router) {}

  goBack(): void {
    this.router.navigate(['/']);
  }
}
