using Alachisoft.NoSDB.Common.JSON.Indexing;
using CSharpTest.Net.Collections;

namespace Alachisoft.NoSDB.Core.Storage.Indexing
{
    public abstract class IndexOperation
    {
        public IndexOperation(long rowId, AttributeValue value)
        {
            _rowId = rowId;
            _value = value;
        }

        public enum Type { Add, Update, Remove, NoOp }
        protected long _rowId;
        protected AttributeValue _value;

        public long RowId
        { get { return _rowId; } }

        public AttributeValue Value { get { return _value; } }

        public abstract Type OperationType { get; }

        public abstract bool MergeOperation(ref IndexOperation postOperation);
        public abstract void Execute(BPlusTree<AttributeValue, long> tree);
    }

    public class RemoveOperation : IndexOperation
    {
        public RemoveOperation(long rowId, AttributeValue value)
            : base(rowId, value)
        { }

        public override IndexOperation.Type OperationType
        {
            get { return Type.Remove; }
        }

        public override bool MergeOperation(ref IndexOperation postOperation)
        {
            if (this._rowId != postOperation.RowId)
                return false;
            switch (postOperation.OperationType)
            {
                case Type.Add:
                    if (this._value == postOperation.Value)
                        postOperation = new NoOp();
                    else
                        postOperation = new UpdateOperation(this._rowId, this._value, postOperation.Value);
                    return true;
                case Type.Remove:
                    return true;
                case Type.Update:
                    UpdateOperation update = postOperation as UpdateOperation;
                    if (update.OldValue == this._value)
                        return true;
                    return false;
                case Type.NoOp:
                    postOperation = this;
                    return true;
                default:
                    return false;
            }
        }

        public override void Execute(BPlusTree<AttributeValue, long> tree)
        {
            tree.TryRemove(this._value, this._rowId);
        }
    }

    public class AddOperation : IndexOperation
    {
        public AddOperation(long rowId, AttributeValue value)
            : base(rowId, value)
        { }

        public override IndexOperation.Type OperationType
        {
            get { return Type.Add; }
        }
        public override bool MergeOperation(ref IndexOperation postOperation)
        {
            if (this._rowId != postOperation.RowId)
                return false;
            switch (postOperation.OperationType)
            {
                case Type.Add:
                    if (this._value == postOperation.Value)
                    {
                        return true;
                    }
                    return false;
                case Type.Remove:
                    if (this._value == postOperation.Value)
                    {
                        postOperation = new NoOp();
                        return true;
                    }
                    return false;
                case Type.Update:
                    if (this._value == (postOperation as UpdateOperation).OldValue)
                        return true;
                    else if (this._value == postOperation.Value)
                        postOperation = new RemoveOperation(postOperation.RowId, (postOperation as UpdateOperation).OldValue);
                    return false;
                case Type.NoOp:
                    postOperation = this;
                    return true;
                default:
                    return false;
            }
        }

        public override void Execute(BPlusTree<AttributeValue, long> tree)
        {
            tree.Add(this._value, this._rowId);
        }
    }

    public class UpdateOperation : IndexOperation
    {
        private AttributeValue _oldValue;
        public UpdateOperation(long rowId, AttributeValue oldValue, AttributeValue newValue)
            : base(rowId, newValue)
        {
            this._oldValue = oldValue;
        }

        public AttributeValue OldValue { get { return _oldValue; } }

        public override IndexOperation.Type OperationType
        {
            get { return Type.Update; }
        }

        public override bool MergeOperation(ref IndexOperation postOperation)
        {
            if (this._rowId != postOperation.RowId)
                return false;
            switch (postOperation.OperationType)
            {
                case Type.Add:
                    if (this._value == postOperation.Value)
                    {
                        postOperation = this;
                        return true;
                    }
                    return false;
                case Type.Remove:
                    if (this._value == postOperation.Value)
                    {
                        postOperation = new RemoveOperation(this._rowId, this._oldValue);
                        return true;
                    }
                    else if (this._oldValue == postOperation.Value)
                    {
                        postOperation = this;
                        return true;
                    }
                    return false;
                case Type.Update:
                    UpdateOperation update = postOperation as UpdateOperation;
                    if (this._oldValue == update.OldValue && this._value == update.Value)
                        return true;
                    else if (this._value == update.OldValue)
                    {
                        update._oldValue = this._oldValue;
                        return true;
                    }
                    else if (this._value == update.Value)
                        postOperation = new RemoveOperation(update._rowId, update._oldValue);
                    else if (this._oldValue == update.Value)
                        postOperation = new AddOperation(update._rowId, update._value);
                    return false;
                case Type.NoOp:
                    postOperation = this;
                    return true;
                default:
                    return false;
            }
        }

        public override void Execute(BPlusTree<AttributeValue, long> tree)
        {
            tree.TryRemove(this._oldValue, this._rowId);
            tree.Add(this._value, this._rowId);
        }
    }

    public class NoOp : IndexOperation
    {
        public NoOp()
            : base(-1, null)
        { }

        public override IndexOperation.Type OperationType
        {
            get { return Type.NoOp; }
        }

        public override bool MergeOperation(ref IndexOperation postOperation)
        {
            return true;
        }

        public override void Execute(BPlusTree<AttributeValue, long> tree)
        {
        }
    }

}
