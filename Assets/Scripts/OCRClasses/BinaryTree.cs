public class BinaryTree<T>
{
    private Node<T> root;

    public BinaryTree(T rootItem)
    {
        root = new Node<T>(rootItem);
    }

    public Node<T> GetRoot() { return root; }

    public class Node<T>
    {
        public T Item { get; private set; }
        public Node<T> LeftChild { get; private set; }
        public Node<T> RightChild { get; private set; }
        public bool isLeaf { get; private set; }

        public Node(T item)
        {
            Item = item;
            isLeaf = true;
        }

        public Node<T> GetLeftChild()
        {
            return LeftChild;
        }

        public Node<T> GetRightChild()
        {
            return RightChild;
        }

        public bool IsLeaf()
        {
            return isLeaf;
        }

        public void InsertToLeft(Node<T> node)
        {
            LeftChild = node;
            isLeaf = false;
        }

        public void InsertToLeft(T item)
        {
            InsertToLeft(new Node<T>(item));
        }

        public void InsertToRight(Node<T> node)
        {
            RightChild = node;
            isLeaf = false;
        }

        public void InsertToRight(T item)
        {
            InsertToRight(new Node<T>(item));
        }
    }
}
