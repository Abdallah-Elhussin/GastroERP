import {
  ChangeDetectionStrategy,
  Component,
  OnInit,
  computed,
  inject,
  signal
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { LanguageService } from '../../../core/services/language.service';
import { AuthService } from '../../../core/services/auth.service';
import { RolePermissionsRepository } from '../../../core/repositories/role-permissions.repository';
import {
  MATRIX_ACTIONS,
  MatrixAction,
  PermissionDto,
  RoleDto
} from '../../../core/models/role-permissions.models';

interface MatrixCell {
  action: MatrixAction;
  permissionId: string | null;
}

interface MatrixRow {
  module: string;
  label: string;
  cells: MatrixCell[];
}

@Component({
  selector: 'app-roles-permissions-page',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, MatIconModule],
  templateUrl: './roles-permissions.page.html',
  styleUrl: './roles-permissions.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class RolesPermissionsPage implements OnInit {
  private repo = inject(RolePermissionsRepository);
  lang = inject(LanguageService);
  auth = inject(AuthService);

  loading = signal(false);
  saving = signal(false);
  error = signal<string | null>(null);
  success = signal<string | null>(null);

  roles = signal<RoleDto[]>([]);
  permissions = signal<PermissionDto[]>([]);
  selectedIds = signal<Set<string>>(new Set());
  selectedRoleId = signal<string | null>(null);
  roleSearch = signal('');
  matrixSearch = signal('');
  dirty = signal(false);

  showRoleModal = signal(false);
  editingRoleId = signal<string | null>(null);
  formName = signal('');
  formNameAr = signal('');
  formDescription = signal('');

  actions = MATRIX_ACTIONS;

  canManageRoles = computed(
    () =>
      this.auth.hasPermission('Identity.Roles.Manage') ||
      this.auth.hasPermission('Settings.Users.Edit') ||
      this.auth.hasPermission('Administrator')
  );
  canManagePermissions = computed(
    () =>
      this.auth.hasPermission('Identity.Permissions.Manage') ||
      this.auth.hasPermission('Settings.Users.Edit') ||
      this.auth.hasPermission('Administrator')
  );

  filteredRoles = computed(() => {
    const q = this.roleSearch().trim().toLowerCase();
    const list = this.roles();
    if (!q) return list;
    return list.filter(
      r =>
        r.name.toLowerCase().includes(q) ||
        (r.nameAr || '').toLowerCase().includes(q) ||
        (r.description || '').toLowerCase().includes(q)
    );
  });

  selectedRole = computed(() => this.roles().find(r => r.id === this.selectedRoleId()) ?? null);

  matrixRows = computed(() => {
    const perms = this.permissions();
    const q = this.matrixSearch().trim().toLowerCase();
    const byModule = new Map<string, PermissionDto[]>();
    for (const p of perms) {
      const key = p.module || p.name.split('.')[0] || 'Other';
      if (!byModule.has(key)) byModule.set(key, []);
      byModule.get(key)!.push(p);
    }

    const rows: MatrixRow[] = [];
    for (const [module, list] of [...byModule.entries()].sort((a, b) => a[0].localeCompare(b[0]))) {
      if (q && !module.toLowerCase().includes(q) && !list.some(p => p.name.toLowerCase().includes(q))) {
        continue;
      }
      const cells: MatrixCell[] = this.actions.map(a => {
        const hit =
          list.find(p => {
            const n = p.name;
            return a.suffixes.some(suf => {
              const s = suf.startsWith('.') ? suf.slice(1) : suf;
              return n === s || n.endsWith('.' + s);
            });
          }) ?? null;
        return { action: a.key, permissionId: hit?.id ?? null };
      });
      rows.push({ module, label: module, cells });
    }
    return rows;
  });

  selectedCount = computed(() => this.selectedIds().size);

  ngOnInit(): void {
    this.reload();
  }

  t(key: string): string {
    return this.lang.t(key);
  }

  roleLabel(r: RoleDto): string {
    return this.lang.language() === 'ar' && r.nameAr ? r.nameAr : r.name;
  }

  isChecked(permissionId: string | null): boolean {
    if (!permissionId) return false;
    return this.selectedIds().has(permissionId);
  }

  toggleCell(permissionId: string | null): void {
    if (!permissionId || !this.canManagePermissions() || !this.selectedRoleId()) return;
    const next = new Set(this.selectedIds());
    if (next.has(permissionId)) next.delete(permissionId);
    else next.add(permissionId);
    this.selectedIds.set(next);
    this.dirty.set(true);
    this.success.set(null);
  }

  selectRole(role: RoleDto): void {
    if (this.dirty() && !confirm(this.t('auth.confirmDiscard'))) return;
    this.selectedRoleId.set(role.id);
    this.dirty.set(false);
    this.error.set(null);
    this.success.set(null);
    this.loading.set(true);
    this.repo.getRolePermissionIds(role.id).subscribe({
      next: ids => {
        this.selectedIds.set(new Set(ids));
        this.loading.set(false);
      },
      error: err => {
        this.error.set(err?.error?.error ?? this.t('auth.loadFailed'));
        this.selectedIds.set(new Set());
        this.loading.set(false);
      }
    });
  }

  openCreate(): void {
    this.editingRoleId.set(null);
    this.formName.set('');
    this.formNameAr.set('');
    this.formDescription.set('');
    this.showRoleModal.set(true);
  }

  openEdit(): void {
    const role = this.selectedRole();
    if (!role) return;
    this.editingRoleId.set(role.id);
    this.formName.set(role.name);
    this.formNameAr.set(role.nameAr || '');
    this.formDescription.set(role.description || '');
    this.showRoleModal.set(true);
  }

  closeModal(): void {
    this.showRoleModal.set(false);
  }

  saveRole(): void {
    const name = this.formName().trim();
    if (!name) {
      this.error.set(this.t('auth.validation.name'));
      return;
    }
    const payload = {
      name,
      nameAr: this.formNameAr().trim() || null,
      description: this.formDescription().trim() || null
    };
    this.saving.set(true);
    this.error.set(null);
    const id = this.editingRoleId();
    const req = id ? this.repo.updateRole(id, payload) : this.repo.createRole(payload);
    req.subscribe({
      next: createdId => {
        this.saving.set(false);
        this.showRoleModal.set(false);
        this.success.set(this.t('auth.saved'));
        this.reload(typeof createdId === 'string' ? createdId : id);
      },
      error: err => {
        this.error.set(err?.error?.error ?? this.t('auth.saveFailed'));
        this.saving.set(false);
      }
    });
  }

  copySelected(): void {
    const role = this.selectedRole();
    if (!role || !this.canManageRoles()) return;
    this.saving.set(true);
    this.repo.copyRole(role.id).subscribe({
      next: newId => {
        this.saving.set(false);
        this.success.set(this.t('auth.copied'));
        this.reload(newId);
      },
      error: err => {
        this.error.set(err?.error?.error ?? this.t('auth.copyFailed'));
        this.saving.set(false);
      }
    });
  }

  grantAll(): void {
    if (!this.canManagePermissions()) return;
    this.selectedIds.set(new Set(this.permissions().map(p => p.id)));
    this.dirty.set(true);
  }

  grantViewOnly(): void {
    if (!this.canManagePermissions()) return;
    const viewIds = this.permissions()
      .filter(p => p.name.endsWith('.View') || p.name.includes('.View.'))
      .map(p => p.id);
    this.selectedIds.set(new Set(viewIds));
    this.dirty.set(true);
  }

  clearAll(): void {
    if (!this.canManagePermissions()) return;
    this.selectedIds.set(new Set());
    this.dirty.set(true);
  }

  savePermissions(): void {
    const roleId = this.selectedRoleId();
    if (!roleId || !this.canManagePermissions()) return;
    this.saving.set(true);
    this.error.set(null);
    this.repo.replaceRolePermissions(roleId, [...this.selectedIds()]).subscribe({
      next: () => {
        this.saving.set(false);
        this.dirty.set(false);
        this.success.set(this.t('auth.permissionsSaved'));
      },
      error: err => {
        this.error.set(err?.error?.error ?? this.t('auth.saveFailed'));
        this.saving.set(false);
      }
    });
  }

  private reload(selectId?: string | null): void {
    this.loading.set(true);
    this.error.set(null);
    this.repo.getRoles().subscribe({
      next: roles => {
        this.roles.set(roles);
        this.repo.getPermissions().subscribe({
          next: perms => {
            this.permissions.set(perms);
            this.loading.set(false);
            const prefer = selectId || this.selectedRoleId();
            const target = roles.find(r => r.id === prefer) ?? roles[0];
            if (target) this.selectRole(target);
          },
          error: err => {
            this.error.set(err?.error?.error ?? this.t('auth.loadFailed'));
            this.loading.set(false);
          }
        });
      },
      error: err => {
        this.error.set(err?.error?.error ?? this.t('auth.loadFailed'));
        this.loading.set(false);
      }
    });
  }
}
