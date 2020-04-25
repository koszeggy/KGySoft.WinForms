using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace KGySoft.WinForms
{
    /// <summary>
    /// Extension methods on <see cref="TreeNode"/> class.
    /// </summary>
    public static class TreeNodeExtensions
    {
        /// <summary>
        /// Obtaining next node by depth search
        /// </summary>
        /// <param name="tn">Start node</param>
        /// <returns>Next node or null of next node not found.</returns>
        public static TreeNode NextNodeDepth(this TreeNode tn)
        {
            return NextNodeDepth(tn, null);
        }

        /// <summary>
        /// Obtaining next node by depth search but only under <paramref name="searchRoot"/>.
        /// </summary>
        /// <param name="tn">Start node</param>
        /// <param name="searchRoot">Root node above which searching is not performed</param>
        /// <returns>Next node or null of next node not found.</returns>
        public static TreeNode NextNodeDepthFromRoot(this TreeNode tn, TreeNode searchRoot)
        {
            return NextNodeDepth(tn, searchRoot);
        }

        /// <summary>
        /// Obtaining next node by depth search
        /// </summary>
        /// <param name="tn">Current node</param>
        /// <param name="searchRoot">Search root node</param>
        private static TreeNode NextNodeDepth(TreeNode tn, TreeNode searchRoot)
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
    }
}
