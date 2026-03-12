using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Diagnostics.CodeAnalysis;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Transactions;
using TreeDataStructures.Interfaces;

namespace TreeDataStructures.Core;

public abstract class BinarySearchTreeBase<TKey, TValue, TNode>(IComparer<TKey>? comparer = null) 
    : ITree<TKey, TValue>
    where TNode : Node<TKey, TValue, TNode>
{
    protected TNode? Root;
    public IComparer<TKey> Comparer { get; protected set; } = comparer ?? Comparer<TKey>.Default; // use it to compare Keys

    public int Count { get; protected set; } 
    
    public bool IsReadOnly => false;  // свойство, часть реализации интерфейса, без нее нельзя, всегда вернет false

    public ICollection<TKey> Keys
    {
        get
        {
            var keys = new List<TKey>();
            foreach (var node in InOrder())
            {
                keys.Add(node.Key);
            }
            return keys;
        }
    }

    public ICollection<TValue> Values
    {
        get
        {
            var values = new List<TValue>();
            foreach (var node in InOrder())
            {
                values.Add(node.Value);
            }
            return values;
        }
    }
    
    
    public virtual void Add(TKey key, TValue value) 
    {
        TNode nodeToAdd = CreateNode(key, value);
        TNode? cur = Root;
        TNode? parent = null;
        int cmp = 0;
        while (cur != null)
        {
            parent = cur;
            cmp = Comparer.Compare(key, cur.Key);
            if (cmp >= 0)
            {
                cur = cur.Right;
            } else
            {
                cur = cur.Left;
            }
        }
        if (parent == null)
        {
            Root = nodeToAdd;
        } else if (cmp < 0)
        {
            parent.Left = nodeToAdd;
        } else
        {
            parent.Right = nodeToAdd;
        }
        nodeToAdd.Parent = parent;
        this.Count++;
        OnNodeAdded(nodeToAdd);
    }

    
    public virtual bool Remove(TKey key)
    {
        TNode? node = FindNode(key);
        if (node == null) { return false; }

        RemoveNode(node);
        OnNodeRemoved(node, node.Parent); // хз сработает ли
        this.Count--;
        return true;
    }
    
    
    protected virtual void RemoveNode(TNode? node)
    {
        ArgumentNullException.ThrowIfNull(node); 
        if (node.Left == null && node.Right == null)
        {
            Transplant(node, null);
        } else if (node.Left == null)
        {
           Transplant(node, node.Right);
        } else if (node.Right == null)
        {
            Transplant(node, node.Left);
        } else
        {
            TNode temp = node.Left;
            while (temp.Right != null)
            {
                temp = temp.Right;
            }

            if (temp.Parent != node)
            {
                Transplant(temp, temp.Left);
                temp.Left = node.Left;
                temp.Left.Parent = temp;
            }
            Transplant(node, temp);
            temp.Right = node.Right;
            temp.Right.Parent = temp;
        }
    }

    public virtual bool ContainsKey(TKey key) => FindNode(key) != null;
    
    public virtual bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value) // шобы компилятор не ругался когда возвращаем null
    {
        TNode? node = FindNode(key);
        if (node != null)
        {
            value = node.Value;
            return true;
        }
        value = default;
        return false;
    }

    public TValue this[TKey key]  // короче штука чтобы был индексатор
    {
        get => TryGetValue(key, out TValue? val) ? val : throw new KeyNotFoundException();
        set
        {
            TNode ?node = FindNode(key);
            if (node == null)
            {
                Add(key, value);
            } else
            {
                node.Value = value;
            }
        }
    }

    
    #region Hooks
    
    /// <summary>
    /// Вызывается после успешной вставки
    /// </summary>
    /// <param name="newNode">Узел, который встал на место</param>
    protected virtual void OnNodeAdded(TNode newNode)  {}
  
    
    /// <summary>
    /// Вызывается после удаления. 
    /// </summary>
    /// <param name="parent">Узел, чей ребенок изменился</param>
    /// <param name="child">Узел, который встал на место удаленного</param>
    protected virtual void OnNodeRemoved(TNode? parent, TNode? child) {}
    
    #endregion
    
    
    #region Helpers
    protected abstract TNode CreateNode(TKey key, TValue value);
    
    protected TNode? FindNode(TKey key)
    {
        TNode? current = Root;
        while (current != null)
        {
            int cmp = Comparer.Compare(key, current.Key);
            if (cmp == 0) { return current; }
            current = cmp < 0 ? current.Left : current.Right;
        }
        return null;
    }

    protected void RotateLeft(TNode x)
    {
        ArgumentNullException.ThrowIfNull(x);

        TNode ?y = x.Right;
        if (y == null)
        {
            throw new ArgumentNullException("Cannot rotate");
        }
        x.Right = y.Left;
        if (y.Left != null)
        {
            y.Left.Parent = x;
        }
        y.Left = x;

        if (x.Parent == null)
        {
            this.Root = y;

        } else if (x == x.Parent.Left)
        {
            x.Parent.Left = y;
        } else
        {
            x.Parent.Right = y;
        }
        y.Parent = x.Parent;
        x.Parent = y;
    }

    protected void RotateRight(TNode y)
    {
        ArgumentNullException.ThrowIfNull(y);

        TNode ?x = y.Left;
        if (x == null)
        {
            throw new ArgumentNullException("Cannot rotate");
        }
        y.Left = x.Right;
        if (x.Right != null)
        {
            x.Right.Parent = y;
        }
        x.Right = y;

        if (y.Parent == null)
        {
            this.Root = x;

        } else if (y == y.Parent.Right)
        {
            y.Parent.Right = x;
        } else
        {
            y.Parent.Left = x;
        }
        x.Parent = y.Parent;
        y.Parent = x;
    }
    
    protected void RotateBigLeft(TNode x)  
    {
        ArgumentNullException.ThrowIfNull(x);
        if (x.Right != null)
        {
            RotateRight(x.Right);
        } else
        {
            throw new NullReferenceException();
        }
        RotateLeft(x);
    }
    
    protected void RotateBigRight(TNode y)
    {
        ArgumentNullException.ThrowIfNull(y);
        if (y.Left != null)
        {
            RotateLeft(y.Left); 
        } else
        {
            throw new NullReferenceException();
        }
        RotateRight(y);
    }
    
    protected void RotateDoubleLeft(TNode x)
    {
        ArgumentNullException.ThrowIfNull(x);
        RotateLeft(x);
        if (x.Parent != null)
        {
            RotateLeft(x.Parent);
        } else
        {
            throw new NullReferenceException();
        }
    }
    
    protected void RotateDoubleRight(TNode y)
    {
        ArgumentNullException.ThrowIfNull(y);
        RotateRight(y);
        if (y.Parent != null)
        {
            RotateRight(y.Parent);    
        } else
        {
            throw new NullReferenceException();
        }
    }
    
    protected void Transplant(TNode u, TNode? v)
    {
        if (u.Parent == null)
        {
            Root = v;
        }
        else if (u.IsLeftChild)
        {
            u.Parent.Left = v;
        }
        else
        {
            u.Parent.Right = v;
        }
        v?.Parent = u.Parent;
    }
    #endregion
    
    public IEnumerable<TreeEntry<TKey, TValue>>  InOrder() => new TreeIterator(Root, TraversalStrategy.InOrder);
    public IEnumerable<TreeEntry<TKey, TValue>>  PreOrder() => new TreeIterator(Root, TraversalStrategy.PreOrder);
    public IEnumerable<TreeEntry<TKey, TValue>>  PostOrder() => new TreeIterator(Root, TraversalStrategy.PostOrder);
    public IEnumerable<TreeEntry<TKey, TValue>>  InOrderReverse() => new TreeIterator(Root, TraversalStrategy.InOrderReverse);
    public IEnumerable<TreeEntry<TKey, TValue>>  PreOrderReverse() => new TreeIterator(Root, TraversalStrategy.PreOrderReverse);
    public IEnumerable<TreeEntry<TKey, TValue>>  PostOrderReverse() => new TreeIterator(Root, TraversalStrategy.PostOrderReverse);
    
    /// <summary>
    /// Внутренний класс-итератор. 
    /// Реализует паттерн Iterator вручную, без yield return (ban).
    /// </summary>
    private struct TreeIterator : 
        IEnumerable<TreeEntry<TKey, TValue>>,
        IEnumerator<TreeEntry<TKey, TValue>>
    {
        private readonly TraversalStrategy _strategy; // or make it template parameter?
        private TNode Root;
        private TNode? currentAlgo;
        private TNode current;
        private TNode? previous = null;
        
        public IEnumerator<TreeEntry<TKey, TValue>> GetEnumerator() => this;
        IEnumerator IEnumerable.GetEnumerator() => this;
        
         public TreeEntry<TKey, TValue> Current  
        {
            get
            {
                int depth = 0;
                TNode cur = current;
                while (cur.Parent != null)
                {
                    depth++;
                    cur = cur.Parent; 
                }
                return new TreeEntry<TKey, TValue>(current.Key, current.Value, depth);    
            }

        }


        object IEnumerator.Current => Current;

        public TreeIterator(TNode root, TraversalStrategy strategy)
        {
            if (root == null)
            {
                Console.WriteLine("null root");
            }
            this._strategy = strategy;
            this.Root = root;
            this.current = root;
            this.previous = null;
            this.currentAlgo = root;
        }

        public bool MoveNext()
        {
            if (current == null)
            {
                return false;
            } 
            else
            {
                switch (_strategy)
                {
                    case TraversalStrategy.InOrder:
                        return MoveNextInOrder();
                    case TraversalStrategy.PreOrder:
                        return MoveNextPreOrder();
                    case TraversalStrategy.PostOrder:
                        return MoveNextPostOrder();
                    case TraversalStrategy.InOrderReverse:
                        return MoveNextInOrderReverse();
                    case TraversalStrategy.PreOrderReverse:
                        return MoveNextPreOrderReverse();
                    case TraversalStrategy.PostOrderReverse:
                        return MoveNextPostOrderReverse();
                    default:
                        throw new InvalidOperationException("Do not have such traversal!\n");
                }
            }
        }

        private bool MoveNextInOrder()
        {
            while (currentAlgo != null)
            {
                if (previous == currentAlgo.Parent)
                {
                    previous = currentAlgo;
                    if (currentAlgo.Left != null)
                    {
                        currentAlgo = currentAlgo.Left;
                    } else
                    {
                        current = currentAlgo;
                        if (currentAlgo.Right != null)
                        {
                            currentAlgo = currentAlgo.Right;
                        } else
                        {
                            currentAlgo = currentAlgo.Parent;
                        }
                        return true;
                    }
                } else if (previous == currentAlgo.Left)
                {
                    previous = currentAlgo;
                    current = currentAlgo;
                    if (currentAlgo.Right != null)
                    {
                        currentAlgo = currentAlgo.Right;
                    } else
                    {
                        currentAlgo = currentAlgo.Parent;
                    }
                    return true;
                } else if (previous == currentAlgo.Right)
                {
                    previous = currentAlgo;
                    currentAlgo = currentAlgo.Parent;
                }                
            }
            return false;
        }

        private bool MoveNextPreOrder()
        {
            while (currentAlgo != null)
            {
                if (previous == currentAlgo.Parent)
                {
                    previous = currentAlgo;
                    current = currentAlgo;
                    if (currentAlgo.Left != null)
                    {
                        currentAlgo = currentAlgo.Left;
                    } else if (currentAlgo.Right != null)
                    {
                        currentAlgo = currentAlgo.Right;
                    } else {
                        currentAlgo = currentAlgo.Parent;
                    }
                    return true;
                } else if (previous == currentAlgo.Left)
                {
                    previous = currentAlgo;
                    if (currentAlgo.Right != null)
                    {
                        currentAlgo = currentAlgo.Right;

                    } else
                    {
                        currentAlgo = currentAlgo.Parent;
                    }
                } else if (previous == currentAlgo.Right)
                {
                    previous = currentAlgo;
                    currentAlgo = currentAlgo.Parent;
                }                
            }
            return false;
        }

        private bool MoveNextPostOrder()
        {
            while (currentAlgo != null)
            {
                if (previous == currentAlgo.Parent)
                {
                    previous = currentAlgo;
                    if (currentAlgo.Left != null)
                    {
                        currentAlgo = currentAlgo.Left;
                    } else
                    {
                        current = currentAlgo;
                        if (currentAlgo.Right != null)
                        {
                            currentAlgo = currentAlgo.Right;
                        } else
                        {
                            currentAlgo = currentAlgo.Parent;
                        }
                        return true;
                    }
                } else if (previous == currentAlgo.Left)
                {
                    previous = currentAlgo;
                    if (currentAlgo.Right != null)
                    {
                        currentAlgo = currentAlgo.Right;

                    } else
                    {
                        currentAlgo = currentAlgo.Parent;
                    }
                } else if (previous == currentAlgo.Right)
                {
                    current = currentAlgo;
                    previous = currentAlgo;
                    currentAlgo = currentAlgo.Parent;
                    return true;
                }                
            }
            return false;
        }

        private bool MoveNextInOrderReverse()
        {
            while (currentAlgo != null)
            {
                if (previous == currentAlgo.Parent)
                {
                    previous = currentAlgo;
                    if (currentAlgo.Right != null)
                    {
                        currentAlgo = currentAlgo.Right;
                    } else
                    {
                        current = currentAlgo;
                        if (currentAlgo.Left != null)
                        {
                            currentAlgo = currentAlgo.Left;
                        } else
                        {
                            currentAlgo = currentAlgo.Parent;
                        }
                        return true;
                    } 
                } else if (previous == currentAlgo.Right)
                {
                    previous = currentAlgo;
                    current = currentAlgo;
                    if (currentAlgo.Left != null)
                    {
                        currentAlgo = currentAlgo.Left;
                    } else
                    {
                        currentAlgo = currentAlgo.Parent;
                    }
                    return true;
                } else if (previous == currentAlgo.Left)
                {
                    previous = currentAlgo;
                    currentAlgo = currentAlgo.Parent;
                }
            }
            return false;
        }

        private bool MoveNextPreOrderReverse()
        {
            while (currentAlgo != null)
            {
                if (previous == currentAlgo.Parent)
                {
                    previous = currentAlgo;
                    if (currentAlgo.Right != null)
                    {
                        currentAlgo = currentAlgo.Right;
                    } else
                    {
                        current = currentAlgo;
                        if (currentAlgo.Left != null)
                        {
                            currentAlgo = currentAlgo.Left;
                        } else
                        {
                            currentAlgo = currentAlgo.Parent;
                        }
                        return true;
                    } 
                } else if (previous == currentAlgo.Right)
                {
                    previous = currentAlgo;
                    if (currentAlgo.Left != null)
                    {
                        currentAlgo = currentAlgo.Left;
                    } else
                    {
                        currentAlgo = currentAlgo.Parent;
                    }
                } else if (previous == currentAlgo.Left)
                {
                    current = currentAlgo;
                    previous = currentAlgo;
                    currentAlgo = currentAlgo.Parent;
                    return true;
                }
            }
            return false;
        }

        private bool MoveNextPostOrderReverse()
        {
            while (currentAlgo != null)
            {
                if (previous == currentAlgo.Parent)
                {
                    previous = currentAlgo;
                    current = currentAlgo;
                    if (currentAlgo.Right != null)
                    {
                        currentAlgo = currentAlgo.Right;
                    } else if (currentAlgo.Left != null)
                    {
                        currentAlgo = currentAlgo.Left;
                    } else {
                        currentAlgo = currentAlgo.Parent;
                    }
                    return true;
                } else if (previous == currentAlgo.Right)
                {
                    previous = currentAlgo;
                    if (currentAlgo.Left != null)
                    {
                        currentAlgo = currentAlgo.Left;

                    } else
                    {
                        currentAlgo = currentAlgo.Parent;
                    }
                } else if (previous == currentAlgo.Left)
                {
                    previous = currentAlgo;
                    currentAlgo = currentAlgo.Parent;
                }                
            }
            return false;

        }

        public void Reset()
        {
            current = Root;
            currentAlgo = Root;
            previous = null;
        }

        
        public void Dispose()
        {
            // TODO release managed resources here
        }
    }
    
    
    private enum TraversalStrategy { InOrder, PreOrder, PostOrder, InOrderReverse, PreOrderReverse, PostOrderReverse }
    
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        throw new NotImplementedException();
    }
    
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


    public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);
    public void Clear() { Root = null; Count = 0; }
    public bool Contains(KeyValuePair<TKey, TValue> item) => ContainsKey(item.Key);
    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => throw new NotImplementedException();
    public bool Remove(KeyValuePair<TKey, TValue> item) => Remove(item.Key);
}
