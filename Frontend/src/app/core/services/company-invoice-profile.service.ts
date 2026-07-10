import { Injectable, inject } from '@angular/core';
import { DataService } from './data.service';

export interface CompanyInvoiceProfile {
  nameAr: string;
  nameEn: string;
  addressAr: string;
  addressEn: string;
  vatNumber: string;
  crNumber: string;
  phone: string;
  logoUrl: string;
}

const PROFILE_KEY = 'gastro_company_invoice_profile';
const SETTINGS_DRAFT_KEY = 'gastro-draft-settings';

@Injectable({ providedIn: 'root' })
export class CompanyInvoiceProfileService {
  private dataService = inject(DataService);

  getProfile(): CompanyInvoiceProfile {
    const branding = this.dataService.branding();
    const settings = this.readSettingsDraft();
    const stored = this.readStoredProfile();

    const nameEn = stored?.nameEn ?? settings?.['companyName'] ?? branding.name ?? 'GastroERP';
    const addressEn = stored?.addressEn ?? settings?.['branchAddress'] ?? '120 Olaya District, Riyadh, KSA';
    const vatNumber = stored?.vatNumber ?? settings?.['taxRegistrationNumber'] ?? '310294817200003';

    return {
      nameAr: stored?.nameAr ?? 'مجموعة جاسترو للضيافة',
      nameEn,
      addressAr: stored?.addressAr ?? 'حي العليا، الرياض، المملكة العربية السعودية',
      addressEn,
      vatNumber,
      crNumber: stored?.crNumber ?? '1010456789',
      phone: stored?.phone ?? '+966 11 234 5678',
      logoUrl: stored?.logoUrl ?? branding.logoUrl
    };
  }

  private readSettingsDraft(): Record<string, string> | null {
    try {
      const raw = localStorage.getItem(SETTINGS_DRAFT_KEY);
      return raw ? JSON.parse(raw) : null;
    } catch {
      return null;
    }
  }

  private readStoredProfile(): Partial<CompanyInvoiceProfile> | null {
    try {
      const raw = localStorage.getItem(PROFILE_KEY);
      return raw ? JSON.parse(raw) : null;
    } catch {
      return null;
    }
  }
}
