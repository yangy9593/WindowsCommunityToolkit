using System.Collections.Generic;
using System.Linq;

namespace WinCompData.Tools
{
#if !WINDOWS_UWP
    public
#endif
    abstract class CanonicalizedNode<T> : Graph.Node<T> where T : CanonicalizedNode<T>, new()
    {
        public CanonicalizedNode()
        {
            Canonical = (T)this;
            NodesInGroup = new T[] { (T)this };
        }

        /// <summary>
        /// The node that is equivalent to this node. Initially set to this.
        /// </summary>
        public T Canonical { get; set; }

        /// <summary>
        /// The nodes that are canonicalized to the canonical node.
        /// </summary>
        public IEnumerable<T> NodesInGroup { get; set; }

        public bool IsCanonical => Canonical == this;

        public IEnumerable<T> CanonicalInRefs
        {
            get
            {
                return
                    // Get the references from all canonical nodes
                    // that reference all versions of this node.
                    from n in NodesInGroup
                    from r in n.InReferences
                    where r.IsCanonical
                    select r.Canonical;
            }
        }
    }
}
