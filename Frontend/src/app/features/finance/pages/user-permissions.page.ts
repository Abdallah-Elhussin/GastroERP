import { ChangeDetectionStrategy, Component, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { LanguageService } from '../../../core/services/language.service';
import { AuthService } from '../../../core/services/auth.service';
import { RolePermissionsRepository } from '../../../core/repositories/role-permissions.repository';
import { SystemUserRepository } from '../../../core/repositories/system-user.repository';
import { UserPermissionsRepository, UserPermissionsStateDto } from '../../../core/repositories/user-permissions.repository';
import { MATRIX_ACTIONS, MatrixAction, PermissionDto } from '../../../core/models/role-permissions.models';
import { BranchLookup, SystemUser } from '../../../core/models/system-user.models';

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
  selector: 'app-user-permissions-page',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, MatIconModule],
  templateUrl: './user-permissions.page.html',
  styleUrl: './user-permissions.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class UserPermissionsPage implements OnInit {
  private repo = inject(UserPermissionsRepository);
  private roleRepo = inject(RolePermissionsRepository);
  private userRepo = inject(SystemUserRepository);
  lang = inject(LanguageService);
  auth = inject(AuthService);

  loading = signal(false);
  saving = signal(false);
  error = signal<string | null>(null);
  success = signal<string | null>(null);

  users = signal<SystemUser[]>([]);
  branches = signal<BranchLookup[]>([]);
  permissions = signal<PermissionDto[]>([]);

  selectedUserId = signal<string | null>(null);
  filterBranchId = signal('');
  matrixSearch = signal('');

  state = signal<UserPermissionsStateDto | null>(null);
  selectedIds = signal<Set<string>>(new Set());
  dirty = signal(false);

  actions = MATRIX_ACTIONS;

  canManagePermissions = computed(
    () =>
      this.auth.hasPermission('Identity.Permissions.Manage') ||
      this.auth.hasPermission('Settings.Users.Edit') ||
      this.auth.hasPermission('Administrator')
  );

  filteredUsers = computed(() => {
    const branchId = this.filterBranchId();
    const list = this.users();
    if (!branchId) return list;
    return list.filter(u => u.branchId === branchId);
  });

  selectedUser = computed(() => this.users().find(u => u.id === this.selectedUserId()) ?? null);

  roleNamesLabel = computed(() => {
    const state = this.state();
    if (!state || state.roleNames.length === 0) return '—';
    return state.roleNames.join('، ');
  });

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
    this.loading.set(true);
    this.error.set(null);
    this.userRepo.getList({ pageSize: 200 }).subscribe({
      next: rows => this.users.set(rows),
      error: () => this.users.set([])
    });
    this.userRepo.getBranches().subscribe({
      next: rows => this.branches.set(rows),
      error: () => this.branches.set([])
    });
    this.roleRepo.getPermissions().subscribe({
      next: perms => {
        this.permissions.set(perms);
        this.loading.set(false);
      },
      error: err => {
        this.error.set(err?.error?.error ?? this.t('auth.loadFailed'));
        this.loading.set(false);
      }
    });
  }

  t(key: string): string {
    return this.lang.t(key);
  }

  userLabel(u: SystemUser): string {
    return u.fullName ? `${u.fullName} (${u.userName})` : u.userName;
  }

  onBranchChange(branchId: string): void {
    this.filterBranchId.set(branchId);
    const current = this.selectedUserId();
    if (current && !this.filteredUsers().some(u => u.id === current)) {
      this.selectedUserId.set(null);
      this.state.set(null);
      this.selectedIds.set(new Set());
      this.dirty.set(false);
    }
  }

  onUserChange(userId: string): void {
    if (!userId) return;
    if (this.dirty() && !confirm(this.t('auth.confirmDiscard'))) return;
    this.selectedUserId.set(userId);
    this.loadState(userId);
  }

  isChecked(permissionId: string | null): boolean {
    if (!permissionId) return false;
    return this.selectedIds().has(permissionId);
  }

  isInherited(permissionId: string | null): boolean {
    if (!permissionId) return false;
    return this.state()?.rolePermissionIds.includes(permissionId) ?? false;
  }

  toggleCell(permissionId: string | null): void {
    if (!permissionId || !this.canManagePermissions() || !this.selectedUserId()) return;
    const next = new Set(this.selectedIds());
    if (next.has(permissionId)) next.delete(permissionId);
    else next.add(permissionId);
    this.selectedIds.set(next);
    this.dirty.set(true);
    this.success.set(null);
  }

  fullPermissions(): void {
    if (!this.canManagePermissions() || !this.selectedUserId()) return;
    this.selectedIds.set(new Set(this.permissions().map(p => p.id)));
    this.dirty.set(true);
    this.success.set(null);
  }

  clearAll(): void {
    if (!this.canManagePermissions() || !this.selectedUserId()) return;
    this.selectedIds.set(new Set());
    this.dirty.set(true);
    this.success.set(null);
  }

  savePermissions(): void {
    const userId = this.selectedUserId();
    if (!userId || !this.canManagePermissions()) return;
    this.saving.set(true);
    this.error.set(null);
    this.repo.replaceUserPermissions(userId, [...this.selectedIds()]).subscribe({
      next: () => {
        this.saving.set(false);
        this.dirty.set(false);
        this.success.set(this.t('auth.permissionsSaved'));
        this.loadState(userId, true);
      },
      error: err => {
        this.error.set(err?.error?.error ?? this.t('auth.saveFailed'));
        this.saving.set(false);
      }
    });
  }

  initFromRole(): void {
    const userId = this.selectedUserId();
    if (!userId || !this.canManagePermissions()) return;
    if (!confirm(this.t('auth.initFromRoleConfirm'))) return;
    this.saving.set(true);
    this.error.set(null);
    this.repo.clearUserPermissionOverrides(userId).subscribe({
      next: () => {
        this.saving.set(false);
        this.success.set(this.t('auth.initFromRoleDone'));
        this.loadState(userId, true);
      },
      error: err => {
        this.error.set(err?.error?.error ?? this.t('auth.saveFailed'));
        this.saving.set(false);
      }
    });
  }

  private loadState(userId: string, silent = false): void {
    if (!silent) {
      this.loading.set(true);
      this.error.set(null);
      this.success.set(null);
    }
    this.repo.getUserPermissionsState(userId).subscribe({
      next: state => {
        this.state.set(state);
        this.selectedIds.set(new Set(state.effectivePermissionIds));
        this.dirty.set(false);
        this.loading.set(false);
      },
      error: err => {
        this.error.set(err?.error?.error ?? this.t('auth.loadFailed'));
        this.state.set(null);
        this.selectedIds.set(new Set());
        this.loading.set(false);
      }
    });
  }
}
