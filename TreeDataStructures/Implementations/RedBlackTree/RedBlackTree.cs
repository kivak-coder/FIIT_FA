using TreeDataStructures.Core;

namespace TreeDataStructures.Implementations.RedBlackTree;

public class RedBlackTree<TKey, TValue> : BinarySearchTreeBase<TKey, TValue, RbNode<TKey, TValue>>
    where TKey : IComparable<TKey>
{
    protected override RbNode<TKey, TValue> CreateNode(TKey key, TValue value)
    {
        throw new NotImplementedException();
    }
    
    protected override void OnNodeAdded(RbNode<TKey, TValue> newNode)
    {
        FixRedBlack(newNode);
    }
    protected override void OnNodeRemoved(RbNode<TKey, TValue>? parent, RbNode<TKey, TValue>? child)
    {
        throw new NotImplementedException();
    }

    private void FixRedBlack(RbNode<TKey, TValue> node)
    {
        if (node == Root)
        {
            node.Color = RbColor.Black;
            return;
        }
        while (node.Parent.Color == RbColor.Red)
        {
            if (node.Parent.IsLeftChild)
            {
                if (Uncle(node).Color == RbColor.Red)
                {
                    node.Parent.Color = RbColor.Black;
                    Uncle(node).Color = RbColor.Black; // не работает по идее
                    grandparent(node).Color = RbColor.Red;
                    node = grandparent(node);
                } else
                {
                    if (node.IsRightChild)
                    {
                        node = node.Parent;
                        RotateLeft(node);
                    }
                    node.Parent.Color = RbColor.Black;
                    grandparent(node).Color = RbColor.Red;
                    RotateRight(grandparent(node)); 
                }
            } else
            {
                if (Uncle(node).Color == RbColor.Red)
                {
                    node.Parent.Color = RbColor.Black;
                    Uncle(node).Color = RbColor.Black;
                    grandparent(node).Color = RbColor.Red;
                    node = grandparent(node);
                } else
                {
                    if (node.IsLeftChild)
                    {
                        node = node.Parent;
                        RotateRight(node);
                    }
                    node.Parent.Color = RbColor.Black;
                    grandparent(node).Color = RbColor.Red;
                    RotateLeft(grandparent(node));
                }
            }
        }
        Root.Color = RbColor.Black;
    }

    private static RbNode<TKey, TValue> Uncle(RbNode<TKey, TValue> node)
    {
        if (node.Parent.IsLeftChild)
        {
            return node.Parent.Parent.Right;
        } else
        {
            return node.Parent.Parent.Left;
        }
    }

    private static RbNode<TKey, TValue> grandparent(RbNode<TKey, TValue> node)
    {
        return node.Parent.Parent;
    }
}