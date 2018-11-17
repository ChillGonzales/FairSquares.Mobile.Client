﻿using MobileClient.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace MobileClient.Services
{
    public class DebugLogger<T> : ILogger<T>
    {
        public DebugLogger()
        { }

        public void LogError(string message)
        {
            Debug.WriteLine($"Error logged from class: {typeof(T).Name}. Message: {message}");
        }
    }
}
