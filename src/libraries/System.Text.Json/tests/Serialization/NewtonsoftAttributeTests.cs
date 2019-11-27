// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using Newtonsoft.Json;
using Xunit;

namespace System.Text.Json.Serialization.Tests
{
    public class NewtonsoftAttributeTests
    {
        [Fact]
        public static void ReadClassWithNewtonsoftJsonPropertyName()
        {
            // Baseline
            var obj = new ClassWithNewtonsoftJsonPropertyName() { MyInt = 3 };

            string json = JsonSerializer.Serialize(obj);
            Assert.Equal(@"{""MyInt"":3}", json);

            var newObj = JsonSerializer.Deserialize<ClassWithNewtonsoftJsonPropertyName>(json);
            Assert.Equal(3, newObj.MyInt);

            // With "use Newtonsoft attributes" flag
            var options = new JsonSerializerOptions() { UseNewtonsoftAttributes = true };

            json = JsonSerializer.Serialize(obj, options);
            Assert.Equal(@"{""FirstInt"":3}", json);

            var objWithOption = JsonSerializer.Deserialize<ClassWithNewtonsoftJsonPropertyName>(json, options);
            Assert.Equal(3, objWithOption.MyInt);
        }

        [Fact]
        public static void ReadClassWithNewtonsoftExtensionData()
        {
            // Baseline
            string json = @"{""MyInt"":3,""Extra"":3}";

            var obj = JsonSerializer.Deserialize<ClassWithNewtonsoftExtensionData>(json);
            Assert.Equal(3, obj.MyInt);
            Assert.Null(obj.ExtensionData);

            string newJson = JsonSerializer.Serialize(obj);
            Assert.Contains(@"""ExtensionData"":null", newJson);
            Assert.Contains(@"""MyInt"":3", newJson);

            // With "use Newtonsoft attributes" flag
            var options = new JsonSerializerOptions() { UseNewtonsoftAttributes = true };

            obj = JsonSerializer.Deserialize<ClassWithNewtonsoftExtensionData>(json, options);
            Assert.Equal(3, obj.MyInt);
            Assert.Equal(3, obj.ExtensionData["Extra"].GetInt32());

            string jsonWithOptions = JsonSerializer.Serialize(obj, options);
            Assert.Contains(@"""MyInt"":3", jsonWithOptions);
            Assert.Contains(@"""Extra"":3", jsonWithOptions);
        }

        [Fact]
        public static void ReadClassWithNewtonsoftAttributes()
        {
            // Baseline
            string json = @"{""FirstInt"":3,""ExtensionData"":{""Data"":3}}";

            var obj = JsonSerializer.Deserialize<ClassWithNewtonsoftAttributes>(json);
            // Skipped on deserialize since there's no matching JSON.
            Assert.Equal(0, obj.MyInt);
            // Treated as normal dictionary property since there is matching JSON.
            Assert.Equal(3, obj.ExtensionData["Data"].GetInt32());

            string newJson = JsonSerializer.Serialize(obj);
            Assert.Contains(@"""MyInt"":0", newJson);
            Assert.Contains(@"""ExtensionData"":{""Data"":3}", newJson);

            // With "use Newtonsoft attributes" flag
            var options = new JsonSerializerOptions() { UseNewtonsoftAttributes = true };

            obj = JsonSerializer.Deserialize<ClassWithNewtonsoftAttributes>(json, options);
            // Per Newtonsoft JsonProperty("FirstInt") attribute, there's a match.
            Assert.Equal(3, obj.MyInt);
            // ExtensionData JSON property is treated as missing data since the
            // ExtensionData class property is reserved (to hold extension data)/
            Assert.Equal(3, obj.ExtensionData["ExtensionData"].GetProperty("Data").GetInt32());

            string jsonWithOptions = JsonSerializer.Serialize(obj, options);
            Assert.Contains(@"""FirstInt"":3", jsonWithOptions);
            Assert.Contains(@"""ExtensionData"":{""Data"":3}", jsonWithOptions);
        }

        [Fact]
        public static void ReadClassWithAttributeConflicts()
        {
            // Baseline
            string json = @"{""NotFirstInt"":3,""Extra"":3}";

            var obj = JsonSerializer.Deserialize<ClassWithAttributeConflicts>(json);
            Assert.Equal(3, obj.MyInt);
            Assert.Equal(3, obj.ExtensionData["Extra"].GetInt32());

            string newJson = JsonSerializer.Serialize(obj);
            Assert.Contains(@"""NotFirstInt"":3", newJson);
            Assert.Contains(@"""Extra"":3", newJson);

            // With "use Newtonsoft attributes" flag
            var options = new JsonSerializerOptions() { UseNewtonsoftAttributes = true };

            obj = JsonSerializer.Deserialize<ClassWithAttributeConflicts>(json, options);
            Assert.Equal(3, obj.MyInt);
            Assert.Equal(3, obj.ExtensionData["Extra"].GetInt32());

            string jsonWithOptions = JsonSerializer.Serialize(obj, options);
            Assert.Contains(@"""NotFirstInt"":3", jsonWithOptions);
            Assert.Contains(@"""Extra"":3", jsonWithOptions);
        }

        [Fact]
        public static void ReadClassWithExtensionDataConflicts()
        {
            // Baseline: Newtonsoft.Json attribute is ignored.
            string json = @"{""Extra"":3}";

            var obj = JsonSerializer.Deserialize<ClassWithExtensionDataConflicts>(json);
            Assert.Equal(3, obj.ExtensionData1["Extra"].GetInt32());
            Assert.Null(obj.ExtensionData2);

            string newJson = JsonSerializer.Serialize(obj);
            Assert.Contains(@"""Extra"":3", newJson);
            Assert.Contains(@"""ExtensionData2"":null", newJson);

            // With "use Newtonsoft attributes" flag: Newtonsoft.Json attribute is still ignored,
            // since System.Text.Json attribute is present and has precedent.
            var options = new JsonSerializerOptions() { UseNewtonsoftAttributes = true };

            obj = JsonSerializer.Deserialize<ClassWithExtensionDataConflicts>(json, options);
            Assert.Equal(3, obj.ExtensionData1["Extra"].GetInt32());
            Assert.Null(obj.ExtensionData2);

            string jsonWithOptions = JsonSerializer.Serialize(obj, options);
            Assert.Contains(@"""Extra"":3", jsonWithOptions);
            Assert.Contains(@"""ExtensionData2"":null", jsonWithOptions);
        }

        [Fact]
        public static void ReadClassWithDuplicateExtensionData()
        {
            // Baseline
            string json = @"{""Extra"":3}";

            Assert.Throws<InvalidOperationException>(() => JsonSerializer.Deserialize<ClassWithDuplicateExtensionData>(json));
            Assert.Throws<InvalidOperationException>(() => JsonSerializer.Serialize(new ClassWithDuplicateExtensionData()));

            // With "use Newtonsoft attributes" flag
            var options = new JsonSerializerOptions() { UseNewtonsoftAttributes = true };

            Assert.Throws<InvalidOperationException>(() => JsonSerializer.Deserialize<ClassWithDuplicateExtensionData>(json, options));
            Assert.Throws<InvalidOperationException>(() => JsonSerializer.Serialize(new ClassWithDuplicateExtensionData(), options));
        }

        [Fact]
        public static void ReadClassWithRepeatedNewtonsoftExtensionData()
        {
            // Baseline: Newtonsoft attributes ignored.
            string json = @"{""Extra"":3}";

            var obj = JsonSerializer.Deserialize<ClassWithRepeatedNewtonsoftExtensionData>(json);
            Assert.Null(obj.ExtensionData1);
            Assert.Null(obj.ExtensionData2);

            string newJson = JsonSerializer.Serialize(obj);
            Assert.Contains(@"""ExtensionData1"":null", newJson);
            Assert.Contains(@"""ExtensionData2"":null", newJson);

            // With "use Newtonsoft attributes" flag: Newtonsoft attributes honored; exception thrown
            // because there's duplicate extension data.
            var options = new JsonSerializerOptions() { UseNewtonsoftAttributes = true };

            Assert.Throws<InvalidOperationException>(() => JsonSerializer.Deserialize<ClassWithRepeatedNewtonsoftExtensionData>(json, options));
            Assert.Throws<InvalidOperationException>(() => JsonSerializer.Serialize(new ClassWithRepeatedNewtonsoftExtensionData(), options));
        }

        [Fact]
        public static void ReadClassWithRepeatedNewtonsoftExtensionDataAndNativeExtensionData()
        {
            // Baseline
            string json = @"{""Extra"":3}";

            var obj = JsonSerializer.Deserialize<ClassWithRepeatedNewtonsoftExtensionDataAndNativeExtensionData>(json);
            Assert.Null(obj.ExtensionData1);
            Assert.Equal(3, ((JsonElement)obj.ExtensionData2["Extra"]).GetInt32());

            string newJson = JsonSerializer.Serialize(obj);
            Assert.Contains(@"""ExtensionData1"":null", newJson);
            Assert.Contains(@"""Extra"":3", newJson);

            // With "use Newtonsoft attributes" flag
            var options = new JsonSerializerOptions() { UseNewtonsoftAttributes = true };

            obj = JsonSerializer.Deserialize<ClassWithRepeatedNewtonsoftExtensionDataAndNativeExtensionData>(json, options);
            Assert.Null(obj.ExtensionData1);
            Assert.Equal(3, ((JsonElement)obj.ExtensionData2["Extra"]).GetInt32());

            string jsonWithOptions = JsonSerializer.Serialize(obj, options);
            Assert.Contains(@"""ExtensionData1"":null", jsonWithOptions);
            Assert.Contains(@"""Extra"":3", jsonWithOptions);
        }

        public class ClassWithNewtonsoftJsonPropertyName
        {
            [JsonProperty("FirstInt")]
            public int MyInt { get; set; }
        }

        public class ClassWithNewtonsoftExtensionData
        {
            public int MyInt { get; set; }

            [Newtonsoft.Json.JsonExtensionData]
            public Dictionary<string, JsonElement> ExtensionData { get; set; }
        }

        public class ClassWithNewtonsoftAttributes
        {
            [JsonProperty("FirstInt")]
            public int MyInt { get; set; }

            [Newtonsoft.Json.JsonExtensionData]
            public Dictionary<string, JsonElement> ExtensionData { get; set; }
        }

        public class ClassWithAttributeConflicts
        {
            [JsonPropertyName("NotFirstInt")]
            [JsonProperty("FirstInt")]
            public int MyInt { get; set; }

            [Newtonsoft.Json.JsonExtensionData]
            [JsonExtensionData]
            public Dictionary<string, JsonElement> ExtensionData { get; set; }
        }

        public class ClassWithExtensionDataConflicts
        {
            [JsonExtensionData]
            public Dictionary<string, JsonElement> ExtensionData1 { get; set; }

            [Newtonsoft.Json.JsonExtensionData]
            public IDictionary<string, object> ExtensionData2 { get; set; }
        }

        public class ClassWithDuplicateExtensionData
        {
            [JsonExtensionData]
            public Dictionary<string, JsonElement> ExtensionData1 { get; set; }

            [Newtonsoft.Json.JsonExtensionData]
            [JsonExtensionData]
            public IDictionary<string, object> ExtensionData2 { get; set; }
        }

        public class ClassWithRepeatedNewtonsoftExtensionData
        {
            [Newtonsoft.Json.JsonExtensionData]
            public Dictionary<string, JsonElement> ExtensionData1 { get; set; }

            [Newtonsoft.Json.JsonExtensionData]
            public IDictionary<string, object> ExtensionData2 { get; set; }
        }

        public class ClassWithRepeatedNewtonsoftExtensionDataAndNativeExtensionData
        {
            [Newtonsoft.Json.JsonExtensionData]
            public Dictionary<string, JsonElement> ExtensionData1 { get; set; }

            [Newtonsoft.Json.JsonExtensionData]
            [JsonExtensionData]
            public IDictionary<string, object> ExtensionData2 { get; set; }
        }
    }
}
