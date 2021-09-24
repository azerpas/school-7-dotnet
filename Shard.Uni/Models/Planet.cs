using System;
namespace Shard.Uni.Models
{
    /**
     * Class reprensenting a Planet
     */
    public class Planet
    {
        public string Name { get; }
        public int Size { get; }

        public Planet(string name, int size)
        {
            Name = name;
            Size = size;
        }
    }
}
