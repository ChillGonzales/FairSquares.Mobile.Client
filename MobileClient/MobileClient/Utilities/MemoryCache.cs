using FairSquares.Measurement.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MobileClient.Utilities
{
    public class MemoryCache<T> : ICache<T>
    {
        private Dictionary<string, T> _cached;
        private readonly ILogger<MemoryCache<T>> _logger;

        public MemoryCache(ILogger<MemoryCache<T>> logger)
        {
            _cached = new Dictionary<string, T>();
            _logger = logger;
        }

        public void Delete(string key)
        {
            try
            {
                if (_cached.ContainsKey(key))
                    _cached.Remove(key);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting cached value for key '{key}'. {ex.ToString()}");
                throw;
            }
        }

        public void Clear()
        {
            try
            {
                _cached.Clear();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error clearing cache.", ex);
                throw;
            }
        }

        public Dictionary<string, T> GetAll()
        {
            try
            {
                return _cached;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting all cached values. {ex.ToString()}");
                throw;
            }
        }

        public T Get(string key)
        {
            try
            {
                if (_cached.ContainsKey(key))
                    return _cached[key];
                return default(T);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting cached value for key '{key}'. {ex.ToString()}");
                throw;
            }
        }

        public void Put(string key, T value)
        {
            try
            {
                _cached[key] = value;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error replacing cached value for key '{key}'. {ex.ToString()}");
                throw;
            }
        }

        public void Put(Dictionary<string, T> keyValuePairs)
        {
            try
            {
                _cached = keyValuePairs;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error replacing cache. {ex.ToString()}");
                throw;
            }
        }

        public void Update(Dictionary<string, T> keyValuePairs)
        {
            try
            {
                foreach (var kvp in keyValuePairs)
                {
                    _cached[kvp.Key] = kvp.Value;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating cache.", ex.ToString());
                throw;
            }
        }
    }
}
