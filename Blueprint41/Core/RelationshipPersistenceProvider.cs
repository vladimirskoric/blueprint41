﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blueprint41.Core;
using System.Reflection;

namespace Blueprint41.Core
{
    internal abstract class RelationshipPersistenceProvider
    {
        protected RelationshipPersistenceProvider(PersistenceProvider factory)
        {
            this.PersistenceProviderFactory = factory;
        }

        public PersistenceProvider PersistenceProviderFactory { get; private set; }

        public abstract IEnumerable<CollectionItem> Load(OGM parent, EntityCollectionBase target);
        public abstract Dictionary<OGM, CollectionItemList> Load(IEnumerable<OGM> parents, Core.EntityCollectionBase target);
        public abstract void Add(Relationship relationship, OGM inItem, OGM outItem, DateTime? moment, bool timedependent);
        public abstract void Remove(Relationship relationship, OGM inItem, OGM outItem, DateTime? moment, bool timedependent);
        public abstract void RemoveAll(Relationship relationship, DirectionEnum direction, OGM item, DateTime? moment, bool timedependent);
        public abstract void AddUnmanaged(Relationship relationship, OGM inItem, OGM outItem, DateTime? startDate, DateTime? endDate, bool fullyUnmanaged = false);
        public abstract void RemoveUnmanaged(Relationship relationship, OGM inItem, OGM outItem, DateTime? startDate);

        public class CollectionItemList
        {
            private CollectionItemList(OGM parent)
            {
                Parent = parent;
                Items = new LinkedList<Core.CollectionItem>();
            }

            public static Dictionary<OGM, CollectionItemList> Get(IEnumerable<CollectionItem> items)
            {
                Dictionary<OGM, CollectionItemList> result = new Dictionary<OGM, CollectionItemList>();
                IEnumerable<CollectionItem> sorted = items.OrderBy(item => item.Parent.GetKey());

                CollectionItemList current = null;
                foreach (CollectionItem item in sorted)
                {
                    if (current == null || current.Parent != item.Parent)
                    {
                        current = new CollectionItemList(item.Parent);
                        result.Add(current.Parent, current);
                    }

                    current.Items.AddLast(item);
                }

                return result;
            }

            public OGM Parent { get; private set; }
            public LinkedList<CollectionItem> Items { get; private set; }
        }
    }
}
