using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace VetClinicServer
{
    [Serializable]
    internal class Animal
    {
        public Animal()
        {
            
        }
        public Animal(int id, string name, string type, DateTime birthday, int owner, Byte[] image)
        {
            Id = id;
            Name = name;
            Type = type;
            Birthday = birthday;
            Owner = owner;
            Image = image;
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public string Type { get; set; }

        public DateTime Birthday { get; set; }

        public int Owner { get; set; }
        public Byte[] Image { get; set; }
    }
}
