﻿// ------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version: 17.0.0.0
//  
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
// ------------------------------------------------------------------------------
namespace Blueprint41.Neo4j.Refactoring.Templates.v5
{
    using System.Linq;
    using System.Text;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Blueprint41;
    using System;
    
    /// <summary>
    /// Class to produce the template output
    /// </summary>
    
    #line 1 "C:\Users\Glenn\source\repos\circles-arrows\blueprint41\Blueprint41\Neo4j\Refactoring\Templates\v5\CreateUniqueConstraint.tt"
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.TextTemplating", "17.0.0.0")]
    internal partial class CreateUniqueConstraint : CreateUniqueConstraintBase
    {
#line hidden
        /// <summary>
        /// Create the template output
        /// </summary>
        public override string TransformText()
        {
            
            #line 7 "C:\Users\Glenn\source\repos\circles-arrows\blueprint41\Blueprint41\Neo4j\Refactoring\Templates\v5\CreateUniqueConstraint.tt"


    Log("	executing {0} -> Create unique constraint for {1}.{2}", this.GetType().Name, Entity.Name, Property.Name);

            
            #line default
            #line hidden
            this.Write("CREATE CONSTRAINT FOR (node:");
            
            #line 11 "C:\Users\Glenn\source\repos\circles-arrows\blueprint41\Blueprint41\Neo4j\Refactoring\Templates\v5\CreateUniqueConstraint.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(Entity.Label.Name));
            
            #line default
            #line hidden
            this.Write(") REQUIRE node.");
            
            #line 11 "C:\Users\Glenn\source\repos\circles-arrows\blueprint41\Blueprint41\Neo4j\Refactoring\Templates\v5\CreateUniqueConstraint.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(Property.Name));
            
            #line default
            #line hidden
            this.Write(" IS UNIQUE\r\n");
            return this.GenerationEnvironment.ToString();
        }
    }
    
    #line default
    #line hidden
}
