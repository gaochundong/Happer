using System;
using System.Collections.Generic;
using System.Linq;

namespace Happer.Pipelining
{
    public abstract class NamedPipelineBase<TDelegate>
    {
        protected readonly List<PipelineItem<TDelegate>> _pipelineItems;

        protected NamedPipelineBase()
        {
            _pipelineItems = new List<PipelineItem<TDelegate>>();
        }

        protected NamedPipelineBase(int capacity)
        {
            _pipelineItems = new List<PipelineItem<TDelegate>>(capacity);
        }

        public IEnumerable<PipelineItem<TDelegate>> PipelineItems
        {
            get { return _pipelineItems.AsReadOnly(); }
        }

        public IEnumerable<TDelegate> PipelineDelegates
        {
            get { return _pipelineItems.Select(pipelineItem => pipelineItem.Delegate); }
        }

        public virtual void AddItemToStartOfPipeline(TDelegate item)
        {
            this.AddItemToStartOfPipeline((PipelineItem<TDelegate>)item);
        }

        public virtual void AddItemToStartOfPipeline(PipelineItem<TDelegate> item, bool replaceInPlace = false)
        {
            this.InsertItemAtPipelineIndex(0, item, replaceInPlace);
        }

        public virtual void AddItemToEndOfPipeline(TDelegate item)
        {
            this.AddItemToEndOfPipeline((PipelineItem<TDelegate>)item);
        }

        public virtual void AddItemToEndOfPipeline(PipelineItem<TDelegate> item, bool replaceInPlace = false)
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

        public virtual void InsertItemAtPipelineIndex(int index, TDelegate item)
        {
            this.InsertItemAtPipelineIndex(index, (PipelineItem<TDelegate>)item);
        }

        public virtual void InsertItemAtPipelineIndex(int index, PipelineItem<TDelegate> item, bool replaceInPlace = false)
        {
            var existingIndex = this.RemoveByName(item.Name);

            var newIndex = (replaceInPlace && existingIndex != -1) ? existingIndex : index;

            _pipelineItems.Insert(newIndex, item);
        }

        public virtual void InsertBefore(string name, TDelegate item)
        {
            this.InsertBefore(name, (PipelineItem<TDelegate>)item);
        }

        public virtual void InsertBefore(string name, PipelineItem<TDelegate> item)
        {
            var existingIndex = _pipelineItems.FindIndex(i => String.Equals(name, i.Name, StringComparison.Ordinal));

            if (existingIndex == -1)
            {
                existingIndex = 0;
            }

            this.InsertItemAtPipelineIndex(existingIndex, item);
        }

        public virtual void InsertAfter(string name, TDelegate item)
        {
            this.InsertAfter(name, (PipelineItem<TDelegate>)item);
        }

        public virtual void InsertAfter(string name, PipelineItem<TDelegate> item)
        {
            var existingIndex = _pipelineItems.FindIndex(i => String.Equals(name, i.Name, StringComparison.Ordinal));

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

        public virtual int RemoveByName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return -1;
            }

            var existingIndex = _pipelineItems.FindIndex(i => String.Equals(name, i.Name, StringComparison.Ordinal));

            if (existingIndex != -1)
            {
                _pipelineItems.RemoveAt(existingIndex);
            }

            return existingIndex;
        }
    }
}
