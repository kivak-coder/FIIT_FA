using System.ComponentModel.DataAnnotations;
using TreeDataStructures.Core;

namespace TreeDataStructures.Implementations.AVL;
#nullable enable

public class AvlTree<TKey, TValue> : BinarySearchTreeBase<TKey, TValue, AvlNode<TKey, TValue>>
    where TKey : IComparable<TKey>
{
    protected override AvlNode<TKey, TValue> CreateNode(TKey key, TValue value)
        => new(key, value);
    
    protected override void OnNodeAdded(AvlNode<TKey, TValue> newNode)
    {
        ArgumentNullException.ThrowIfNull(newNode);
        AvlNode<TKey, TValue> node;
        if (newNode.Parent != null)
        {
            node = newNode.Parent;
        } else
        {
            node = newNode; 
        }

        while (node.Parent != null)
        {
            node.Height = UpdateHeight(node);
            Balance(node);
            node = node.Parent;
        }        
    } 

    private int UpdateHeight(AvlNode<TKey, TValue> node)
    {
        return Math.Max(GetHeight(node.Left), GetHeight(node.Right)) + 1;
    }

    private int GetHeight(AvlNode<TKey, TValue>? node)
    {
        return node?.Height ?? 0;
    }

    private int CountBalanceFactor(AvlNode<TKey, TValue> node)
    {
        return GetHeight(node.Right) - GetHeight(node.Left);
    }

    private int GetBalanceFactor(AvlNode<TKey, TValue> node)
    {
        return node == null ? 0 : CountBalanceFactor(node);
    }

    private void Balance(AvlNode<TKey, TValue> node)
    {
        if (GetBalanceFactor(node) < -1) // перекос вправо
        {
            if (GetBalanceFactor(node?.Left) <= 0)
            {
                RotateRight(node);
                UpdateHeight(node);
                UpdateHeight(node.Left);
            } else
            {
                RotateBigRight(node);
                UpdateHeight(node);
                UpdateHeight(node.Left);
                UpdateHeight(node.Right);
            }
        } else if (GetBalanceFactor(node) > 1) // перекос влево
        {
            if (GetBalanceFactor(node.Right) > 0)
            {
                RotateBigLeft(node);
                UpdateHeight(node);
                UpdateHeight(node.Left);
                UpdateHeight(node.Right);
            } else
            {
                RotateLeft(node);
                UpdateHeight(node);
                UpdateHeight(node.Right);
            }
        }
    }
}