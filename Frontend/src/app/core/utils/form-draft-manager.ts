import { FormGroup } from '@angular/forms';
import { Subscription } from 'rxjs';
import { debounceTime } from 'rxjs/operators';

export class FormDraftManager {
  private cacheKey: string;
  private changeSub?: Subscription;

  // Undo / Redo history stacks
  private undoStack: any[] = [];
  private redoStack: any[] = [];
  private isApplyingState = false;

  constructor(private form: FormGroup, storageKey: string) {
    this.cacheKey = `gastro-draft-${storageKey}`;

    // Load initial draft if exists
    const draft = localStorage.getItem(this.cacheKey);
    if (draft) {
      try {
        const parsed = JSON.parse(draft);
        this.form.patchValue(parsed, { emitEvent: false });
        this.undoStack.push(parsed);
      } catch (e) {
        console.error('Failed to restore form draft', e);
      }
    } else {
      this.undoStack.push(JSON.parse(JSON.stringify(this.form.value)));
    }

    // Subscribe to form value changes (debounced by 300ms for performance)
    this.changeSub = this.form.valueChanges
      .pipe(debounceTime(300))
      .subscribe(val => {
        if (this.isApplyingState) return;

        // Save state to localStorage (autosave)
        localStorage.setItem(this.cacheKey, JSON.stringify(val));

        // Push to undo stack, clear redo
        this.undoStack.push(JSON.parse(JSON.stringify(val)));
        if (this.undoStack.length > 50) {
          this.undoStack.shift(); // cap history limit to 50 edits
        }
        this.redoStack = [];
      });
  }

  undo(): void {
    if (this.undoStack.length <= 1) return; // Keep at least the initial state

    // Pop current state and move to redo stack
    const current = this.undoStack.pop();
    this.redoStack.push(current);

    // Apply previous state
    const previous = this.undoStack[this.undoStack.length - 1];
    this.applyState(previous);
  }

  redo(): void {
    if (this.redoStack.length === 0) return;

    const next = this.redoStack.pop();
    this.undoStack.push(next);
    this.applyState(next);
  }

  canUndo(): boolean {
    return this.undoStack.length > 1;
  }

  canRedo(): boolean {
    return this.redoStack.length > 0;
  }

  clearDraft(): void {
    localStorage.removeItem(this.cacheKey);
    this.undoStack = [JSON.parse(JSON.stringify(this.form.value))];
    this.redoStack = [];
  }

  destroy(): void {
    if (this.changeSub) {
      this.changeSub.unsubscribe();
    }
  }

  private applyState(state: any): void {
    this.isApplyingState = true;
    this.form.patchValue(state, { emitEvent: true });
    localStorage.setItem(this.cacheKey, JSON.stringify(state));
    this.isApplyingState = false;
  }
}
