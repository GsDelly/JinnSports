﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JinnSports.Parser.App.JsonParserService.JsonEntities
{
    public abstract class JsonObject
    {
        public virtual int Id { get; set; }

        public string Name { get; set; }
    }
}
