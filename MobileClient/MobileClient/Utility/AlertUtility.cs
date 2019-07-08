using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MobileClient.Utility
{
    public class AlertUtility
    {
        private readonly Func<string, string, string, string, Task<bool>> _displayWithResult;
        private readonly Func<string, string, string, Task> _display;

        public AlertUtility(Func<string, string, string, string, Task<bool>> displayWithResult,
                      Func<string, string, string, Task> displayWithoutResult)
        {
            _displayWithResult = displayWithResult;
            _display = displayWithoutResult;
        }
        
        public async Task<bool> Display(string title, string message, string accept, string cancel)
        {
            return await _displayWithResult(title, message, accept, cancel);
        }

        public async Task Display(string title, string message, string cancel)
        {
            await _display(title, message, cancel);
        }
    }
}
