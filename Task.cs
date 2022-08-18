﻿using System;
using System.Collections.Generic;
using System.Text;

namespace ExamTask
{
    internal class Task
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Header { get; set; }
        public string Description { get; set; }
        public DateTime DateTime { get; set; }
        public string Status { get; set; }

        public Task(int id, string name, string header, string description,  string status)
        {
            Id = id;
            Name = name;
            Header = header;
            Description = description;
            DateTime = DateTime.Now;
            Status = status;
        }
    }
}
