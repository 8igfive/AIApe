using System;
using System.Linq;
using System.Text.Json;
using Xunit;
using Buaa.AIBot.Bot.Framework;
using System.Collections.Generic;

namespace AIBotTest.Bot.Framework
{
    public class BotStatusTest
    {
        #region MyClass definition

        public class MyClass : IEquatable<MyClass>
        {
            public class MyInnerClas : IEquatable<MyInnerClas>
            {
                public int Number { get; set; }
                public string Message { get; set; }

                public override bool Equals(object obj)
                {
                    return Equals(obj as MyInnerClas);
                }

                public bool Equals(MyInnerClas other)
                {
                    return other != null &&
                           Number == other.Number &&
                           Message == other.Message;
                }

                public override int GetHashCode()
                {
                    return HashCode.Combine(Number, Message);
                }
            }

            public MyInnerClas Inner { get; set; }
            public MyEnum Type { get; set; }

            public override bool Equals(object obj)
            {
                return Equals(obj as MyClass);
            }

            public bool Equals(MyClass other)
            {
                return other != null &&
                       EqualityComparer<MyInnerClas>.Default.Equals(Inner, other.Inner) &&
                       Type == other.Type;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Inner, Type);
            }
        }

        #endregion

        public enum MyEnum
        {
            A, B, C
        }

        #region Get equals after Put

        private void GetEqualsAfterPutTest<T>(T val)
        {
            IBotStatusContainer status = new BotStatus<int>();
            status.Put("key", val);
            var res = status.Get<T>("key");
            Assert.Equal(val, res);
        }

        [Theory]
        [InlineData("")]
        [InlineData("true")]
        [InlineData("null")]
        [InlineData("[abcd, efgh]")]
        [InlineData("123456789")]
        [InlineData("{\"msg\":true}")]
        [InlineData("\"\"")]
        public void GetEqualsAfterPut_String(string str)
        {
            GetEqualsAfterPutTest(str);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1024)]
        [InlineData(-1024)]
        public void GetEqualsAfterPut_Integer(int num)
        {
            GetEqualsAfterPutTest(num);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1e-9)]
        [InlineData(-1e-9)]
        [InlineData(1e9)]
        [InlineData(-1e9)]
        public void GetEqualsAfterPut_Float(double num)
        {
            GetEqualsAfterPutTest(num);
        }

        [Theory]
        [InlineData(default(MyEnum))]
        [InlineData(MyEnum.B)]
        public void GetEqualsAfterPut_Enum(MyEnum en)
        {
            GetEqualsAfterPutTest(en);
        }

        [Fact]
        public void GetEqualsAfterPut_Class()
        {
            var obj1 = new MyClass()
            {
                Inner = new MyClass.MyInnerClas()
                {
                    Number = 1024,
                    Message = "aa"
                },
                Type = MyEnum.A
            };
            GetEqualsAfterPutTest(obj1);

            var obj2 = new MyClass()
            {
                Inner = new MyClass.MyInnerClas()
                {
                    Number = 0,
                    Message = string.Empty
                },
                Type = MyEnum.B
            };
            GetEqualsAfterPutTest(obj2);

            var obj3 = new MyClass()
            {
                Inner = null,
                Type = MyEnum.B
            };
            GetEqualsAfterPutTest(obj3);
        }

        #endregion

        #region TryGet

        private void TryGetTest<T>(T val)
        {
            IBotStatusContainer status = new BotStatus<int>();

            T res;
            var ret1 = status.TryGet("key", out res);
            status.Put("key", val);
            var ret2 = status.TryGet("key", out res);

            Assert.Equal(val, res);
            Assert.False(ret1);
            Assert.True(ret2);
        }

        [Theory]
        [InlineData("")]
        [InlineData("true")]
        [InlineData("null")]
        [InlineData("[abcd, efgh]")]
        [InlineData("123456789")]
        [InlineData("{\"msg\":true}")]
        [InlineData("\"\"")]
        public void TryGet_String(string str)
        {
            TryGetTest(str);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1024)]
        [InlineData(-1024)]
        public void TryGet_Integer(int num)
        {
            TryGetTest(num);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1e-9)]
        [InlineData(-1e-9)]
        [InlineData(1e9)]
        [InlineData(-1e9)]
        public void TryGet_Float(double num)
        {
            TryGetTest(num);
        }

        [Theory]
        [InlineData(default(MyEnum))]
        [InlineData(MyEnum.B)]
        public void TryGet_Enum(MyEnum en)
        {
            TryGetTest(en);
        }


        [Fact]
        public void TryGet_WrongType()
        {
            IBotStatusContainer status = new BotStatus<int>();
            status.Put("key", 1);
            var ret = status.TryGet<string>("key", out _);

            Assert.False(ret);
        }

        #endregion

        #region GetOrDefault

        private void GetOrDefaultTest<T>(T val, T def)
        {
            IBotStatusContainer status = new BotStatus<int>();
            
            var ret1 = status.GetOrDefault("key", def);
            status.Put("key", val);
            var ret2 = status.GetOrDefault("key", def);

            Assert.Equal(def, ret1);
            Assert.Equal(val, ret2);
        }

        [Theory]
        [InlineData("", "1")]
        [InlineData("true", "")]
        [InlineData("null", "false")]
        [InlineData("[abcd, efgh]", "")]
        [InlineData("123456789", "")]
        [InlineData("{\"msg\":true}", "")]
        [InlineData("\"\"", "")]
        public void GetOrDefault_String(string str, string def)
        {
            GetOrDefaultTest(str, def);
        }

        [Theory]
        [InlineData(0, -1)]
        [InlineData(1024, 0)]
        [InlineData(-1024, 0)]
        public void GetOrDefault_Integer(int num, int def)
        {
            GetOrDefaultTest(num, def);
        }

        [Theory]
        [InlineData(0, 123)]
        [InlineData(1e-9, 0)]
        [InlineData(-1e-9, 0)]
        [InlineData(1e9, 0)]
        [InlineData(-1e9, 0)]
        public void GetOrDefault_Float(double num, double def)
        {
            GetOrDefaultTest(num, def);
        }

        [Theory]
        [InlineData(default(MyEnum), MyEnum.C)]
        [InlineData(MyEnum.B, MyEnum.A)]
        public void GetOrDefault_Enum(MyEnum en, MyEnum def)
        {
            GetOrDefaultTest(en, def);
        }

        [Fact]
        public void GetOrDefault_WrongType()
        {
            IBotStatusContainer status = new BotStatus<int>();
            status.Put("key", 1);
            var ret = status.GetOrDefault<string>("key", "default");

            Assert.Equal("default", ret);
        }

        #endregion

        [Fact]
        public void AsEnumableTest()
        {
            IBotStatusContainer status = new BotStatus<int>();
            var dict = new Dictionary<string, string>()
            {
                ["1"] = "msg1",
                ["2"] = "msg2",
                ["3"] = "msg3",
            };

            foreach (var pair in dict)
            {
                status.Put(pair.Key, pair.Value);
            }

            var res = new Dictionary<string, string>(
                status
                .AsEnumable()
                .Select(kv => 
                new KeyValuePair<string, string>(kv.Key, JsonSerializer.Deserialize<string>(kv.Value))));
            Assert.Equal(dict, res);
        }

        [Fact]
        public void RemoveTest()
        {
            IBotStatusContainer status = new BotStatus<int>();
            var dict = new Dictionary<string, string>()
            {
                ["1"] = "msg1",
                ["2"] = "msg2",
                ["3"] = "msg3",
            };

            foreach (var pair in dict)
            {
                status.Put(pair.Key, pair.Value);
            }
            status.Remove("1");

            int count = status.AsEnumable().Count();
            Assert.Equal(2, count);
            Assert.False(status.TryGet<string>("1", out _));
        }

        [Fact]
        public void ClearTest()
        {
            IBotStatusContainer status = new BotStatus<int>();
            var dict = new Dictionary<string, string>()
            {
                ["1"] = "msg1",
                ["2"] = "msg2",
                ["3"] = "msg3",
            };

            foreach (var pair in dict)
            {
                status.Put(pair.Key, pair.Value);
            }
            status.Clear();

            int count = status.AsEnumable().Count();
            Assert.Equal(0, count);
        }

        #region Reserve

        [Fact]
        public void Reserve_1()
        {
            IBotStatusContainer status = new BotStatus<int>();
            var dict = new Dictionary<string, string>();
            foreach (var i in Enumerable.Range(0, 10))
            {
                dict[i.ToString()] = $"msg{i}";
            }
            foreach (var pair in dict)
            {
                status.Put(pair.Key, pair.Value);
            }
            status.Reserve("1");
            int count = status.AsEnumable().Count();
            var kv = status.AsEnumable()
                .Select(kv =>
                new KeyValuePair<string, string>(kv.Key, JsonSerializer.Deserialize<string>(kv.Value)))
                .First();

            Assert.Equal(1, count);
            Assert.Equal("1", kv.Key);
            Assert.Equal("msg1", kv.Value);
        }

        [Fact]
        public void Reserve_2()
        {
            IBotStatusContainer status = new BotStatus<int>();
            var dict = new Dictionary<string, string>();
            foreach (var i in Enumerable.Range(0, 10))
            {
                dict[i.ToString()] = $"msg{i}";
            }
            foreach (var pair in dict)
            {
                status.Put(pair.Key, pair.Value);
            }
            status.Reserve("1", "3");
            var list = status.AsEnumable()
                .Select(kv =>
                new KeyValuePair<string, string>(kv.Key, JsonSerializer.Deserialize<string>(kv.Value)))
                .OrderBy(kv => int.Parse(kv.Key)).ToList();

            Assert.Equal(2, list.Count);
            Assert.Equal("1", list[0].Key);
            Assert.Equal("msg1", list[0].Value);
            Assert.Equal("3", list[1].Key);
            Assert.Equal("msg3", list[1].Value);
        }

        [Fact]
        public void Reserve_3()
        {
            IBotStatusContainer status = new BotStatus<int>();
            var dict = new Dictionary<string, string>();
            foreach (var i in Enumerable.Range(0, 10))
            {
                dict[i.ToString()] = $"msg{i}";
            }
            foreach (var pair in dict)
            {
                status.Put(pair.Key, pair.Value);
            }
            status.Reserve("1", "3", "5");
            var list = status.AsEnumable()
                .Select(kv =>
                new KeyValuePair<string, string>(kv.Key, JsonSerializer.Deserialize<string>(kv.Value)))
                .OrderBy(kv => int.Parse(kv.Key)).ToList();

            Assert.Equal(3, list.Count);
            Assert.Equal("1", list[0].Key);
            Assert.Equal("msg1", list[0].Value);
            Assert.Equal("3", list[1].Key);
            Assert.Equal("msg3", list[1].Value);
            Assert.Equal("5", list[2].Key);
            Assert.Equal("msg5", list[2].Value);
        }

        [Fact]
        public void Reserve_4()
        {
            IBotStatusContainer status = new BotStatus<int>();
            var dict = new Dictionary<string, string>();
            foreach (var i in Enumerable.Range(0, 10))
            {
                dict[i.ToString()] = $"msg{i}";
            }
            foreach (var pair in dict)
            {
                status.Put(pair.Key, pair.Value);
            }
            status.Reserve("1", "3", "5", "7");
            var list = status.AsEnumable()
                .Select(kv =>
                new KeyValuePair<string, string>(kv.Key, JsonSerializer.Deserialize<string>(kv.Value)))
                .OrderBy(kv => int.Parse(kv.Key)).ToList();

            Assert.Equal(4, list.Count);
            Assert.Equal("1", list[0].Key);
            Assert.Equal("msg1", list[0].Value);
            Assert.Equal("3", list[1].Key);
            Assert.Equal("msg3", list[1].Value);
            Assert.Equal("5", list[2].Key);
            Assert.Equal("msg5", list[2].Value);
            Assert.Equal("7", list[3].Key);
            Assert.Equal("msg7", list[3].Value);
        }

        [Fact]
        public void Reserve_5()
        {
            IBotStatusContainer status = new BotStatus<int>();
            var dict = new Dictionary<string, string>();
            foreach (var i in Enumerable.Range(0, 10))
            {
                dict[i.ToString()] = $"msg{i}";
            }
            foreach (var pair in dict)
            {
                status.Put(pair.Key, pair.Value);
            }
            status.Reserve("1", "3", "5", "7", "9");
            var list = status.AsEnumable()
                .Select(kv =>
                new KeyValuePair<string, string>(kv.Key, JsonSerializer.Deserialize<string>(kv.Value)))
                .OrderBy(kv => int.Parse(kv.Key)).ToList();

            Assert.Equal(5, list.Count);
            Assert.Equal("1", list[0].Key);
            Assert.Equal("msg1", list[0].Value);
            Assert.Equal("3", list[1].Key);
            Assert.Equal("msg3", list[1].Value);
            Assert.Equal("5", list[2].Key);
            Assert.Equal("msg5", list[2].Value);
            Assert.Equal("7", list[3].Key);
            Assert.Equal("msg7", list[3].Value);
            Assert.Equal("9", list[4].Key);
            Assert.Equal("msg9", list[4].Value);
        }

        #endregion
    }
}
