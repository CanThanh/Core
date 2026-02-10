import { Injectable, signal } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { AuthApiService, LoginRequest, RegisterRequest, UserInfo } from '@asset-management/api-client';
import { TokenService } from './token.service';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  currentUser = signal<UserInfo | null>(null);
  isAuthenticated = signal<boolean>(false);

  constructor(
    private authApiService: AuthApiService,
    private tokenService: TokenService,
    private router: Router
  ) {
    this.loadUserFromStorage();
  }

  login(request: LoginRequest): Observable<any> {
    return this.authApiService.login(request).pipe(
      tap(response => {
        this.tokenService.saveTokens(response.accessToken, response.refreshToken);
        this.tokenService.saveUser(response.user);
        this.currentUser.set(response.user);
        this.isAuthenticated.set(true);
      })
    );
  }

  register(request: RegisterRequest): Observable<any> {
    return this.authApiService.register(request);
  }

  logout(): void {
    this.tokenService.clearTokens();
    this.currentUser.set(null);
    this.isAuthenticated.set(false);
    this.router.navigate(['/login']);
  }

  refreshToken(): Observable<any> {
    const refreshToken = this.tokenService.getRefreshToken();
    if (!refreshToken) {
      this.logout();
      throw new Error('No refresh token available');
    }

    return this.authApiService.refreshToken({ refreshToken }).pipe(
      tap(response => {
        this.tokenService.saveTokens(response.accessToken, response.refreshToken);
      })
    );
  }

  private loadUserFromStorage(): void {
    const user = this.tokenService.getUser();
    if (user) {
      this.currentUser.set(user);
      this.isAuthenticated.set(true);
    }
  }
}
