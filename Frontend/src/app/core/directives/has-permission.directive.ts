import { Directive, Input, TemplateRef, ViewContainerRef, inject, effect } from '@angular/core';
import { AuthService } from '../services/auth.service';

@Directive({
  selector: '[appHasPermission]',
  standalone: true
})
export class HasPermissionDirective {
  private templateRef = inject(TemplateRef);
  private viewContainer = inject(ViewContainerRef);
  private authService = inject(AuthService);

  private permission = '';
  private hasView = false;

  @Input() set appHasPermission(val: string) {
    this.permission = val;
    this.updateView();
  }

  constructor() {
    // Re-evaluate when permissions update
    effect(() => {
      // Access signal to register dependency
      this.authService.userPermissions();
      this.updateView();
    });
  }

  private updateView(): void {
    const userPerms = this.authService.userPermissions();
    const hasPerm = userPerms.includes('ALL') || userPerms.includes(this.permission);

    if (hasPerm && !this.hasView) {
      this.viewContainer.createEmbeddedView(this.templateRef);
      this.hasView = true;
    } else if (!hasPerm && this.hasView) {
      this.viewContainer.clear();
      this.hasView = false;
    }
  }
}
