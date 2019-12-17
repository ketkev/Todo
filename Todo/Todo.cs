using System;
using System.Collections.Generic;
using System.Text;

namespace Todo
{
    [Serializable]
    class Todo : IItem
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal PlannedHours { get; set; }
        public decimal ActualHours { get; set; }
        public bool Done { get; set; }

        public Todo(string name, string description, decimal plannedHours)
        {
            Name = name;
            Description = description;
            PlannedHours = plannedHours;
            ActualHours = 0;
            Done = false;
        }

        public Todo(string name)
        {
            Name = name;
            Description = "";
            PlannedHours = 0;
            ActualHours = 0;
            Done = false;
        }
    }
}
