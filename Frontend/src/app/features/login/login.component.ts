import { Component, ChangeDetectionStrategy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { ThemeService } from '../../core/services/theme.service';
import { LanguageService } from '../../core/services/language.service';
import { DataService } from '../../core/services/data.service';
import { AuthService } from '../../core/services/auth.service';
import { AuthLayoutComponent } from '../../shared/layouts/auth-layout/auth-layout.component';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    ReactiveFormsModule,
    MatButtonModule,
    MatIconModule,
    MatCheckboxModule,
    AuthLayoutComponent
  ],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class LoginComponent {
  fb = inject(FormBuilder);
  router = inject(Router);
  themeService = inject(ThemeService);
  langService = inject(LanguageService);
  dataService = inject(DataService);
  authService = inject(AuthService);

  loginForm: FormGroup = this.fb.group({
    email: ['admin@gastroerp.com', [Validators.required, Validators.email]],
    password: ['admin', Validators.required],
    rememberMe: [true]
  });

  onSubmit(): void {
    if (this.loginForm.valid) {
      const { email, password } = this.loginForm.value;
      this.authService.login(email, password).subscribe((result) => {
        if (result.success) {
          this.router.navigate(['/dashboard']);
        } else {
          alert(result.error ?? this.t('login.invalidCredentials'));
        }
      });
    } else {
      this.loginForm.markAllAsTouched();
    }
  }

  t(key: string): string {
    return this.langService.t(key);
  }
}
