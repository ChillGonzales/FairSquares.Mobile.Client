﻿using System;
using System.Collections.Generic;
using System.Text;

namespace MobileClient.Utilities
{
    public interface ICache<T>
    {
        Dictionary<string, T> GetAll();
        T Get(string key);
        void Put(string key, T value);
        void Delete(string key);
        void Clear();
        void Put(Dictionary<string, T> keyValuePairs);
        void Update(Dictionary<string, T> keyValuePairs);
    }
}
