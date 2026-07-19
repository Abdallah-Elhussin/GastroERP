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
