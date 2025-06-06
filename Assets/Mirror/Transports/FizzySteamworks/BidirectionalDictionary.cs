﻿using System.Collections;
using System.Collections.Generic;

namespace Mirror.FizzySteam
{
    public class BidirectionalDictionary<T1, T2> : IEnumerable
    {
        private readonly Dictionary<T1, T2> t1ToT2Dict = new();
        private readonly Dictionary<T2, T1> t2ToT1Dict = new();

        public IEnumerable<T1> FirstTypes => t1ToT2Dict.Keys;
        public IEnumerable<T2> SecondTypes => t2ToT1Dict.Keys;

        public int Count => t1ToT2Dict.Count;

        public T1 this[T2 key]
        {
            get => t2ToT1Dict[key];
            set => Add(key, value);
        }

        public T2 this[T1 key]
        {
            get => t1ToT2Dict[key];
            set => Add(key, value);
        }

        public IEnumerator GetEnumerator()
        {
            return t1ToT2Dict.GetEnumerator();
        }

        public void Add(T1 key, T2 value)
        {
            if (t1ToT2Dict.ContainsKey(key)) Remove(key);

            t1ToT2Dict[key] = value;
            t2ToT1Dict[value] = key;
        }

        public void Add(T2 key, T1 value)
        {
            if (t2ToT1Dict.ContainsKey(key)) Remove(key);

            t2ToT1Dict[key] = value;
            t1ToT2Dict[value] = key;
        }

        public T2 Get(T1 key)
        {
            return t1ToT2Dict[key];
        }

        public T1 Get(T2 key)
        {
            return t2ToT1Dict[key];
        }

        public bool TryGetValue(T1 key, out T2 value)
        {
            return t1ToT2Dict.TryGetValue(key, out value);
        }

        public bool TryGetValue(T2 key, out T1 value)
        {
            return t2ToT1Dict.TryGetValue(key, out value);
        }

        public bool Contains(T1 key)
        {
            return t1ToT2Dict.ContainsKey(key);
        }

        public bool Contains(T2 key)
        {
            return t2ToT1Dict.ContainsKey(key);
        }

        public void Remove(T1 key)
        {
            if (Contains(key))
            {
                var val = t1ToT2Dict[key];
                t1ToT2Dict.Remove(key);
                t2ToT1Dict.Remove(val);
            }
        }

        public void Remove(T2 key)
        {
            if (Contains(key))
            {
                var val = t2ToT1Dict[key];
                t1ToT2Dict.Remove(val);
                t2ToT1Dict.Remove(key);
            }
        }
    }
}