using System;
namespace Shard.Uni
{
    public static class Constants
    {
        public static class Roles
        {
            public const string Admin = "admin";
            public const string User = "user";
            public const string Player = "player";
            public const string Shard = "shard";
        }

        public static class Fighters
        {
            public static class Health
            {
                public const int Bomber = 50;
                public const int Cruiser = 400;
                public const int Fighter = 80;
                public const int Cargo = 100;
            }

            public static class Damage
            {
                public const int Bomber = 1000;
                public const int Cruiser = 40;
                public const int Fighter = 10;
                public const int Cargo = 0;
            }

            public static class Timeout
            {
                public const int Bomber = 60;
                public const int Cruiser = 6;
                public const int Fighter = 6;
                public const int Cargo = 1000;
            }
        }
    }
}

