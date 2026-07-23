import { AccountTreeNode, ChartAccount } from '../../../core/models/chart-of-account.models';

export function flattenTreeAccounts(nodes: AccountTreeNode[]): ChartAccount[] {
  const out: ChartAccount[] = [];
  const walk = (list: AccountTreeNode[]) => {
    for (const n of list) {
      out.push(n);
      if (n.children?.length) walk(n.children);
    }
  };
  walk(nodes);
  return out;
}

/** Collect an account node and all descendants by id (empty if root not found). */
export function collectSubtreeIds(nodes: AccountTreeNode[], rootId: string | null | undefined): Set<string> {
  const ids = new Set<string>();
  if (!rootId) return ids;

  const find = (list: AccountTreeNode[]): AccountTreeNode | null => {
    for (const n of list) {
      if (n.id === rootId) return n;
      const found = find(n.children ?? []);
      if (found) return found;
    }
    return null;
  };

  const root = find(nodes);
  if (!root) return ids;

  const walk = (n: AccountTreeNode) => {
    ids.add(n.id);
    for (const c of n.children ?? []) walk(c);
  };
  walk(root);
  return ids;
}
