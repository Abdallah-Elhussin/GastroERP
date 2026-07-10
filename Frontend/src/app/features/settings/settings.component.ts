import { Component, ChangeDetectionStrategy, inject, OnInit, OnDestroy, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { AppButtonComponent } from '../../shared/ui/app-button/app-button.component';
import { AppCardComponent } from '../../shared/ui/app-card/app-card.component';
import { FormDraftManager } from '../../core/utils/form-draft-manager';
import { LanguageService } from '../../core/services/language.service';
import { SettingsRepository } from '../../core/repositories/settings.repository';

@Component({
  selector: 'app-settings',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatIconModule,
    AppButtonComponent,
    AppCardComponent
  ],
  templateUrl: './settings.component.html',
  styleUrl: './settings.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SettingsComponent implements OnInit, OnDestroy {
  fb = inject(FormBuilder);
  langService = inject(LanguageService);
  settingsRepo = inject(SettingsRepository);

  settingsForm: FormGroup = this.fb.group({
    companyName: ['Gastro Hospitality Group', Validators.required],
    taxRate: [15, [Validators.required, Validators.min(0)]],
    checkoutWorkflow: ['Redirect to POS', Validators.required],
    securityTimeout: [30, [Validators.required, Validators.min(5)]],
    fiscalYear: ['2026 (Active)', Validators.required],
    currency: ['SAR (SR)', Validators.required],
    branchAddress: ['120 Olaya District, Riyadh, KSA', Validators.required],
    taxRegistrationNumber: ['310294817200003', Validators.required]
  });

  draftManager!: FormDraftManager;

  ngOnInit(): void {
    this.draftManager = new FormDraftManager(this.settingsForm, 'settings');

    // Fetch live settings
    this.settingsRepo.getSettings().subscribe(data => {
      if (data) {
        this.settingsForm.patchValue(data, { emitEvent: false });
      }
    });

    this.settingsRepo.getRoles().subscribe(rolesData => {
      if (rolesData && rolesData.length > 0) {
        this.roles.set(rolesData);
      }
    });

    this.settingsRepo.getAuditLogs().subscribe(logs => {
      if (logs && logs.length > 0) {
        this.auditLogs.set(logs);
      }
    });
  }

  ngOnDestroy(): void {
    if (this.draftManager) {
      this.draftManager.destroy();
    }
  }

  onSubmit(): void {
    if (this.settingsForm.valid) {
      this.settingsRepo.updateSettings(this.settingsForm.value).subscribe(() => {
        alert(this.t('settings.saved'));
        this.draftManager.clearDraft();
      });
    }
  }

  roles = signal<{
    name: string;
    permissions: { reports: boolean; voids: boolean; settings: boolean; hr: boolean };
  }[]>([
    { name: 'Administrator', permissions: { reports: true, voids: true, settings: true, hr: true } },
    { name: 'Branch Manager', permissions: { reports: true, voids: true, settings: false, hr: true } },
    { name: 'POS Cashier', permissions: { reports: false, voids: false, settings: false, hr: false } },
    { name: 'Kitchen Staff', permissions: { reports: false, voids: false, settings: false, hr: false } }
  ]);

  togglePermission(roleName: string, permissionKey: 'reports' | 'voids' | 'settings' | 'hr'): void {
    this.roles.update(list =>
      list.map(role => {
        if (role.name === roleName) {
          const key = permissionKey;
          const targetState = !role.permissions[key];
          this.settingsRepo.updateRolePermission(roleName, permissionKey, targetState).subscribe();
          return {
            ...role,
            permissions: {
              ...role.permissions,
              [key]: targetState
            }
          };
        }
        return role;
      })
    );
  }

  auditLogs = signal<any[]>([
    { operator: 'Julian Sterling', action: 'Updated General VAT Tax Rate to 15%', ip: '192.168.1.55', time: '10m ago' },
    { operator: 'POS Cashier #1', action: 'Voided Wagyu Burger on Ticket #1084', ip: '192.168.1.102', time: '35m ago' },
    { operator: 'System Daemon', action: 'Triggered low inventory email to supplier', ip: '127.0.0.1', time: '1h ago' }
  ]);

  offlineCacheFlag = signal<boolean>(true);
  kdsPushFlag = signal<boolean>(false);
  smsReceiptsFlag = signal<boolean>(true);

  toggleFlag(flagName: 'offline' | 'kds' | 'sms'): void {
    if (flagName === 'offline') this.offlineCacheFlag.update(v => !v);
    if (flagName === 'kds') this.kdsPushFlag.update(v => !v);
    if (flagName === 'sms') this.smsReceiptsFlag.update(v => !v);
  }

  selectedEmailTemplate = signal<string>('Professional Corporate Receipt');
  selectedSmsTemplate = signal<string>('Standard Order Pick-Up SMS');

  t(key: string): string {
    return this.langService.t(key);
  }
}
