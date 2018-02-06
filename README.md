# blueprint41

An Object Graph Mapper for CSharp and Neo4j. It has support for defining the object model as a schema. It support refactor scripts written in CSharp, which you can add to your project. When you run your program and the graph is of an older version, the upgrade script will automatically be executed against the graph. It also support generation of type-safe data objects, so you know at compile time if your code is compatible with the latest upgrades.