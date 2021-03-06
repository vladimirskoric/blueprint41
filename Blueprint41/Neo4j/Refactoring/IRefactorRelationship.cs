﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blueprint41.Neo4j.Refactoring
{
    public interface IRefactorRelationship
    {
        void Rename(string newName, string alias = null);

        void SetInEntity(Entity target);
        void SetOutEntity(Entity target);

        void RemoveTimeDependance();

        void Merge(Relationship target);

        void Deprecate();
    }
}
