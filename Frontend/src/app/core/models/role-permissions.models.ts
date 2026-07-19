export interface RoleDto {
  id: string;
  name: string;
  nameAr?: string | null;
  description?: string | null;
  isSystem: boolean;
  isActive: boolean;
}

export interface PermissionDto {
  id: string;
  module: string;
  name: string;
  displayName: string;
}

export interface UpsertRolePayload {
  name: string;
  nameAr?: string | null;
  description?: string | null;
}

export type MatrixAction =
  | 'view'
  | 'create'
  | 'update'
  | 'delete'
  | 'post'
  | 'print'
  | 'approve';

export const MATRIX_ACTIONS: { key: MatrixAction; labelKey: string; suffixes: string[] }[] = [
  { key: 'view', labelKey: 'auth.col.view', suffixes: ['.View', '.Dashboard.View', '.ViewDashboard', '.ViewReports'] },
  { key: 'create', labelKey: 'auth.col.add', suffixes: ['.Create', '.Issue', '.Add'] },
  { key: 'update', labelKey: 'auth.col.edit', suffixes: ['.Update', '.Edit', '.Manage'] },
  { key: 'delete', labelKey: 'auth.col.delete', suffixes: ['.Delete', '.Cancel'] },
  { key: 'post', labelKey: 'auth.col.post', suffixes: ['.Post', '.Complete', '.Close'] },
  { key: 'print', labelKey: 'auth.col.print', suffixes: ['.Print', '.Export'] },
  { key: 'approve', labelKey: 'auth.col.approve', suffixes: ['.Approve'] }
];
