export type GoodsIssueStatus = 'Draft' | 'Approved' | 'Posted' | 'Cancelled' | string;

export interface GoodsIssueLine {
  id?: string;
  inventoryItemId: string;
  itemNameAr?: string | null;
  itemSku?: string | null;
  unitId: string;
  unitNameAr?: string | null;
  warehouseId: string;
  warehouseNameAr?: string | null;
  quantity: number;
  unitCost: number;
  totalCost?: number;
  costCenterId?: string | null;
  costCenterNameAr?: string | null;
  notes?: string | null;
}

export interface GoodsIssueDoc {
  id: string;
  tenantId: string;
  warehouseId?: string | null;
  warehouseNameAr?: string | null;
  issueDestinationId?: string | null;
  issueDestinationNameAr?: string | null;
  issueNumber: string;
  issueDate: string;
  approvalDate?: string | null;
  currency: string;
  notes?: string | null;
  status: GoodsIssueStatus;
  statusCode: number;
  isConfirmed: boolean;
  isCompleted: boolean;
  lineCount: number;
  totalAmount: number;
  lines: GoodsIssueLine[];
  createdAt: string;
}

export interface GoodsIssueLineInput {
  inventoryItemId: string;
  unitId: string;
  quantity: number;
  unitCost?: number;
  warehouseId?: string | null;
  costCenterId?: string | null;
  notes?: string | null;
}

export interface CreateGoodsIssuePayload {
  issueNumber?: string | null;
  autoGenerateNumber?: boolean;
  issueDate?: string | null;
  warehouseId?: string | null;
  issueDestinationId?: string | null;
  currency?: string;
  notes?: string | null;
  lines?: GoodsIssueLineInput[];
}

export interface UpdateGoodsIssuePayload {
  issueDate: string;
  warehouseId?: string | null;
  issueDestinationId?: string | null;
  currency?: string;
  notes?: string | null;
  lines?: GoodsIssueLineInput[];
}

export interface IssueDestination {
  id: string;
  tenantId: string;
  code: string;
  nameAr: string;
  nameEn?: string | null;
  description?: string | null;
  destinationType?: string;
  destinationTypeCode?: number;
  defaultGlAccountId?: string | null;
  glAccountNameAr?: string | null;
  defaultCostCenterId?: string | null;
  costCenterNameAr?: string | null;
  allowChangeAccountOnIssue?: boolean;
  requireEmployee?: boolean;
  requireProject?: boolean;
  requireCostCenter?: boolean;
  requireBranch?: boolean;
  requireReason?: boolean;
  requireApproval?: boolean;
  allowDirectIssue?: boolean;
  allowNegativeStock?: boolean;
  policySummary?: string;
  sortOrder: number;
  isSystem: boolean;
  isActive: boolean;
}

export interface UpsertIssueDestinationPayload {
  code?: string;
  nameAr: string;
  nameEn?: string | null;
  description?: string | null;
  defaultCostCenterId?: string | null;
  sortOrder?: number;
  isActive?: boolean;
}

export interface GoodsIssueListParams {
  page?: number;
  pageSize?: number;
  search?: string;
  status?: number | null;
  from?: string | null;
  to?: string | null;
}
