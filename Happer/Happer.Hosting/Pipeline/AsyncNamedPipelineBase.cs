using System;
using System.Collections.Generic;
using System.Linq;

namespace Happer.Pipelining
{
    public abstract class AsyncNamedPipelineBase<TAsyncDelegate, TSyncDelegate>
    {
        protected readonly List<PipelineItem<TAsyncDelegate>> _pipelineItems;

        protected AsyncNamedPipelineBase()
        {
            _pipelineItems = new List<PipelineItem<TAsyncDelegate>>();
        }

        protected AsyncNamedPipelineBase(int capacity)
        {
            _pipelineItems = new List<PipelineItem<TAsyncDelegate>>(capacity);
        }

        public IEnumerable<PipelineItem<TAsyncDelegate>> PipelineItems
        {
            get { return _pipelineItems.AsReadOnly(); }
        }

        public IEnumerable<TAsyncDelegate> PipelineDelegates
        {
            get { return _pipelineItems.Select(pipelineItem => pipelineItem.Delegate); }
        }

        public virtual void AddItemToStartOfPipeline(TAsyncDelegate item)
        {
            this.AddItemToStartOfPipeline((PipelineItem<TAsyncDelegate>)item);
        }

        public virtual void AddItemToStartOfPipeline(TSyncDelegate item)
        {
            this.AddItemToStartOfPipeline(this.Wrap(item));
        }

        public virtual void AddItemToStartOfPipeline(PipelineItem<TAsyncDelegate> item, bool replaceInPlace = false)
        {
            this.InsertItemAtPipelineIndex(0, item, replaceInPlace);
        }

        public virtual void AddItemToStartOfPipeline(PipelineItem<TSyncDelegate> item, bool replaceInPlace = false)
        {
            this.AddItemToStartOfPipeline(this.Wrap(item), replaceInPlace);
        }

        public virtual void AddItemToEndOfPipeline(TAsyncDelegate item)
        {
            this.AddItemToEndOfPipeline((PipelineItem<TAsyncDelegate>)item);
        }

        public virtual void AddItemToEndOfPipeline(TSyncDelegate item)
        {
            this.AddItemToEndOfPipeline(this.Wrap(item));
        }

        public virtual void AddItemToEndOfPipeline(PipelineItem<TAsyncDelegate> item, bool replaceInPlace = false)
        {
            var existingIndex = this.RemoveByName(item.Name);

            if (replaceInPlace && existingIndex != -1)
            {
                this.InsertItemAtPipelineIndex(existingIndex, item);
            }
            else
            {
                _pipelineItems.Add(item);
            }
        }

        public virtual void AddItemToEndOfPipeline(PipelineItem<TSyncDelegate> item, bool replaceInPlace = false)
        {
            this.AddItemToEndOfPipeline(this.Wrap(item), replaceInPlace);
        }

        public virtual void InsertItemAtPipelineIndex(int index, TAsyncDelegate item)
        {
            this.InsertItemAtPipelineIndex(index, (PipelineItem<TAsyncDelegate>)item);
        }

        public virtual void InsertItemAtPipelineIndex(int index, TSyncDelegate item)
        {
            this.InsertItemAtPipelineIndex(index, this.Wrap(item));
        }

        public virtual void InsertItemAtPipelineIndex(int index, PipelineItem<TAsyncDelegate> item, bool replaceInPlace = false)
        {
            var existingIndex = this.RemoveByName(item.Name);

            var newIndex = (replaceInPlace && existingIndex != -1) ? existingIndex : index;

            _pipelineItems.Insert(newIndex, item);
        }

        public virtual void InsertItemAtPipelineIndex(int index, PipelineItem<TSyncDelegate> item, bool replaceInPlace = false)
        {
            this.InsertItemAtPipelineIndex(index, this.Wrap(item), replaceInPlace);
        }

        public virtual void InsertBefore(string name, TAsyncDelegate item)
        {
            this.InsertBefore(name, (PipelineItem<TAsyncDelegate>)item);
        }

        public virtual void InsertBefore(string name, TSyncDelegate item)
        {
            this.InsertBefore(name, this.Wrap(item));
        }

        public virtual void InsertBefore(string name, PipelineItem<TAsyncDelegate> item)
        {
            var existingIndex = _pipelineItems.FindIndex(i => string.Equals(name, i.Name, StringComparison.Ordinal));

            if (existingIndex == -1)
            {
                existingIndex = 0;
            }

            this.InsertItemAtPipelineIndex(existingIndex, item);
        }

        public virtual void InsertBefore(string name, PipelineItem<TSyncDelegate> item)
        {
            this.InsertBefore(name, this.Wrap(item));
        }

        public virtual void InsertAfter(string name, TAsyncDelegate item)
        {
            this.InsertAfter(name, (PipelineItem<TAsyncDelegate>)item);
        }

        public virtual void InsertAfter(string name, TSyncDelegate item)
        {
            this.InsertAfter(name, this.Wrap(item));
        }

        public virtual void InsertAfter(string name, PipelineItem<TAsyncDelegate> item)
        {
            var existingIndex = _pipelineItems.FindIndex(i => string.Equals(name, i.Name, StringComparison.Ordinal));

            if (existingIndex == -1)
            {
                existingIndex = _pipelineItems.Count;
            }

            existingIndex++;

            if (existingIndex > _pipelineItems.Count)
            {
                this.AddItemToEndOfPipeline(item);
            }
            else
            {
                this.InsertItemAtPipelineIndex(existingIndex, item);
            }
        }

        public virtual void InsertAfter(string name, PipelineItem<TSyncDelegate> item)
        {
            this.InsertAfter(name, this.Wrap(item));
        }

        public virtual int RemoveByName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return -1;
            }

            var existingIndex = _pipelineItems.FindIndex(i => string.Equals(name, i.Name, StringComparison.Ordinal));

            if (existingIndex != -1)
            {
                _pipelineItems.RemoveAt(existingIndex);
            }

            return existingIndex;
        }

        protected abstract PipelineItem<TAsyncDelegate> Wrap(PipelineItem<TSyncDelegate> syncDelegate);
    }
}
