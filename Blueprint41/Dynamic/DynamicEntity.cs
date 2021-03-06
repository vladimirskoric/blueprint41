﻿using Blueprint41.Core;
using Blueprint41.Neo4j.Refactoring;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Blueprint41.Dynamic
{
    public class DynamicEntity : DynamicObject, OGM
    {
        internal DynamicEntity(Entity entity, bool shouldExecute) : this(entity, shouldExecute, null) { }
        internal DynamicEntity(Entity entity, bool shouldExecute, object initialize)
        {
            ShouldExecute = shouldExecute;
            Guid = entity.Parent.GenerateGuid(entity.Name);

            if (entity.Parent.IsUpgraded)
                throw new InvalidOperationException("You cannot use dynamic entity outside of the upgrade script.");

            DynamicEntityType = entity;

            Transaction = Transaction.Current;
            if (ShouldExecute)
                Transaction.Register(this);

            foreach (Property item in entity.GetPropertiesOfBaseTypesAndSelf())
                RefactorActionPropertyAdded(item);

            if (initialize == null)
                return;

            var values = initialize.GetType().GetProperties().ToDictionary(x => x.Name, x => x.GetValue(initialize, null));
            foreach (KeyValuePair<string, object> init in values)
            {
                Property property = DynamicEntityType.Search(init.Key);
                if (property == null)
                    throw new ArgumentException(string.Format("The property '{0}' is not contained within entity '{1}'.", init.Key, entity.Name));


                if (property.PropertyType != PropertyType.Collection)
                {
                    if (!TrySetMember(init.Key, init.Value))
                        throw new ArgumentException(string.Format("The property '{0}' is not contained within entity '{1}'.", init.Key, entity.Name));
                }
                else
                {
                    if ((object)init.Value == null)
                        continue; // or throw that you cannot delete the collection and should call the Clear method instead?

                    IEnumerable<DynamicEntity> input = init.Value as IEnumerable<DynamicEntity>;
                    if (input == null)
                        input = (init.Value as IEnumerable<object>)?.Cast<DynamicEntity>();
                    if (input == null)
                        throw new InvalidCastException("should be a collection of DynamicEntity");

                    object member;
                    if (!TryGetMember(init.Key, out member))
                        throw new ArgumentException(string.Format("The property '{0}' is not contained within entity '{1}'.", init.Key, entity.Name));

                    EntityCollection<DynamicEntity> collection = member as EntityCollection<DynamicEntity>;
                    if ((object)collection == null)
                        throw new ArgumentException(string.Format("The property '{0}' is not contained within entity '{1}'.", init.Key, entity.Name));

                    foreach (DynamicEntity item in input)
                    {
                        if (item.GetEntity() != property.EntityReturnType)
                            throw new InvalidCastException("Wrong type of object inserted in collection...");

                        collection.Add(item, false);
                    }
                }
            }
        }

        internal void RefactorActionPropertyAdded(Property item)
        {
            if (item.PropertyType == PropertyType.Attribute)
                return;

            if (item.Relationship.IsTimeDependent)
                DynamicEntityLinks.Add(item.Name, new EntityTimeCollection<DynamicEntity>(this, item));
            else
                DynamicEntityLinks.Add(item.Name, new EntityCollection<DynamicEntity>(this, item));
        }
        internal void RefactorActionPropertyRenamed(string oldname, Property item, MergeAlgorithm mergeAlgorithm = MergeAlgorithm.NotApplicable)
        {
            if (item.PropertyType == PropertyType.Attribute)
            {
                switch (mergeAlgorithm)
                {
                    case MergeAlgorithm.NotApplicable:
                    case MergeAlgorithm.ThrowOnConflict:
                        {
                            object oldvalue;
                            if (DynamicEntityValues.TryGetValue(oldname, out oldvalue))
                            {
                                DynamicEntityValues.Add(item.Name, oldvalue);
                                DynamicEntityValues.Remove(oldname);
                            }
                            break;
                        }
                    case MergeAlgorithm.PreferSource:
                        {
                            object oldvalue;
                            if (DynamicEntityValues.TryGetValue(oldname, out oldvalue))
                            {
                                if (DynamicEntityValues.ContainsKey(item.Name))
                                    DynamicEntityValues[item.Name] = oldvalue;
                                else
                                    DynamicEntityValues.Add(item.Name, oldvalue);
                                DynamicEntityValues.Remove(oldname);
                            }
                            break;
                        }
                    case MergeAlgorithm.PreferTarget:
                        {
                            if (!DynamicEntityValues.ContainsKey(item.Name))
                            {
                                object oldvalue;
                                if (DynamicEntityValues.TryGetValue(oldname, out oldvalue))
                                {
                                    DynamicEntityValues.Add(item.Name, oldvalue);
                                    DynamicEntityValues.Remove(oldname);
                                }
                            }
                            break;
                        }
                    default:
                        throw new NotImplementedException();
                }
            }
            else
            {
                switch (mergeAlgorithm)
                {
                    case MergeAlgorithm.NotApplicable:
                    case MergeAlgorithm.ThrowOnConflict:
                        {
                            EntityCollectionBase<DynamicEntity> oldvalue;
                            if (DynamicEntityLinks.TryGetValue(oldname, out oldvalue))
                            {
                                DynamicEntityLinks.Add(item.Name, oldvalue);
                                DynamicEntityLinks.Remove(oldname);
                            }
                            break;
                        }
                    case MergeAlgorithm.PreferSource:
                        {
                            EntityCollectionBase<DynamicEntity> oldvalue;
                            if (DynamicEntityLinks.TryGetValue(oldname, out oldvalue))
                            {
                                if (DynamicEntityLinks.ContainsKey(item.Name))
                                    DynamicEntityLinks[item.Name] = oldvalue;
                                else
                                    DynamicEntityLinks.Add(item.Name, oldvalue);
                                DynamicEntityLinks.Remove(oldname);
                            }
                            break;
                        }
                    case MergeAlgorithm.PreferTarget:
                        {
                            if (!DynamicEntityLinks.ContainsKey(item.Name))
                            {
                                EntityCollectionBase<DynamicEntity> oldvalue;
                                if (DynamicEntityLinks.TryGetValue(oldname, out oldvalue))
                                {
                                    DynamicEntityLinks.Add(item.Name, oldvalue);
                                    DynamicEntityLinks.Remove(oldname);
                                }
                            }
                            break;
                        }
                    default:
                        throw new NotImplementedException();
                }
            }
        }
        internal void RefactorActionPropertyConverted(Property item, Type target)
        {
            if (item.PropertyType != PropertyType.Attribute)
                throw new NotSupportedException("Only primitive properties can have their return type converted.");

            object oldValue;
            if (DynamicEntityValues.TryGetValue(item.Name, out oldValue))
            {
                object newValue = Conversion.Convert(item.SystemReturnType, target, oldValue);
                DynamicEntityValues[item.Name] = newValue;
            }
        }
        internal void RefactorActionPropertyRerouted(string oldname, Entity to, Property item)
        {
            throw new NotSupportedException("Rerouting of static data is not yet supported.");
        }
        internal void RefactorActionPropertyRemoved(Property item)
        {
            if (item.PropertyType == PropertyType.Attribute)
                DynamicEntityValues.Remove(item.Name);
            else
                DynamicEntityLinks.Remove(item.Name);
        }

        private readonly Entity DynamicEntityType;
        private readonly Dictionary<string, object> DynamicEntityValues = new Dictionary<string, object>();
        private readonly Dictionary<string, EntityCollectionBase<DynamicEntity>> DynamicEntityLinks = new Dictionary<string, EntityCollectionBase<DynamicEntity>>();
        public Guid Guid { get; private set; }
        public IReadOnlyDictionary<string, object> GetDynamicEntityValues()
        {
            return this.DynamicEntityValues;
        }
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            return TryGetMember(binder.Name, out result);
        }

        public bool TryGetMember(string propertyName, out object result)
        {
            result = null;
            Property property = DynamicEntityType.Search(propertyName);
            if ((object)property == null)
                return false;

            LazyGet();

            switch (property.PropertyType)
            {
                case PropertyType.Attribute:
                    {
                        DynamicEntityValues.TryGetValue(propertyName, out result);
                        break;
                    }
                case PropertyType.Collection:
                    {
                        EntityCollectionBase<DynamicEntity> collection;
                        DynamicEntityLinks.TryGetValue(propertyName, out collection);
                        result = collection;
                        break;
                    }
                case PropertyType.Lookup:
                    {
                        EntityCollectionBase<DynamicEntity> collection;
                        DynamicEntityLinks.TryGetValue(propertyName, out collection);
                        result = ((ILookupHelper<DynamicEntity>)collection).GetItem(null);
                        break;
                    }
            }
            return true;
        }
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            return TrySetMember(binder.Name, value);
        }
        public bool TrySetMember(string name, object value)
        {
            Property property = DynamicEntityType.Search(name);
            if ((object)property == null)
                return false;

            if (property.IsKey)
                KeyCheck();
            else
                LazySet();

            if (value == null)
            {
                if (DynamicEntityValues.ContainsKey(name))
                    DynamicEntityValues.Remove(name);
            }
            else
            {
                Type type = value.GetType();
                switch (property.PropertyType)
                {
                    case PropertyType.Attribute:
                        {
                            try
                            {
                                if (type != property.SystemReturnType)
                                    value = Convert.ChangeType(value, property.SystemReturnType);
                            }
                            catch
                            {
                                throw new InvalidCastException($"Cannot cast type '{type.Name}' to '{property.SystemReturnType.Name}'");
                            }

                            if (DynamicEntityValues.ContainsKey(name))
                                DynamicEntityValues[name] = value;
                            else
                                DynamicEntityValues.Add(name, value);

                            break;
                        }
                    case PropertyType.Collection:
                        {
                            object tmp;
                            TryGetMember(name, out tmp);
                            EntityCollectionBase<DynamicEntity> collection = tmp as EntityCollectionBase<DynamicEntity>;
                            // TODO: add nice error for tmp not being a EntityCollectionBase<DynamicEntity>

                            // TODO: add nice error for value not being a IEnumerable<DynamicEntity>
                            foreach (DynamicEntity item in (IEnumerable<DynamicEntity>)value)
                                collection.Add(item, false);

                            break;
                        }
                    case PropertyType.Lookup:
                        {
                            if (!typeof(DynamicEntity).IsAssignableFrom(type))
                                throw new InvalidCastException($"Cannot cast type '{type.Name}' to 'DynamicEntity'.");

                            DynamicEntity node = value as DynamicEntity;
                            if ((object)node != null && node.GetEntity() != property.EntityReturnType)
                                return false;

                            EntityCollectionBase<DynamicEntity> collection;
                            DynamicEntityLinks.TryGetValue(name, out collection);
                            ((ILookupHelper<DynamicEntity>)collection).SetItem(node, null);

                            break;
                        }
                }
            }

            if (property.IsKey)
                KeySet();

            return true;
        }

        internal bool ShouldExecute = false;

        void OGM.Save()
        {
            switch (PersistenceState)
            {
                case PersistenceState.New:
                case PersistenceState.HasUid:
                case PersistenceState.Loaded:
                    break;
                case PersistenceState.NewAndChanged:
                    Transaction.NodePersistenceProvider.Insert(this);
                    PersistenceState = PersistenceState.Persisted;
                    break;
                case PersistenceState.LoadedAndChanged:
                    Transaction.NodePersistenceProvider.Update(this);
                    PersistenceState = PersistenceState.Persisted;
                    break;
                case PersistenceState.Persisted:
                    break;
                case PersistenceState.Deleted:
                    Transaction.NodePersistenceProvider.Delete(this);
                    PersistenceState = PersistenceState.Persisted;
                    return;
                case PersistenceState.ForceDeleted:
                    Transaction.NodePersistenceProvider.ForceDelete(this);
                    PersistenceState = PersistenceState.Persisted;
                    return;
                case PersistenceState.OutOfScope:
                case PersistenceState.Error:
                    throw new InvalidOperationException(string.Format("The object with key '{0}' cannot be saved because it's state was {1}.", GetKey() ?? "<null>", PersistenceState.ToString()));
                default:
                    throw new NotImplementedException(string.Format("The object with key '{0}' has an invalid/unknown state {1}.", GetKey() ?? "<null>", PersistenceState.ToString()));
            }
        }

        void OGM.ValidateSave()
        {
            bool isUpdate = (PersistenceState != PersistenceState.New && PersistenceState != PersistenceState.NewAndChanged);

            object key = GetKey();
            //TODO:: Validation here
            foreach (Entity inherited in DynamicEntityType.GetBaseTypesAndSelf())
            {
                foreach (Property attr in inherited.Properties)
                {
                    if (attr.IsNodeType)
                        continue;

                    if (attr.IsKey && DynamicEntityType.FunctionalId != null)
                        continue;

                    if (attr.Nullable == false && attr.PropertyType == PropertyType.Attribute)
                    {
                        object value;
                        if (!TryGetMember(attr.Name, out value) || (object)value == null)
                            throw new PersistenceException(string.Format("Cannot save {0} with key '{1}' because the {2} cannot be null.", DynamicEntityType.Name, key, attr.Name));
                    }

                    if (attr.Nullable == false && attr.PropertyType == PropertyType.Lookup)
                    {
                        object value;
                        if (!TryGetMember(attr.Name, out value) || (object)value == null)
                            throw new PersistenceException(string.Format("Cannot save {0} with key '{1}' because the {2} cannot be null.", DynamicEntityType.Name, key, attr.Name));
                    }
                }
            }
        }

        void OGM.ValidateDelete()
        {
            //TODO:: Validation here
            object key = GetKey();
            foreach (var relationship in DynamicEntityType.Parent.Relations)
            {
                if (DynamicEntityType.IsSelfOrSubclassOf(relationship.InEntity) && relationship.OutProperty != null && relationship.OutProperty.Nullable == false)
                {
                    if (RelationshipExists(relationship.OutProperty, this))
                        throw new PersistenceException(string.Format("Cannot delete {0} with key '{1}' because it is participating in a {2} relationship.", DynamicEntityType.Name, key, relationship.Neo4JRelationshipType));
                }
                if (DynamicEntityType.IsSelfOrSubclassOf(relationship.OutEntity) && relationship.InProperty != null && relationship.InProperty.Nullable == false)
                {
                    if (RelationshipExists(relationship.InProperty, this))
                        throw new PersistenceException(string.Format("Cannot delete {0} with key '{1}' because it is participating in a {2} relationship.", DynamicEntityType.Name, key, relationship.Neo4JRelationshipType));
                }
            }
        }

        internal object GetKey()
        {
            return ((OGM)this).GetKey();
        }

        object OGM.GetKey()
        {
            object key;
            DynamicEntityValues.TryGetValue(DynamicEntityType.Key.Name, out key);

            if (Transaction == null)
            {
                Conversion converter;
                if (!PersistenceProvider.CurrentPersistenceProvider.ConvertToStoredTypeCache.TryGetValue(DynamicEntityType.Key.SystemReturnType, out converter))
                    return key;

                if ((object)converter == null)
                    return key;

                return converter.Convert(key);
            }

            return Transaction.ConvertToStoredType(DynamicEntityType.Key.SystemReturnType, key);
        }

        void OGM.SetKey(object key)
        {
            object convertedKey = Transaction.ConvertFromStoredType(DynamicEntityType.Key.SystemReturnType, key);
            if (DynamicEntityValues.ContainsKey(DynamicEntityType.Key.Name))
                DynamicEntityValues[DynamicEntityType.Key.Name] = convertedKey;
            else
                DynamicEntityValues.Add(DynamicEntityType.Key.Name, convertedKey);

            PersistenceState = PersistenceState.HasUid;

            if (ShouldExecute)
                Transaction.Current.Register(DynamicEntityType.Name, this);
        }

        private void KeyCheck()
        {
            if (PersistenceState != PersistenceState.New && PersistenceState != PersistenceState.NewAndChanged)
                throw new InvalidOperationException("You cannot set the key unless if the object is a newly created one.");

            if (HasKey)
                throw new InvalidOperationException("You cannot set the key multiple times.");
        }
        private void KeySet()
        {
            if (ShouldExecute)
                Transaction.Register(DynamicEntityType.Name, this);
        }

        private void LazySet()
        {
            if (PersistenceState == PersistenceState.Persisted)
                throw new InvalidOperationException("This object was already flushed to the data store.");
            else if (PersistenceState == PersistenceState.OutOfScope)
                throw new InvalidOperationException("The transaction for this object has already ended.");

            LazyGet();

            if (PersistenceState == PersistenceState.New)
                PersistenceState = PersistenceState.NewAndChanged;
            else if (PersistenceState == PersistenceState.Loaded)
                PersistenceState = PersistenceState.LoadedAndChanged;
        }

        private void LazyGet()
        {
            switch (PersistenceState)
            {
                case PersistenceState.New:
                case PersistenceState.NewAndChanged:
                    break;
                case PersistenceState.HasUid:
                    Transaction.NodePersistenceProvider.Load(this);
                    break;
                case PersistenceState.Loaded:
                case PersistenceState.LoadedAndChanged:
                case PersistenceState.OutOfScope:
                case PersistenceState.Persisted:
                    break;
                case PersistenceState.Deleted:
                case PersistenceState.ForceDeleted:
                    throw new InvalidOperationException("The object has been deleted, you cannot make changes to it anymore.");
                case PersistenceState.Error:
                    throw new InvalidOperationException("The object suffered an unexpected failure.");
                default:
                    throw new NotImplementedException(string.Format("The PersistenceState '{0}' is not yet implemented.", PersistenceState.ToString()));
            }
        }

        IDictionary<string, object> OGM.GetData()
        {
            Transaction current = Transaction.Current;
            return DynamicEntityValues.ToDictionary(item => item.Key, item => current.ConvertToStoredType(DynamicEntityType.Search(item.Key).SystemReturnType, item.Value));
        }

        void OGM.SetData(IReadOnlyDictionary<string, object> data)
        {
            Transaction current = Transaction.Current;
            DynamicEntityValues.Clear();
            foreach (KeyValuePair<string, object> item in data)
            {
                DynamicEntityValues.Add(item.Key, current.ConvertFromStoredType(DynamicEntityType.Search(item.Key).SystemReturnType, item.Value));
            }
        }

        void OGM.Delete(bool force)
        {
            throw new NotImplementedException();
        }

        public Entity GetEntity()
        {
            return DynamicEntityType;
        }

        public PersistenceState PersistenceState
        {
            get
            {
                return ((OGM)this).PersistenceState;
            }
            internal set
            {
                ((OGM)this).PersistenceState = value;
            }
        }

        PersistenceState OGM.PersistenceState { get; set; }

        public Transaction Transaction
        {
            get
            {
                return ((OGM)this).Transaction;
            }
            internal set
            {
                ((OGM)this).Transaction = value;
            }
        }

        Transaction OGM.Transaction { get; set; }

        public bool HasKey
        {
            get
            {
                object key = ((OGM)this).GetKey();
                return (key != null);
            }
        }

        protected bool RelationshipExists(Property foreignProperty, OGM instance)
        {
            return Transaction.NodePersistenceProvider.RelationshipExists(foreignProperty, instance);
        }

        public static dynamic Load(Entity entity, object key)
        {
            if (key == null)
                return null;

            DynamicEntity item = Lookup(entity, key);
            item.LazyGet();

            if (item.PersistenceState != PersistenceState.New && item.PersistenceState != PersistenceState.HasUid)
                return item;
            else
                return null;
        }
        public static dynamic Lookup(Entity entity, object key)
        {
            if (key == null)
                return null;

            DynamicEntity instance = (DynamicEntity)Transaction.RunningTransaction.GetEntityByKey(entity.Name, key);
            if (instance != null)
                return instance;

            DynamicEntity item = new DynamicEntity(entity, Parser.ShouldExecute);
            ((OGM)item).SetKey(key);

            return item;
        }
        public static void Delete(Entity entity, object key)
        {
            throw new NotImplementedException();
        }
    }
}
