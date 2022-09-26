﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;

namespace CrestApps.RetsSdk.Contracts
{
    public interface IMetadataCollection<T> : IEnumerable<T> where T : class
    {
        void Add(T resource);
        void Remove(T resource);
        IEnumerable<T> Get();
        T Get(object value);

        Type GetGenericType();
    }

    public interface IMetadataCollection : IEnumerable
    {
        string Version { get; set; }
        DateTime Date { get; set; }
    }

    public interface IMetadataCollectionLoad
    {
        void Load(Type collectionType, XElement xElement);

    }
}