﻿using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace RetsSdk.Contracts
{
    public interface IMetadataCollection<T> where T : class
    {
        void Add(T resource);
        void Remove(T resource);
        IEnumerable<T> Get();

        Type GetGenericType();
    }

    public interface IMetadataCollection
    {
        string Version { get; set; }
        DateTime Date { get; set; }

    }

    public interface IMetadataCollectionLoad
    {
        void Load(Type collectionType, XElement xElement);

    }
}