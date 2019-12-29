using System;
using System.Collections.Generic;
using System.Text;

namespace Todo
{
    [Serializable]
    public class Dir : IItem
    {
        public string Name { get; set; }
        public Dir root { get; set; }
        public List<IItem> Content { get; set; }

        public Dir(string name, Dir root, List<IItem> content)
        {
            Name = name;
            this.root = root;
            Content = content;
        }

        public Dir(string name, List<IItem> content)
        {
            Name = name;
            Content = content;
            root = this;
        }

        public Dir()
        {
            Name = "";
            Content = new List<IItem>();
            root = this;
        }
    }
}
