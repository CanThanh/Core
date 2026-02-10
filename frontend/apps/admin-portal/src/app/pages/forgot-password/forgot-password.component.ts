import { Component, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { CardModule } from 'primeng/card';
import { InputTextModule } from 'primeng/inputtext';
import { ButtonModule } from 'primeng/button';
import { MessageModule } from 'primeng/message';
import { AuthApiService } from '@qlts/api-client';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink,
    CardModule,
    InputTextModule,
    ButtonModule,
    MessageModule,
    TranslateModule
  ],
  templateUrl: './forgot-password.component.html',
  styleUrl: './forgot-password.component.css'
})
export class ForgotPasswordComponent {
  forgotPasswordForm: FormGroup;
  loading = signal(false);
  errorMessage = signal<string | null>(null);
  successMessage = signal<string | null>(null);
  resetToken = signal<string | null>(null);

  constructor(
    private fb: FormBuilder,
    private authApiService: AuthApiService,
    private router: Router,
    private translate: TranslateService
  ) {
    this.forgotPasswordForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]]
    });
  }

  onSubmit(): void {
    if (this.forgotPasswordForm.invalid) return;

    this.loading.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);
    this.resetToken.set(null);

    const email = this.forgotPasswordForm.value.email;

    this.authApiService.forgotPassword(email).subscribe({
      next: (response) => {
        this.loading.set(false);
        this.successMessage.set(response.message);

        // In demo mode, show the reset token
        if (response.resetToken) {
          this.resetToken.set(response.resetToken);
        }
      },
      error: (error) => {
        this.loading.set(false);
        this.errorMessage.set(error.error?.error || this.translate.instant('auth.resetEmailFailed'));
      }
    });
  }

  goToResetPassword(): void {
    const email = this.forgotPasswordForm.value.email;
    const token = this.resetToken();

    this.router.navigate(['/reset-password'], {
      queryParams: { email, token }
    });
  }
}
