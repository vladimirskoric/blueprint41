﻿using Blueprint41.Core;
using Neo4j.Driver.V1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blueprint41.Neo4j.Persistence
{
    public class Neo4JPersistenceProvider : PersistenceProvider
    {
        internal const decimal DECIMAL_FACTOR = 100000000m;

        private IDriver driver;
        public IDriver Driver
        {
            get
            {
                if(driver == null)
                {
                    lock (typeof(Neo4JPersistenceProvider))
                    {
                        if(driver == null)
                            driver = GraphDatabase.Driver(Uri, AuthTokens.Basic(Username, Password));
                    }
                }
                return driver;
            }
        }
        private string Uri;
        private string Username;
        private string Password;

        public Neo4JPersistenceProvider(string uri, string username, string password)
        {
            Uri = uri;
            Username = username;
            Password = password;
            //this.Driver = GraphDatabase.Driver(uri, AuthTokens.Basic(username, password));
        }

        internal override NodePersistenceProvider GetNodePersistenceProvider()
        {
            return new Neo4JNodePersistenceProvider(this);
        }

        internal override RelationshipPersistenceProvider GetRelationshipPersistenceProvider()
        {
            return new Neo4JRelationshipPersistenceProvider(this);
        }

        internal override Transaction NewTransaction(bool withTransaction)
        {
            return new Neo4jTransaction(Driver, withTransaction);
        }

        public override IEnumerable<TypeMapping> SupportedTypeMappings
        {
            get
            {
                return supportedTypeMappings;
            }
        }

        public static readonly List<TypeMapping> supportedTypeMappings = new List<TypeMapping>()
        {
            new TypeMapping(typeof(bool), typeof(bool), "b"),
            new TypeMapping(typeof(bool?), typeof(bool?), "b"),
            new TypeMapping(typeof(sbyte), typeof(long), "i"),
            new TypeMapping(typeof(sbyte?), typeof(long?), "i"),
            new TypeMapping(typeof(short), typeof(long), "i"),
            new TypeMapping(typeof(short?), typeof(long?), "i"),
            new TypeMapping(typeof(int), typeof(long), "i"),
            new TypeMapping(typeof(int?), typeof(long?), "i"),
            new TypeMapping(typeof(long), typeof(long), "i"),
            new TypeMapping(typeof(long?), typeof(long?), "i"),
            new TypeMapping(typeof(float), typeof(double),"f"),
            new TypeMapping(typeof(float?), typeof(double?),"f"),
            new TypeMapping(typeof(double), typeof(double),"f"),
            new TypeMapping(typeof(double?), typeof(double?),"f"),
            new TypeMapping(typeof(char), typeof(string),"s"),
            new TypeMapping(typeof(char?), typeof(string),"s"),
            new TypeMapping(typeof(string), typeof(string),"s"),
            new TypeMapping(typeof(DateTime), typeof(long),"dt"),
            new TypeMapping(typeof(DateTime?), typeof(long?),"dt"),
            new TypeMapping(typeof(Guid), typeof(string),"s"),
            new TypeMapping(typeof(Guid?), typeof(string),"s"),
            new TypeMapping(typeof(decimal), typeof(long),"d"),
            new TypeMapping(typeof(decimal?), typeof(long?),"d"),
            // lists...
            new TypeMapping(typeof(List<string>), typeof(List<object>), "l_s"),
        };
    }
}
/*

Neo4j type  Dotnet type Description                                                 Value range
----------- ----------- ----------------------------------------------------------- ------------------------------------------------------
boolean     bool        bit                                                         true/false
byte        sbyte       8-bit integer                                               -128 to 127, inclusive
short       short       16-bit integer                                              -32768 to 32767, inclusive
int         int         32-bit integer                                              -2147483648 to 2147483647, inclusive
long        long        64-bit integer                                              -9223372036854775808 to 9223372036854775807, inclusive
float       float       32-bit IEEE 754 floating-point number
double      double      64-bit IEEE 754 floating-point number
char        char        16-bit unsigned integers representing Unicode characters    u0000 to uffff(0 to 65535)
String      string      sequence of Unicode characters

*/
