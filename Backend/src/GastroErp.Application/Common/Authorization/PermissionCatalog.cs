using System.Reflection;

namespace GastroErp.Application.Common.Authorization;

public sealed record PermissionDefinition(
    string Module,
    string Name,
    string DisplayName,
    string? Category = null,
    string? Group = null);

public static class PermissionCatalog
{
    public static IReadOnlyList<PermissionDefinition> GetAll()
    {
        var results = new List<PermissionDefinition>();
        CollectFromType(typeof(Permissions), results);
        return results
            .GroupBy(p => p.Name, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .OrderBy(p => p.Module)
            .ThenBy(p => p.Name)
            .ToList();
    }

    private static void CollectFromType(Type type, ICollection<PermissionDefinition> results, string? module = null)
    {
        if (type.IsNested && type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Any(f => f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(string)))
        {
            module ??= type.Name;
            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy))
            {
                if (!field.IsLiteral || field.IsInitOnly || field.FieldType != typeof(string))
                {
                    continue;
                }

                var permissionName = (string)field.GetRawConstantValue()!;
                var dotIndex = permissionName.IndexOf('.');
                var resolvedModule = dotIndex > 0 ? permissionName[..dotIndex] : module;
                results.Add(new PermissionDefinition(resolvedModule, permissionName, permissionName, resolvedModule, type.Name));
            }
            return;
        }

        foreach (var nested in type.GetNestedTypes(BindingFlags.Public))
        {
            CollectFromType(nested, results, nested.Name);
        }
    }
}
