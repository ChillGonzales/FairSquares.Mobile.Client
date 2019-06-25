using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace MobileClient.Utilities
{
    public class LocalSqlCache<T> : ICache<T> where T : new()
    {
        private readonly string _path;
        private readonly ILogger<LocalSqlCache<T>> _logger;
        private readonly SQLiteConnection _connection;
        private readonly string _tableName;
        private readonly object _syncLock;

        public LocalSqlCache(string path, ILogger<LocalSqlCache<T>> logger)
        {
            _path = path;
            _logger = logger;
            _tableName = typeof(Storable).Name.ToString();
            _connection = new SQLiteConnection(path);
            _connection.CreateTable<Storable>(CreateFlags.None);
            _syncLock = new object();
        }

        public void Delete(string key)
        {
            try
            {
                lock (_syncLock)
                {
                    _connection.Delete<Storable>(key);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to delete object with key '{key}' from table.", ex);
            }
        }
        public void Clear()
        {
            try
            {
                lock (_syncLock)
                {
                    _connection.DeleteAll<Storable>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to clear table.", ex);
            }
        }
        public T Get(string key)
        {
            try
            {
                lock (_syncLock)
                {
                    var stored = _connection.Find<Storable>(key);
                    return stored == null ? default(T) : JsonConvert.DeserializeObject<T>(stored.SerializedValue);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to get object with key '{key}' from table.", ex);
                return default(T);
            }
        }
        public Dictionary<string, T> GetAll()
        {
            try
            {
                lock (_syncLock)
                {
                    var rows = _connection.Query<Storable>($"SELECT * FROM [{_tableName}]");
                    return rows.ToDictionary(x => x.Key, y => JsonConvert.DeserializeObject<T>(y.SerializedValue));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to get all rows from table.", ex);
                return default(Dictionary<string, T>);
            }
        }
        public void Put(string key, T value)
        {
            try
            {
                lock (_syncLock)
                {
                    this.Delete(key);
                    _connection.Insert(new Storable()
                    {
                        Key = key,
                        SerializedValue = JsonConvert.SerializeObject(value)
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception occurred while putting local cache.", ex);
            }
        }
        public void Put(Dictionary<string, T> keyValuePairs)
        {
            try
            {
                lock (_syncLock)
                {
                    _connection.DeleteAll<Storable>();
                    foreach (var kvp in keyValuePairs)
                    {
                        _connection.Insert(new Storable()
                        {
                            Key = kvp.Key,
                            SerializedValue = JsonConvert.SerializeObject(kvp.Value)
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception occurred while putting local cache.", ex);
            }
        }
        public void Update(Dictionary<string, T> keyValuePairs)
        {
            try
            {
                lock (_syncLock)
                {
                    foreach (var kvp in keyValuePairs)
                    {
                        var value = _connection.Find<Storable>(kvp.Key);
                        if (value != null)
                            this.Delete(kvp.Key);
                        _connection.Insert(new Storable() { Key = kvp.Key, SerializedValue = JsonConvert.SerializeObject(kvp.Value) });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception occurred while updating local cache.", ex);
            }
        }
    }

    internal class Storable
    {
        [PrimaryKey]
        public string Key { get; set; }
        public string SerializedValue { get; set; }
    }
}
