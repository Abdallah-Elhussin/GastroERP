import { Component, ChangeDetectionStrategy, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { ThemeService } from '../../core/services/theme.service';
import { LanguageService } from '../../core/services/language.service';
import { DataService } from '../../core/services/data.service';
import { FullscreenLayoutComponent } from '../../shared/layouts/fullscreen-layout/fullscreen-layout.component';

@Component({
  selector: 'app-setup-wizard',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    ReactiveFormsModule,
    MatButtonModule,
    MatIconModule,
    FullscreenLayoutComponent
  ],
  templateUrl: './setup-wizard.component.html',
  styleUrl: './setup-wizard.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SetupWizardComponent {
  fb = inject(FormBuilder);
  router = inject(Router);
  themeService = inject(ThemeService);
  langService = inject(LanguageService);
  dataService = inject(DataService);

  currentStep = signal<number>(0); // 0: Admin, 1: Venue, 2: Location, 3: Review

  // Stepper Forms
  adminForm: FormGroup = this.fb.group({
    fullName: ['Alexander Hamilton', Validators.required],
    email: ['admin@gastroerp.com', [Validators.required, Validators.email]],
    mobile: ['+1 (555) 123-4567', Validators.required],
    password: ['password123', [Validators.required, Validators.minLength(6)]],
    confirmPassword: ['password123', Validators.required]
  });

  venueForm: FormGroup = this.fb.group({
    legalName: ['Gourmet Hospitality Group LLC', Validators.required],
    industryType: ['Fine Dining', Validators.required],
    branchesCount: [1, [Validators.required, Validators.min(1)]],
    hqAddress: ['100 Premium Plaza, Suite 450, New York, NY 10001', Validators.required]
  });

  locationForm: FormGroup = this.fb.group({
    country: ['United States', Validators.required],
    city: ['New York', Validators.required],
    state: ['NY', Validators.required],
    streetName: ['Broadway', Validators.required],
    bldgApt: ['120', Validators.required],
    postalCode: ['10001', Validators.required]
  });

  get verificationChecks() {
    return [
      { labelKey: 'wizard.check.admin', ok: this.adminForm.valid },
      { labelKey: 'wizard.check.company', ok: this.venueForm.valid },
      { labelKey: 'wizard.check.location', ok: this.locationForm.valid },
      { labelKey: 'wizard.check.integrity', ok: true }
    ];
  }

  nextStep(): void {
    if (this.currentStep() === 0 && this.adminForm.valid) {
      this.currentStep.set(1);
    } else if (this.currentStep() === 1 && this.venueForm.valid) {
      this.currentStep.set(2);
    } else if (this.currentStep() === 2 && this.locationForm.valid) {
      this.currentStep.set(3);
    }
  }

  prevStep(): void {
    if (this.currentStep() > 0) {
      this.currentStep.update(step => step - 1);
    }
  }

  onComplete(): void {
    if (this.adminForm.valid && this.venueForm.valid && this.locationForm.valid) {
      console.log('Complete Setup Wizard Data:', {
        admin: this.adminForm.value,
        venue: this.venueForm.value,
        location: this.locationForm.value
      });

      // Update branding values dynamically based on what was configured in setup!
      this.dataService.updateBranding({
        name: this.venueForm.value.legalName.split(' ')[0] || 'GastroERP'
      });

      // Redirect directly to login/portal
      this.router.navigate(['/login']);
    }
  }

  t(key: string): string {
    return this.langService.t(key);
  }
}
