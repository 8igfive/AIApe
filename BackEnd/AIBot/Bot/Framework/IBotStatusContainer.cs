using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections;

namespace Buaa.AIBot.Bot.Framework
{
    /// <summary>
    /// The container of bot's status.
    /// </summary>
    /// <remarks>
    /// Using key-value pair to store the satus of bot.
    /// Every value should be able to be dumped by <see cref="JsonSerializer"/>, and can be load from it.
    /// </remarks>
    public interface IBotStatusContainer
    {
        int UserId { get; }

        IEnumerable<KeyValuePair<string, string>> AsEnumable();

        /// <summary>
        /// Store an boject.
        /// </summary>
        /// <remarks><paramref name="value"/> should be able to be dumped by <see cref="JsonSerializer"/>.</remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        void Put<T>(string key, T value);

        /// <summary>
        /// Get an object.
        /// </summary>
        /// <typeparam name="T">The Type of the value.</typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        T Get<T>(string key);

        /// <summary>
        /// Try get an object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns>true if object exist.</returns>
        bool TryGet<T>(string key, out T value);

        /// <summary>
        /// If contains the key, return the object, or return <paramref name="defaultValue"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        T GetOrDefault<T>(string key, T defaultValue);

        /// <summary>
        /// Remove an object by key.
        /// </summary>
        /// <param name="key"></param>
        void Remove(string key);
        /// <summary>
        /// Remove all objects.
        /// </summary>
        void Clear();
        /// <summary>
        /// Remove all other keys except given keys.
        /// </summary>
        /// <param name="key"></param>
        void Reserve(string key);
        /// <summary>
        /// Remove all other keys except given keys.
        /// </summary>
        /// <param name="key1"></param>
        /// <param name="key2"></param>
        void Reserve(string key1, string key2);
        /// <summary>
        /// Remove all other keys except given keys.
        /// </summary>
        /// <param name="key1"></param>
        /// <param name="key2"></param>
        /// <param name="key3"></param>
        void Reserve(string key1, string key2, string key3);
        /// <summary>
        /// Remove all other keys except given keys.
        /// </summary>
        /// <param name="key1"></param>
        /// <param name="key2"></param>
        /// <param name="key3"></param>
        /// <param name="keys"></param>
        void Reserve(string key1, string key2, string key3, params string[] keys);
    }

    /// <summary>
    /// The Status of a Bot.
    /// </summary>
    /// <typeparam name="IdType">The type to mark a status, usually an enum.</typeparam>
    public class BotStatus<IdType> : IBotStatusContainer
    {
        public int UserId { get; set; }

        public string this[string key]
        {
            get => Items[key];
            set => Items[key] = value;
        }

        /// <summary>
        /// The status of Bot.
        /// </summary>
        public IdType Status { get; set; }

        /// <summary>
        /// Stored key-values of this bot.
        /// </summary>
        /// <remarks>
        /// The values have already dumped to JSON, if you need clr object, using <see cref="BotStatus{IdType}.Get{T}(string)"/>
        /// </remarks>
        public Dictionary<string, string> Items { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Store an boject.
        /// </summary>
        /// <remarks><paramref name="value"/> should be able to be dumped by <see cref="JsonSerializer"/>.</remarks>
        /// <seealso cref="IBotStatusContainer{IdType}.Get{T}(string)"/>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public T Get<T>(string key)
        {
            return JsonSerializer.Deserialize<T>(Items[key]);
        }

        /// <summary>
        /// Try get an object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns>true if object exist.</returns>
        public bool TryGet<T>(string key, out T value)
        {
            try
            {
                if (Items.TryGetValue(key, out string json))
                {
                    value = JsonSerializer.Deserialize<T>(json);
                    return true;
                }
            }
            catch (Exception) { }
            value = default(T);
            return false;
        }

        /// <summary>
        /// If contains the key, return the object, or return <paramref name="defaultValue"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public T GetOrDefault<T>(string key, T defaultValue)
        {
            try
            {
                if (Items.TryGetValue(key, out string json))
                {
                    return JsonSerializer.Deserialize<T>(json);
                }
            }
            catch (Exception) { }
            return defaultValue;
        }

        /// <summary>
        /// Get an object.
        /// </summary>
        /// <seealso cref="IBotStatusContainer{IdType}.Put{T}(string, T)"/>
        /// <typeparam name="T">The Type of the value.</typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public void Put<T>(string key, T value)
        {
            Items[key] = JsonSerializer.Serialize(value);
        }

        /// <summary>
        /// Remove an object by key.
        /// </summary>
        /// <param name="key"></param>
        public void Remove(string key)
        {
            Items.Remove(key);
        }

        /// <summary>
        /// Remove all objects.
        /// </summary>
        public void Clear()
        {
            Items.Clear();
        }

        /// <summary>
        /// Remove all other keys except given keys.
        /// </summary>
        /// <param name="key"></param>
        public void Reserve(string key)
        {
            var tmp = new Dictionary<string, string>() 
            { 
                [key] = Items[key]
            };
            Items = tmp;
        }

        /// <summary>
        /// Remove all other keys except given keys.
        /// </summary>
        /// <param name="key1"></param>
        /// <param name="key2"></param>
        public void Reserve(string key1, string key2)
        {
            var tmp = new Dictionary<string, string>(2)
            {
                [key1] = Items[key1],
                [key2] = Items[key2]
            };
            Items = tmp;
        }

        /// <summary>
        /// Remove all other keys except given keys.
        /// </summary>
        /// <param name="key1"></param>
        /// <param name="key2"></param>
        /// <param name="key3"></param>
        public void Reserve(string key1, string key2, string key3)
        {
            var tmp = new Dictionary<string, string>(3)
            {
                [key1] = Items[key1],
                [key2] = Items[key2],
                [key3] = Items[key3]
            };
            Items = tmp;
        }

        /// <summary>
        /// Remove all other keys except given keys.
        /// </summary>
        /// <param name="key1"></param>
        /// <param name="key2"></param>
        /// <param name="key3"></param>
        /// <param name="keys"></param>
        public void Reserve(string key1, string key2, string key3, params string[] keys)
        {
            var tmp = new Dictionary<string, string>(keys.Length + 3)
            {
                [key1] = Items[key1],
                [key2] = Items[key2],
                [key3] = Items[key3]
            };
            foreach (var k in keys)
            {
                tmp[k] = Items[k];
            }
            Items = tmp;
        }

        public IEnumerable<KeyValuePair<string, string>> AsEnumable()
        {
            return Items;
        }
    }
}
