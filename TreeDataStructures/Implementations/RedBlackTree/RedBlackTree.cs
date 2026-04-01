using System.Globalization;
using TreeDataStructures.Core;

namespace TreeDataStructures.Implementations.RedBlackTree;

public class RedBlackTree<TKey, TValue> : BinarySearchTreeBase<TKey, TValue, RbNode<TKey, TValue>>
    where TKey : IComparable<TKey>
{
    protected override RbNode<TKey, TValue> CreateNode(TKey key, TValue value)
    {
        return new(key, value);
    }
    
    protected override void OnNodeRemoved(RbNode<TKey, TValue>? parent, RbNode<TKey, TValue>? child)
    {
        if (GetColor(child) == RbColor.Black)
        {
            while (GetColor(child) == RbColor.Black && child != Root)
            {
               bool isLeft;
               if (child != null)
                {
                    isLeft = child.IsLeftChild;
                } else if (parent != null)
                {
                    if (parent.Left == null) // мб не совсем верно
                    {
                        isLeft = false;
                    } else
                    {
                        isLeft = true;
                    }
                } else
                {
                    break;
                }
                
                if (isLeft)
                {
                    var sibling = parent?.Right; // а нужна ли отдельная функция для нахождения?
                    if (GetColor(sibling) == RbColor.Red)
                    {
                        SetColor(sibling, RbColor.Black);
                        SetColor(parent, RbColor.Red);
                        RotateLeft(parent);
                        sibling = parent?.Right;
                    }
                    if (GetColor(sibling?.Right) == RbColor.Black && GetColor(sibling?.Left) == RbColor.Black && GetColor(sibling) == RbColor.Black)
                    {
                        SetColor(sibling, RbColor.Red);
                        child = parent;
                        parent = child?.Parent;
                        continue;
                    }
                    if (GetColor(sibling) == RbColor.Black && GetColor(sibling?.Right) == RbColor.Black)
                    {
                        SetColor(sibling?.Left, RbColor.Black);
                        SetColor(sibling, RbColor.Red);
                        RotateRight(sibling);
                        sibling = parent?.Right;
                    }
                    SetColor(sibling, GetColor(parent));
                    SetColor(parent, RbColor.Black);
                    SetColor(sibling?.Right, RbColor.Black);
                    RotateLeft(parent);
                    child = Root;
                } else
                {
                    var sibling = parent?.Left;
                    if (GetColor(sibling) == RbColor.Red)
                    {
                        SetColor(sibling, RbColor.Black);
                        SetColor(parent, RbColor.Red);
                        RotateRight(parent);
                        sibling = parent?.Left;
                    }
                    if (GetColor(sibling?.Right) == RbColor.Black && GetColor(sibling?.Left) == RbColor.Black && GetColor(sibling) == RbColor.Black)
                    {
                        SetColor(sibling, RbColor.Red);
                        child = parent;
                        parent = child?.Parent;
                        continue;
                    }
                    if (GetColor(sibling) == RbColor.Black && GetColor(sibling?.Left) == RbColor.Black)
                    {
                        SetColor(sibling?.Right, RbColor.Black);
                        SetColor(sibling, RbColor.Red);
                        RotateLeft(sibling);
                        sibling = parent?.Left;
                    }
                    SetColor(sibling, GetColor(parent));
                    SetColor(parent, RbColor.Black);
                    SetColor(sibling?.Left, RbColor.Black);
                    RotateRight(parent);
                    child = Root;
                }
            }
            SetColor(child, RbColor.Black);
            SetColor(Root, RbColor.Black);
        }
    }

    protected override void OnNodeAdded(RbNode<TKey, TValue> newNode)
    {
        if (newNode == Root)
        {
            SetColor(newNode, RbColor.Black);
            return;
        }
        while (GetColor(newNode.Parent) == RbColor.Red)
        {
            var uncle = Uncle(newNode);
            var parent = newNode.Parent;
            var grandparent = Grandparent(newNode);
            if (grandparent == null) break;

            if (newNode.Parent.IsLeftChild)
            {
                if (GetColor(uncle) == RbColor.Red)
                {
                    SetColor(parent, RbColor.Black);
                    SetColor(uncle, RbColor.Black); 
                    SetColor(grandparent, RbColor.Red);
                    newNode = grandparent;
                } else
                {
                    if (newNode.IsRightChild)
                    {
                        newNode = parent;
                        RotateLeft(newNode);
                        parent = newNode.Parent;
                        grandparent = Grandparent(newNode);
                    }
                    SetColor(parent, RbColor.Black);
                    if (grandparent != null)
                    {
                        SetColor(grandparent, RbColor.Red);
                        RotateRight(grandparent);  
                    }
                }
            } else
            {
                if (GetColor(uncle) == RbColor.Red)
                {
                    SetColor(parent, RbColor.Black);
                    SetColor(uncle, RbColor.Black);
                    SetColor(grandparent, RbColor.Red);
                    newNode = grandparent;
                } else
                {
                    if (newNode.IsLeftChild)
                    {
                        newNode = parent;
                        RotateRight(newNode);
                        parent = newNode.Parent;
                        grandparent = Grandparent(newNode);
                    }
                    SetColor(parent, RbColor.Black);
                    if (grandparent != null)
                    {
                        SetColor(grandparent, RbColor.Red);
                        RotateLeft(grandparent);  
                    }
                }
            }
        }
        SetColor(Root, RbColor.Black);
    }

    private static RbNode<TKey, TValue>? Uncle(RbNode<TKey, TValue> node)
    {
        var grand = Grandparent(node); 
        if (grand == null) return null;
        return node.Parent.IsLeftChild ? grand.Right : grand.Left;
    }

    private static RbNode<TKey, TValue>? Grandparent(RbNode<TKey, TValue> node)
    {
        return node.Parent?.Parent;
    }

    private static RbNode<TKey, TValue>? Sibling(RbNode<TKey, TValue> ?node)
    {
        if (node?.Parent == null) return null;
        return node.IsLeftChild ? node.Parent?.Right : node.Parent?.Left;
    }

    private static void SetColor(RbNode<TKey, TValue> ?node, RbColor color)
    {
        if (node != null)
        {
            node.Color = color;
        }
    }

    private static RbColor GetColor(RbNode<TKey, TValue> ?node)
    {
        return node?.Color ?? RbColor.Black;

    }
}