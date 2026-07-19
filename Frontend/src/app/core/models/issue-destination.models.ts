export type IssueDestinationTypeCode =
  | 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 9 | 10 | 11 | 12 | 13 | 99;

export interface IssueDestination {
  id: string;
  tenantId: string;
  code: string;
  nameAr: string;
  nameEn?: string | null;
  description?: string | null;
  destinationType: string;
  destinationTypeCode: IssueDestinationTypeCode | number;
  defaultGlAccountId?: string | null;
  glAccountNameAr?: string | null;
  defaultCostCenterId?: string | null;
  costCenterNameAr?: string | null;
  allowChangeAccountOnIssue: boolean;
  requireEmployee: boolean;
  requireProject: boolean;
  requireCostCenter: boolean;
  requireBranch: boolean;
  requireReason: boolean;
  requireApproval: boolean;
  allowDirectIssue: boolean;
  allowNegativeStock: boolean;
  workflowDefinitionId?: string | null;
  policySummary: string;
  sortOrder: number;
  isSystem: boolean;
  isActive: boolean;
}

export interface UpsertIssueDestinationPayload {
  code?: string;
  nameAr: string;
  nameEn?: string | null;
  description?: string | null;
  destinationType: number;
  defaultGlAccountId?: string | null;
  defaultCostCenterId?: string | null;
  allowChangeAccountOnIssue?: boolean;
  requireEmployee?: boolean;
  requireProject?: boolean;
  requireCostCenter?: boolean;
  requireBranch?: boolean;
  requireReason?: boolean;
  requireApproval?: boolean;
  allowDirectIssue?: boolean;
  allowNegativeStock?: boolean;
  workflowDefinitionId?: string | null;
  sortOrder?: number;
  isActive?: boolean;
}

export const ISSUE_DESTINATION_TYPES: { value: number; labelKey: string }[] = [
  { value: 1, labelKey: 'inv.dest.type.kitchen' },
  { value: 2, labelKey: 'inv.dest.type.production' },
  { value: 3, labelKey: 'inv.dest.type.branch' },
  { value: 4, labelKey: 'inv.dest.type.administration' },
  { value: 5, labelKey: 'inv.dest.type.marketing' },
  { value: 6, labelKey: 'inv.dest.type.maintenance' },
  { value: 7, labelKey: 'inv.dest.type.waste' },
  { value: 8, labelKey: 'inv.dest.type.staffMeals' },
  { value: 9, labelKey: 'inv.dest.type.complimentary' },
  { value: 10, labelKey: 'inv.dest.type.assets' },
  { value: 11, labelKey: 'inv.dest.type.project' },
  { value: 12, labelKey: 'inv.dest.type.costCenter' },
  { value: 13, labelKey: 'inv.dest.type.expense' },
  { value: 99, labelKey: 'inv.dest.type.other' }
];
