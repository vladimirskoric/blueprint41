﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace Blueprint41.Core
{
    public abstract class Data
    {
        protected Data()
        {
            UnidentifiedProperties = null;
        }

        public void Initialize(OGM parent)
        {
            if (Wrapper != null)
                throw new NotSupportedException("You should not call this method... Leave it to the professionals... Greetzz from the A Team.");

            Wrapper = parent;
            InitializeCollections();

            bool hasUnidentifiedProperties = !string.IsNullOrEmpty(Wrapper.GetEntity().UnidentifiedProperties);
            if (hasUnidentifiedProperties)
                UnidentifiedProperties = new UnidentifiedPropertyCollection(Wrapper);
        }
        protected abstract void InitializeCollections();

        public OGM Wrapper { get; private set; }

        private static Dictionary<string, Dictionary<string, PropertyDetail>> s_PropertyDetails = new Dictionary<string, Dictionary<string, Data.PropertyDetail>>();
        protected Dictionary<string, PropertyDetail> PropertyDetails
        {
            get
            {
                PersistenceProvider factory = Transaction.RunningTransaction?.PersistenceProviderFactory;

                Entity entity = Wrapper.GetEntity();
                
                Dictionary<string, PropertyDetail> propertyDetails;
                if (!Data.s_PropertyDetails.TryGetValue(entity.Name, out propertyDetails))
                {
                    lock (Data.s_PropertyDetails)
                    {
                        if (!Data.s_PropertyDetails.TryGetValue(entity.Name, out propertyDetails))
                        {
                            propertyDetails = new Dictionary<string, PropertyDetail>();

                            PropertyInfo[] infos = this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
                            foreach (Property property in entity.GetPropertiesOfBaseTypesAndSelf())
                            {
                                PropertyInfo info = infos.FirstOrDefault(item => item.Name == property.Name);
                                if (info == null)
                                    throw new MissingFieldException("The field {0} does not exist, please regenerate the data access classes.");

                                if (!typeof(EntityCollectionBase).IsAssignableFrom(info.PropertyType) && !typeof(OGM).IsAssignableFrom(info.PropertyType))
                                    propertyDetails.Add(info.Name, new PropertyDetail(factory.SupportedTypeMappings, info));
                            }
                            Data.s_PropertyDetails.Add(entity.Name, propertyDetails);
                        }
                    }
                }
                return propertyDetails;
            }
        }

        public virtual void MapFrom(IReadOnlyDictionary<string, object> properties)
        {
            bool hasUnidentifiedProperties = !string.IsNullOrEmpty(Wrapper.GetEntity().UnidentifiedProperties);
            if (hasUnidentifiedProperties)
            {
                if (UnidentifiedProperties == null)
                    UnidentifiedProperties = new UnidentifiedPropertyCollection(Wrapper);
                else 
                    UnidentifiedProperties.ClearInternal();
            }
            foreach (KeyValuePair<string, object> property in properties)
            {
                PropertyDetail info;
                if (PropertyDetails.TryGetValue(property.Key, out info))
                    info.SetValue(this, property.Value);
                else
                    if (hasUnidentifiedProperties)
                        UnidentifiedProperties.AddInternal(property.Key, property.Value);
            }
        }

        public virtual IDictionary<string, object> MapTo()
        {
            IDictionary<string, object> dictionary = new Dictionary<string, object>();

            foreach (PropertyDetail info in PropertyDetails.Values)
            {
                object value = info.GetValue(this);
                if (value != null)
                {
                    dictionary.Add(info.PropertyName, info.GetValue(this));
                }
            }

            if (!string.IsNullOrEmpty(Wrapper.GetEntity().UnidentifiedProperties))
                UnidentifiedProperties.ForEachInternal(item => dictionary.Add(item));

            return dictionary;
        }

        public object GetValue(string propertyName, bool throwWhenMissing = false)
        {

            PropertyDetail info;
            if (PropertyDetails.TryGetValue(propertyName, out info))
            {
                return info.GetValue(this);
            }
            else
            {
                if (!string.IsNullOrEmpty(Wrapper.GetEntity().UnidentifiedProperties))
                {
                    object value;
                    if (UnidentifiedProperties.TryGetValue(propertyName, out value))
                        return value;
                }
            }

            if (throwWhenMissing)
                throw new KeyNotFoundException(string.Format("A property with name '{0}' does not exist.", propertyName));

            return null;
        }

        internal PersistenceState PersistenceState = PersistenceState.New;

        protected class PropertyDetail
        {
            internal PropertyDetail(IEnumerable<TypeMapping> supportedTypeMappings, PropertyInfo info)
            {
                PropertyName = info.Name;
                Mapping = supportedTypeMappings.FirstOrDefault(item => item.ReturnType == info.PropertyType);

                GetValue = BuildGetAccessor(info);
                SetValue = BuildSetAccessor(info);
            }

            public Func<Data, object> GetValue;
            public Action<Data, object> SetValue;

            public string PropertyName { get; private set; }
            public TypeMapping Mapping { get; private set; }

            private Func<Data, object> BuildGetAccessor(PropertyInfo propertyInfo)
            {
                if (Mapping == null)
                    return delegate (Data d)
                    {
                        throw new NotSupportedException(string.Format("The property type '{0}' is not supported by the storage provider.", propertyInfo.PropertyType.Name));
                    };

                var instance = Expression.Parameter(typeof(Data), "i");
                var cast = Expression.TypeAs(instance, propertyInfo.DeclaringType);
                var property = Expression.Property(cast, propertyInfo);
                var convertedValue = Mapping.NeedsConversion ?
                                        (Expression)Expression.Call(Mapping.GetGetConverter(), property) :
                                        (Expression)property;
                var value = Expression.TypeAs(convertedValue, typeof(object));

                return (Func<Data, object>)Expression.Lambda(value, instance).Compile();
            }
            private Action<Data, object> BuildSetAccessor(PropertyInfo propertyInfo)
            {
                if (Mapping == null)
                    return delegate (Data d, object o)
                    {
                        throw new NotSupportedException(string.Format("The property type '{0}' is not supported by the storage provider.", propertyInfo.PropertyType.Name));
                    };

                MethodInfo method = propertyInfo.GetSetMethod(true);

                var obj = Expression.Parameter(typeof(Data), "o");
                var value = Expression.Parameter(typeof(object));
                var valueTypesafe = Expression.Convert(value, Mapping.PersistedType);

                var valueWithConversion = Mapping.NeedsConversion ?
                                            (Expression)Expression.Call(Mapping.GetSetConverter(), valueTypesafe) :
                                            (Expression)valueTypesafe;

                Expression<Action<Data, object>> expr =
                    Expression.Lambda<Action<Data, object>>(
                        Expression.Call(
                            Expression.Convert(obj, method.DeclaringType),
                            method,
                            valueWithConversion),
                        obj,
                        value);

                return expr.Compile();
            }
        }

        internal protected abstract void SetKey(object key);

        public UnidentifiedPropertyCollection UnidentifiedProperties { get; set; }
    }
 }
