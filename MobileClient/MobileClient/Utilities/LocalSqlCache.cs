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

        public LocalSqlCache(string path, ILogger<LocalSqlCache<T>> logger)
        {
            _path = path;
            _logger = logger;
            _tableName = typeof(Storable).Name.ToString();
            _connection = new SQLiteConnection(path);
            _connection.CreateTable<Storable>(CreateFlags.None);
        }

        public void Delete(string key)
        {
            _connection.Delete<Storable>(key);
        }
        public T Get(string key)
        {
            var stored = _connection.Get<Storable>(key);
            return JsonConvert.DeserializeObject<T>(stored.SerializedValue);
        }
        public Dictionary<string, T> GetAll()
        {
            var rows = _connection.Query<Storable>($"SELECT * FROM [{_tableName}]");
            return rows.ToDictionary(x => x.Key, y => JsonConvert.DeserializeObject<T>(y.SerializedValue));
        }
        public void Put(string key, T value)
        {
            this.Delete(key);
            _connection.Insert(new Storable()
            {
                Key = key,
                SerializedValue = JsonConvert.SerializeObject(value)
            });
        }
        public void Put(Dictionary<string, T> keyValuePairs)
        {
            _connection.DeleteAll<T>();
            foreach (var kvp in keyValuePairs)
            {
                _connection.Insert(new Storable()
                {
                    Key = kvp.Key,
                    SerializedValue = JsonConvert.SerializeObject(kvp.Value)
                });
            }
        }
        public void Update(Dictionary<string, T> keyValuePairs)
        {
            foreach (var kvp in keyValuePairs)
            {
                var value = _connection.Find<Storable>(kvp.Key);
                if (value != null)
                {
                    value.SerializedValue = JsonConvert.SerializeObject(kvp.Value);
                    _connection.Update(value);
                }
                else
                {
                    _connection.Insert(new Storable() { Key = kvp.Key, SerializedValue = JsonConvert.SerializeObject(kvp.Value) });
                }
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
