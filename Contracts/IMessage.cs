﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts
{
    internal interface IMessage
    {
        public string ToJson();
        public IMessage FromJson(string json);
    }
}