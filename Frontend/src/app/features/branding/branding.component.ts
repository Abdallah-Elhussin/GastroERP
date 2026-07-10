import { Component, ChangeDetectionStrategy, inject, signal, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { Subscription } from 'rxjs';
import { BrandingRepository } from '../../core/repositories/branding.repository';
import { LanguageService } from '../../core/services/language.service';
import { BrandingConfig } from '../../core/models/erp.models';
import { MediaPickerComponent } from '../../shared/components/media-picker/media-picker.component';
import { MediaFile } from '../../core/repositories/media.repository';

@Component({
  selector: 'app-branding',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatButtonModule,
    MatIconModule,
    MatDialogModule
  ],
  templateUrl: './branding.component.html',
  styleUrl: './branding.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class BrandingComponent implements OnInit, OnDestroy {
  brandingRepository = inject(BrandingRepository);
  langService = inject(LanguageService);
  fb = inject(FormBuilder);
  dialog = inject(MatDialog);

  isSaving = signal<boolean>(false);
  activeConfig = signal<BrandingConfig | null>(null);

  brandingForm!: FormGroup;
  private changeSub?: Subscription;

  ngOnInit(): void {
    // Retrieve configuration from Repository layer
    this.brandingRepository.getBrandingConfig().subscribe(config => {
      this.activeConfig.set(config);
      
      this.brandingForm = this.fb.group({
        name: [config.name, Validators.required],
        position: [config.position, Validators.required],
        aspectRatio: [config.aspectRatio, Validators.required],
        clickAction: [config.clickAction, Validators.required],
        primaryColor: [config.primaryColor || '#845ec2', Validators.required],
        accentColor: [config.accentColor || '#ff9671', Validators.required],
        fontFamily: [config.fontFamily || 'Inter', Validators.required]
      });

      // Apply initial styling parameters
      this.applyStyles(config);

      // Subscribe to changes for real-time live preview (without refresh)
      this.changeSub = this.brandingForm.valueChanges.subscribe(val => {
        this.applyStyles(val);
      });
    });
  }

  ngOnDestroy(): void {
    if (this.changeSub) {
      this.changeSub.unsubscribe();
    }
  }

  onSubmit(): void {
    if (this.brandingForm.valid) {
      this.isSaving.set(true);
      this.brandingRepository.updateBrandingConfig(this.brandingForm.value).subscribe(updated => {
        this.activeConfig.set(updated);
        this.isSaving.set(false);
        alert(this.t('branding.saved'));
      });
    }
  }

  changeLogo(): void {
    const dialogRef = this.dialog.open(MediaPickerComponent);
    dialogRef.afterClosed().subscribe((selectedFile: MediaFile) => {
      if (selectedFile) {
        this.brandingRepository.updateBrandingConfig({ logoUrl: selectedFile.url }).subscribe(updated => {
          this.activeConfig.set(updated);
        });
      }
    });
  }

  changeBackground(): void {
    const dialogRef = this.dialog.open(MediaPickerComponent);
    dialogRef.afterClosed().subscribe((selectedFile: MediaFile) => {
      if (selectedFile) {
        this.brandingRepository.updateBrandingConfig({ loginBgUrl: selectedFile.url }).subscribe(updated => {
          this.activeConfig.set(updated);
        });
      }
    });
  }

  private applyStyles(config: Partial<BrandingConfig>): void {
    if (config.primaryColor) {
      document.documentElement.style.setProperty('--primary-color', config.primaryColor);
    }
    if (config.accentColor) {
      document.documentElement.style.setProperty('--accent-color', config.accentColor);
    }
    if (config.fontFamily) {
      document.documentElement.style.setProperty('--font-family', config.fontFamily);
    }
  }

  t(key: string): string {
    return this.langService.t(key);
  }
}
