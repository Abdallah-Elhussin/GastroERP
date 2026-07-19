export type NotificationNoteType = 1 | 2;
export type NotificationPartyType = 1 | 2 | 3 | 4 | 5 | 6;

export const NOTIFICATION_NOTE_TYPES: { value: NotificationNoteType; labelKey: string }[] = [
  { value: 1, labelKey: 'fin.reason.note.debit' },
  { value: 2, labelKey: 'fin.reason.note.credit' }
];

export const NOTIFICATION_PARTY_TYPES: { value: NotificationPartyType; labelKey: string }[] = [
  { value: 1, labelKey: 'fin.reason.party.customer' },
  { value: 2, labelKey: 'fin.reason.party.supplier' },
  { value: 3, labelKey: 'fin.reason.party.employee' },
  { value: 4, labelKey: 'fin.reason.party.salesRep' },
  { value: 5, labelKey: 'fin.reason.party.general' },
  { value: 6, labelKey: 'fin.reason.party.other' }
];

/** AccountClassification.code expected for party filtering. */
export const PARTY_CLASSIFICATION_CODE: Partial<Record<NotificationPartyType, string>> = {
  1: 'receivable',
  2: 'payable',
  3: 'salaries_payable'
};

export interface NotificationReason {
  id: string;
  number: number;
  code: string;
  nameAr: string;
  nameEn?: string | null;
  noteType: NotificationNoteType | number;
  partyType: NotificationPartyType | number;
  counterpartAccountId: string;
  accountNumber?: string | null;
  accountNameAr?: string | null;
  usesTax: boolean;
  isActive: boolean;
  hasBeenUsed: boolean;
  createdAt: string;
  createdBy?: string | null;
  updatedAt?: string | null;
  updatedBy?: string | null;
}

export interface UpsertNotificationReasonPayload {
  code: string;
  nameAr: string;
  nameEn?: string | null;
  noteType: number;
  partyType: number;
  counterpartAccountId: string;
  usesTax: boolean;
  isActive: boolean;
}

export interface NotificationReasonListFilter {
  search?: string;
  noteType?: number | null;
  partyType?: number | null;
  isActive?: boolean | null;
  pageSize?: number;
}
