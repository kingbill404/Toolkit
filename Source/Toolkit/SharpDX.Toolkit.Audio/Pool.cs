﻿// Copyright (c) 2010-2013 SharpDX - SharpDX Team
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpDX.Toolkit.Audio
{
    internal abstract class Pool<TItem>
    {
        private List<TItem> activeItems;
        private List<TItem> swap;
        private Queue<TItem> freeItems;
      

        public Pool()
        {    
            activeItems = new List<TItem>();
            swap = new List<TItem>();
            freeItems = new Queue<TItem>();
        }


        public TItem Acquire(bool trackItem)
        {
            lock (freeItems)
            {

                if (freeItems.Count == 0)
                    AuditItems();

                TItem item;
                if (freeItems.Count == 0)
                {
                    item = Create();
                }
                else
                {
                    item = freeItems.Dequeue();
                    Reset(item);
                }

                if(trackItem)
                    activeItems.Add(item);
                
                return item;
            }
        }


        public void Clear()
        {
            lock (freeItems)
            {
                foreach (var item in activeItems)
                {
                    ClearItem(item);
                }

                activeItems.Clear();

                while (freeItems.Count > 0)
                {
                    ClearItem(freeItems.Dequeue());
                }               
            }
        }


        protected abstract bool IsActive(TItem item);
        protected abstract TItem Create();
        protected virtual void Reset(TItem item) { }
        protected virtual void ClearItem(TItem item) { }


        private void AuditItems()
        {
            foreach (var item in activeItems)
            {
                if (IsActive(item))
                {
                    swap.Add(item);
                }
                else
                {
                    freeItems.Enqueue(item);
                }
            }

            var temp = activeItems;
            activeItems = swap;
            swap = temp;
            swap.Clear();
        }


    }
}
