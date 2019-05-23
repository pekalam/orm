using System;
using System.Collections.Generic;
using System.Text;
using SimpleORM.Attributes;

namespace SimpleORM.Tests.TestUtil
{
    public class RecurrentEntities
    {
        public class Recurrent1
        {
            [Entity]
            public class Entry
            {
                [PrimaryKey]
                public int Id { get; set; }
                public int Name { get; set; }
                [ForeignKey("Class2")]
                public int Fk { get; set; }
                public Class2 Class2 { get; set; }
            }

            [Entity]
            public class Class2
            {
                [PrimaryKey]
                public int Id { get; set; }
                public int Name { get; set; }
                [ForeignKey("Class21")]
                public int Fk21 { get; set; }
                public Class21 Class21 { get; set; }
                [ForeignKey("Class22")]
                public int Fk22 { get; set; }
                public Class22 Class22 { get; set; }
            }

            [Entity]
            public class Class21
            {
                [PrimaryKey]
                public int Id { get; set; }
                public int Name { get; set; }
            }

            [Entity]
            public class Class22
            {
                [PrimaryKey]
                public int Id { get; set; }
                public int Name { get; set; }
                [ForeignKey("Recurrent")]
                public int Fk { get; set; }
                public Entry Recurrent { get; set; }
            }
        }

        public class NotRecurrent1
        {
            [Entity]
            public class Entry
            {
                [PrimaryKey]
                public int Id { get; set; }
                [ForeignKey("Foreign")]
                public int Fk { get; set; }
                public Class2 Foreign { get; set; }
            }

            [Entity]
            public class Class2
            {
                [PrimaryKey]
                public int Id { get; set; }
            }
        }

        public class Recurrent3
        {
            [Entity]
            public class Entry
            {
                [PrimaryKey]
                public int Id { get; set; }
                [ForeignKey("Foreign")]
                public int Fk { get; set; }
                public Class22 Foreign { get; set; }
            }

            [Entity]
            public class Class22
            {
                [PrimaryKey]
                public int Id { get; set; }
                [ForeignKey("Recursive")]
                public int Fk { get; set; }
                public Entry Recursive { get; set; }
            }
        }

        public class Recurrent2
        {
            [Entity]
            public class Entry
            {
                [PrimaryKey]
                public int Id { get; set; }
                [ForeignKey("Foreign")]
                public int Fk { get; set; }
                public Class222 Foreign { get; set; }
            }

            [Entity]
            public class Class222
            {
                [PrimaryKey]
                public int Id { get; set; }
                [ForeignKey("NotRecursive")]
                public int Fk { get; set; }
                public Class333 NotRecursive { get; set; }
            }


            [Entity]
            public class Class333
            {
                [PrimaryKey]
                public int Id { get; set; }
                [ForeignKey("Recursive")]
                public int Fk { get; set; }
                public Entry Recursive { get; set; }
            }
        }
    }
}
