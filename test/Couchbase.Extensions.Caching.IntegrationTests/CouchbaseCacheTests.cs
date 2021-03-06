﻿using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Couchbase.Extensions.Caching.IntegrationTests.Infrastructure;
using Couchbase.IO;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Couchbase.Extensions.Caching.IntegrationTests
{
    [TestFixture]
    public class CouchbaseCacheTests
    {
        [OneTimeSetUp]
        public void SetUp()
        {
            ClusterHelper.Initialize(TestConfiguration.GetCurrentConfiguration());
        }

        [Test]
        public void Test_Set()
        {
            var cache = GetCache();

            var poco = new Poco {Name = "poco1", Age = 12};
            const string key = "CouchbaseCacheTests.Test_Set";
            var bucket = ClusterHelper.GetBucket("default");
            bucket.Remove(key);

            cache.Set(key, GetBytes(poco), null);

            var result = bucket.Get<byte[]>(key);
            var actual = GetObject<Poco>(result.Value);

            Assert.AreEqual(actual.ToString(), poco.ToString());
        }

        [Test]
        public async Task Test_SetAsync()
        {
            var cache = GetCache();

            var poco = new Poco { Name = "poco1", Age = 12 };
            const string key = "CouchbaseCacheTests.Test_SetAsync";
            var bucket = ClusterHelper.GetBucket("default");
            bucket.Remove(key);

            await cache.SetAsync(key, GetBytes(poco), null);

            var result = await bucket.GetAsync<byte[]>(key);
            var actual = GetObject<Poco>(result.Value);

            Assert.AreEqual(actual.ToString(), poco.ToString());
        }

        [Test]
        public void Test_Get()
        {
            var cache = GetCache();

            var poco = new Poco { Name = "poco1", Age = 12 };
            const string key = "CouchbaseCacheTests.Test_Get";
            var bucket = ClusterHelper.GetBucket("default");
            bucket.Remove(key);

            var result = bucket.Insert(key, GetBytes(poco));
            Assert.IsTrue(result.Success);

            var bytes = cache.Get(key);
            var actual = GetObject<Poco>(bytes);

            Assert.AreEqual(actual.ToString(), poco.ToString());
        }

        [Test]
        public async Task Test_GetAsync()
        {
            var cache = GetCache();

            var poco = new Poco { Name = "poco1", Age = 12 };
            const string key = "CouchbaseCacheTests.Test_GetAsync";
            var bucket = ClusterHelper.GetBucket("default");
            bucket.Remove(key);

            var result = bucket.Insert(key, GetBytes(poco));
            Assert.IsTrue(result.Success);

            var bytes = await cache.GetAsync(key);
            var actual = GetObject<Poco>(bytes);

            Assert.AreEqual(actual.ToString(), poco.ToString());
        }

        [Test]
        public void Test_Remove()
        {
            var cache = GetCache();

            var poco = new Poco { Name = "poco1", Age = 12 };
            const string key = "CouchbaseCacheTests.Test_Remove";
            var bucket = ClusterHelper.GetBucket("default");
            bucket.Remove(key);

            var result = bucket.Insert(key, GetBytes(poco));
            Assert.IsTrue(result.Success);

            cache.Remove(key);

            var actual = bucket.Get<byte[]>(key);

            Assert.AreEqual(actual.Status, ResponseStatus.KeyNotFound);
        }

        [Test]
        public async Task Test_RemoveAsync()
        {
            var cache = GetCache();

            var poco = new Poco { Name = "poco1", Age = 12 };
            const string key = "CouchbaseCacheTests.Test_RemoveAsync";
            var bucket = ClusterHelper.GetBucket("default");
            bucket.Remove(key);

            var result = await bucket.InsertAsync(key, GetBytes(poco));
            Assert.IsTrue(result.Success);

            await cache.RemoveAsync(key);

            var actual = await bucket.GetAsync<byte[]>(key);

            Assert.AreEqual(actual.Status, ResponseStatus.KeyNotFound);
        }

        [Test]
        public void Test_Refresh()
        {
            var cache = GetCache();

            var poco = new Poco { Name = "poco1", Age = 12 };
            const string key = "CouchbaseCacheTests.Test_Refresh";
            var bucket = ClusterHelper.GetBucket("default");
            bucket.Remove(key);

            var v = bucket.Insert(key, GetBytes(poco), new TimeSpan(0, 0, 0, 2));

            cache.Refresh(key);

            Thread.Sleep(2000);
            var result = bucket.Get<byte[]>(key);
            Assert.AreEqual(ResponseStatus.Success, result.Status);
        }

        [Test]
        public async Task Test_RefreshAsync()
        {
            var cache = GetCache();

            var poco = new Poco { Name = "poco1", Age = 12 };
            const string key = "CouchbaseCacheTests.Test_RefreshAsync";
            var bucket = ClusterHelper.GetBucket("default");
            await bucket.RemoveAsync(key);

            var v = await bucket.InsertAsync(key, GetBytes(poco), new TimeSpan(0, 0, 0, 2));

            await cache.RefreshAsync(key);

            Thread.Sleep(2000);
            var result = await bucket.GetAsync<byte[]>(key);
            Assert.AreEqual(ResponseStatus.Success, result.Status);
        }

        [Test]
        public void Test_Refresh_WithTimeSpan()
        {
            var cache = GetCache(new TimeSpan(0,0,0,4));

            var poco = new Poco { Name = "poco1", Age = 12 };
            const string key = "CouchbaseCacheTests.Test_Refresh";
            var bucket = ClusterHelper.GetBucket("default");
            bucket.Remove(key);

            var v = bucket.Insert(key, GetBytes(poco), new TimeSpan(0, 0, 0, 2));

            cache.Refresh(key);

            Thread.Sleep(2000);
            var result = bucket.Get<byte[]>(key);
            Assert.AreEqual(ResponseStatus.Success, result.Status);
        }

        [Test]
        public async Task Test_RefreshAsync_WithTimeSpan()
        {
            var cache = GetCache(new TimeSpan(0, 0, 0, 4));

            var poco = new Poco { Name = "poco1", Age = 12 };
            const string key = "CouchbaseCacheTests.Test_RefreshAsync_WithTimeSpan";
            var bucket = ClusterHelper.GetBucket("default");
            await bucket.RemoveAsync(key);

            var v = await bucket.InsertAsync(key, GetBytes(poco), new TimeSpan(0, 0, 0, 2));

            await cache.RefreshAsync(key);

            Thread.Sleep(2000);
            var result = await bucket.GetAsync<byte[]>(key);
            Assert.AreEqual(ResponseStatus.Success, result.Status);
        }

        static byte[] GetBytes(Poco poco)
        {
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(poco));
        }

        static T GetObject<T>(byte[] bytes)
        {
            return JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(bytes));
        }

        private static CouchbaseCache GetCache(TimeSpan? timeSpan = null)
        {
            var options = new CouchbaseCacheOptions
            {
                BucketName = "default",
                Configuration = TestConfiguration.GetCurrentConfiguration(),
                LifeSpan = timeSpan
            };

            return new CouchbaseCache(options);
        }

        public class Poco
        {
            public string Name { get; set; }

            public int Age { get; set; }

            public override string ToString()
            {
                return string.Concat(Name, "-", Age);
            }
        }
    }
}
