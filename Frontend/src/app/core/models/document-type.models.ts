export const DOCUMENT_MODULES = [
  { value: 1, code: 'Inventory', labelKey: 'fin.doc.module.inventory' },
  { value: 2, code: 'Purchasing', labelKey: 'fin.doc.module.purchasing' },
  { value: 3, code: 'Sales', labelKey: 'fin.doc.module.sales' },
  { value: 4, code: 'Finance', labelKey: 'fin.doc.module.finance' },
  { value: 5, code: 'HR', labelKey: 'fin.doc.module.hr' },
  { value: 6, code: 'Production', labelKey: 'fin.doc.module.production' },
  { value: 7, code: 'Maintenance', labelKey: 'fin.doc.module.maintenance' },
  { value: 8, code: 'POS', labelKey: 'fin.doc.module.pos' },
  { value: 9, code: 'General', labelKey: 'fin.doc.module.general' }
] as const;

export const APPROVAL_MODES = [
  { value: 0, labelKey: 'fin.doc.approval.none' },
  { value: 1, labelKey: 'fin.doc.approval.single' },
  { value: 2, labelKey: 'fin.doc.approval.multi' },
  { value: 3, labelKey: 'fin.doc.approval.amount' },
  { value: 4, labelKey: 'fin.doc.approval.branch' },
  { value: 5, labelKey: 'fin.doc.approval.department' }
] as const;

export const POSTING_MODES = [
  { value: 0, labelKey: 'fin.doc.posting.manual' },
  { value: 1, labelKey: 'fin.doc.posting.auto' },
  { value: 2, labelKey: 'fin.doc.posting.afterApproval' },
  { value: 3, labelKey: 'fin.doc.posting.journalOnly' },
  { value: 4, labelKey: 'fin.doc.posting.stockOnly' },
  { value: 5, labelKey: 'fin.doc.posting.both' }
] as const;

export interface DocumentTypeLifecycleStage {
  code: string;
  nameAr: string;
  nameEn: string;
  sortOrder: number;
  isTerminal: boolean;
}

export interface DocumentType {
  id: string;
  code: string;
  nameAr: string;
  nameEn: string;
  description?: string | null;
  module: number;
  moduleCode: string;
  prefix: string;
  suffix?: string | null;
  startingNumber: number;
  lastNumber: number;
  numberLength: number;
  resetYearly: boolean;
  resetMonthly: boolean;
  numberPerBranch: boolean;
  numberPerCompany: boolean;
  approvalMode: number;
  requiresApproval: boolean;
  usesWorkflow: boolean;
  workflowDefinitionId?: string | null;
  postingMode: number;
  autoPost: boolean;
  postAfterApproval: boolean;
  affectsInventory: boolean;
  affectsCost: boolean;
  affectsAccounting: boolean;
  affectsCash: boolean;
  affectsCustomers: boolean;
  affectsSuppliers: boolean;
  affectsAssets: boolean;
  affectsPayroll: boolean;
  allowCreate: boolean;
  allowUpdate: boolean;
  allowApprove: boolean;
  allowPost: boolean;
  allowCancel: boolean;
  allowDelete: boolean;
  allowAttachments: boolean;
  allowPrint: boolean;
  allowEditAfterSave: boolean;
  allowDeleteDocuments: boolean;
  allowCancelDocuments: boolean;
  allowCopy: boolean;
  allowReopen: boolean;
  showInReports: boolean;
  showInDashboard: boolean;
  isSystem: boolean;
  isActive: boolean;
  sortOrder: number;
  lifecycleStages: DocumentTypeLifecycleStage[];
}

export type UpsertDocumentTypePayload = Omit<
  DocumentType,
  'id' | 'moduleCode' | 'isSystem'
>;
