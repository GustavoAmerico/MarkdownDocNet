using EnvDTE;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MarkdownVsix
{
    /// <summary>A static helper class for working with the UI hierarchies.</summary>
    internal static class UIHierarchyHelper
    {
        /// <summary>Gets an enumerable set of the selected UI hierarchy items.</summary>
        /// <param name="package">The hosting package.</param>
        /// <returns>The enumerable set of selected UI hierarchy items.</returns>
        internal static IEnumerable<UIHierarchyItem> GetSelectedUIHierarchyItems(GenerateMarkdownPackage package)
        {
            var solutionExplorer = GetSolutionExplorer(package);

            return ((object[])solutionExplorer.SelectedItems).Cast<UIHierarchyItem>().ToList();
        }

        /// <summary>Gets the solution explorer for the specified hosting package.</summary>
        /// <param name="package">The hosting package.</param>
        /// <returns>The solution explorer.</returns>
        internal static UIHierarchy GetSolutionExplorer(GenerateMarkdownPackage package)
        {
            return package.IDE.ToolWindows.SolutionExplorer;
        }

        /// <summary>Gets the top level (solution) UI hierarchy item.</summary>
        /// <param name="package">The hosting package.</param>
        /// <returns>The top level (solution) UI hierarchy item, otherwise null.</returns>
        internal static UIHierarchyItem GetTopUIHierarchyItem(GenerateMarkdownPackage package)
        {
            var solutionExplorer = GetSolutionExplorer(package);

            return solutionExplorer.UIHierarchyItems.Count > 0
                ? solutionExplorer.UIHierarchyItems.Item(1)
                : null;
        }

        /// <summary>Determines whether the specified item has any expanded children.</summary>
        /// <param name="parentItem">The parent item.</param>
        /// <returns>True if there are expanded children, false otherwise.</returns>
        internal static bool HasExpandedChildren(UIHierarchyItem parentItem)
        {
            if (parentItem == null)
            {
                throw new ArgumentNullException(nameof(parentItem));
            }

            return parentItem.UIHierarchyItems.Cast<UIHierarchyItem>().Any(
                childItem => childItem.UIHierarchyItems.Expanded || HasExpandedChildren(childItem));
        }
    }
}