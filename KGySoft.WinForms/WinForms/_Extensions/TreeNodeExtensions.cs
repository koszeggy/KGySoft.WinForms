#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: TreeNodeExtensions.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2026 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System.Windows.Forms;

#endregion

namespace KGySoft.WinForms
{
    /// <summary>
    /// Provides extension methods for the <see cref="TreeNode"/> class.
    /// </summary>
    public static class TreeNodeExtensions
    {
        #region Methods

        #region Public Methods

        /// <summary>
        /// Gets the next node by depth search.
        /// </summary>
        /// <param name="node">The start node.</param>
        /// <returns>The next node by depth search, or <see langword="null"/> if next node was not found.</returns>
        public static TreeNode? NextNodeDepth(this TreeNode? node) => NextNodeDepth(node, null);

        /// <summary>
        /// Gets the next node by depth search, restricting the search to the children of the specified <paramref name="searchRoot"/>.
        /// </summary>
        /// <param name="node">Start node</param>
        /// <param name="searchRoot">Root node above which searching is not performed</param>
        /// <returns>The next node by depth search, or <see langword="null"/> if no next node was found under <paramref name="searchRoot"/>.</returns>
        public static TreeNode? NextNodeDepthFromRoot(this TreeNode? node, TreeNode? searchRoot) => NextNodeDepth(node, searchRoot);

        #endregion

        #region Private Methods

        private static TreeNode? NextNodeDepth(TreeNode? tn, TreeNode? searchRoot)
        {
            if (tn == null)
                return null;

            // has children
            if (tn.Nodes.Count > 0)
                return tn.Nodes[0];

            // has next sibling which is not the sibling of the defined local root
            if (tn.NextNode != null)
                if (searchRoot == null || searchRoot != tn)
                    return tn.NextNode;
                else if (searchRoot == tn)
                    return null;

            // neither child nor next sibling: searching for the first parent that has next sibling (which is not the defined search root)
            while (tn.Parent != null && (searchRoot == null || searchRoot != tn.Parent))
            {
                tn = tn.Parent;
                if (tn.NextNode != null)
                    return tn.NextNode;
            }

            // no more elements
            return null;
        }

        #endregion

        #endregion
    }
}
