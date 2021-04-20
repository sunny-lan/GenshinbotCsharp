﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace genshinbot
{
    /// <summary>
    /// Represents a stream of data which can be subscribed to.
    /// Keeps track of the # of subscribers, and disables the stream if # is 0
    /// Behavior on enabled/disabled is dependent on type of stream
    /// </summary>
    /// <typeparam name="T">Type of data</typeparam>
    public class Stream<T>
    {
        private int subCount = 0;

        public Stream(T init)
        {
            Value = init;
        }
        public int Subscribers
        {
            get => subCount;
            set
            {
                bool zero = value == 0,
                    prev = subCount == 0;
                if (zero != prev)
                {
                    EnableChanged?.Invoke(!zero);
                }
                subCount = value;
            }
        }

        /// <summary>
        /// This function will be called whenever the stream enabled state changes
        /// </summary>
        public virtual Action<bool> EnableChanged { get; set; }

        public T Value
        {
            get;
            private set;
        }

        public void Update(T newVal)
        {
            Value = newVal;
            foreach (var sub in subs)
                sub(newVal);
        }

        private ISet<Action<T>> subs = new HashSet<Action<T>>();

        event Action<T> Change
        {
            add
            {
                Subscribers++;
                Debug.Assert(subs.Add(value), "Subscriber added twice");
            }
            remove
            {
                Subscribers--;
                Debug.Assert(subs.Remove(value), "Subscriber not in list");
            }
        }

        public interface Subscriber : IDisposable { }

        class _Subscriber : Subscriber
        {
            Stream<T> parent;
            private Action<T> handler;

            internal _Subscriber(Stream<T> parent, Action<T> handler)
            {
                this.parent = parent;
                this.handler = handler;
                parent.Change += handler;
            }

            private bool disposed = false;

            public void Dispose()
            {
                Debug.Assert(!disposed, "Subscriber disposed twice");
                disposed = true;
                parent.Change -= handler;
            }

            ~_Subscriber()
            {
                if (!disposed)
                    Dispose();
            }
        }

        /// <summary>
        /// Attaches a callback as a subscriber to the stream
        /// </summary>
        /// <param name="callback"></param>
        /// <returns>A Subscriber object which should be disposed when the subscription is finished</returns>
        public Subscriber Listen(Action<T> callback)
        {
            return new _Subscriber(this, callback);
        }
    }
}