﻿using Blueprint41.Modeller.Schemas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model = Blueprint41.Modeller.Schemas.Modeller;
namespace Blueprint41.Modeller
{
    public static class Extensions
    {
        public static List<Relationship> GetRelationships(this Submodel model, bool includeInherited = false)
        {
            List<Relationship> relationships = model.Model.Relationships.Relationship.Where(item => model.Node.Any(entity => entity.Label == item.Source.Label) && model.Node.Any(entity => entity.Label == item.Target.Label)).ToList();

            if (includeInherited == false)
                return relationships;

            foreach (var node in model.Node)
            {
                Dictionary<RelationshipDirection, List<Relationship>> inheritedPropertyByDirection = GetInheritedRelationships(node.Entity, model);
                foreach (var item in inheritedPropertyByDirection[RelationshipDirection.In])
                {
                    Relationship relationship = new Relationship(model.Model, (relationship)item.Xml.Clone());
                    relationship.Source.Label = node.Label;
                    relationships.Add(relationship);
                    model.CreatedInheritedRelationships.Add(relationship);
                }
                foreach (var item in inheritedPropertyByDirection[RelationshipDirection.Out])
                {
                    Relationship relationship = new Relationship(model.Model, (relationship)item.Xml.Clone());
                    relationship.Target.Label = node.Label;
                    relationships.Add(relationship);
                    model.CreatedInheritedRelationships.Add(relationship);
                }
            }

            return relationships;
        }

        public static Dictionary<RelationshipDirection, List<Relationship>> GetInheritedRelationships(this Entity entity, Submodel model)
        {
            Dictionary<RelationshipDirection, List<Relationship>> result = new Dictionary<RelationshipDirection, List<Schemas.Relationship>>();
            result.Add(RelationshipDirection.In, new List<Schemas.Relationship>());
            result.Add(RelationshipDirection.Out, new List<Schemas.Relationship>());
            Entity current = entity.ParentEntity;
            while (current != null)
            {
                result[RelationshipDirection.In].AddRange(model.Model.Relationships.Relationship.Where(item => item.Source.Label == current.Label && model.Node.Any(node => node.Label == item.Target.Label)));
                result[RelationshipDirection.Out].AddRange(model.Model.Relationships.Relationship.Where(item => item.Target.Label == current.Label && model.Node.Any(node => node.Label == item.Source.Label)));
                current = current.ParentEntity;
            }

            return result;
        }

        public static List<Relationship> GetCurrentRelationshipsInGraph(this Entity entity, Submodel model)
        {
            List<Relationship> relationships = model.Model.Relationships.Relationship.Where(item => item.Source.ReferenceGuid == entity.Guid || item.Target.ReferenceGuid == entity.Guid).ToList();

            foreach (Relationship relationship in model.CreatedInheritedRelationships.Where(rel => rel.InEntity == entity.Label || rel.OutEntity == entity.Label))
            {
                relationships.Add(relationship);
            }
            
            return relationships;
        }
        
    }
}