using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using UnityEngine;
using System;

public class Vector2Converter : JsonConverter {
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
        Vector2 vector = (Vector2)value;

        writer.WriteStartObject();
        writer.WritePropertyName("x");
        writer.WriteValue(vector.x);
        writer.WritePropertyName("y");
        writer.WriteValue(vector.y);
        writer.WriteEndObject();
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
        JObject jsonObject = JObject.Load(reader);
        float x = (float)jsonObject["x"];
        float y = (float)jsonObject["y"];
        return new Vector2(x, y);
    }

    public override bool CanConvert(Type objectType) {
        return objectType == typeof(Vector2) || objectType == typeof(Vector2?);
    }
}

public class Vector3Converter : JsonConverter {
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
        Vector3 vector = (Vector3)value;

        writer.WriteStartObject();
        writer.WritePropertyName("x");
        writer.WriteValue(vector.x);
        writer.WritePropertyName("y");
        writer.WriteValue(vector.y);
        writer.WritePropertyName("z");
        writer.WriteValue(vector.z);
        writer.WriteEndObject();
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
        JObject jsonObject = JObject.Load(reader);
        float x = (float)jsonObject["x"];
        float y = (float)jsonObject["y"];
        float z = (float)jsonObject["z"];
        return new Vector3(x, y, z);
    }

    public override bool CanConvert(Type objectType) {
        return objectType == typeof(Vector3) || objectType == typeof(Vector3?);
    }
}

public class Vector4Converter : JsonConverter {
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
        Vector4 vector = (Vector4)value;

        writer.WriteStartObject();
        writer.WritePropertyName("x");
        writer.WriteValue(vector.x);
        writer.WritePropertyName("y");
        writer.WriteValue(vector.y);
        writer.WritePropertyName("z");
        writer.WriteValue(vector.z);
        writer.WritePropertyName("w");
        writer.WriteValue(vector.w);
        writer.WriteEndObject();
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
        JObject jsonObject = JObject.Load(reader);
        float x = (float)jsonObject["x"];
        float y = (float)jsonObject["y"];
        float z = (float)jsonObject["z"];
        float w = (float)jsonObject["w"];
        return new Vector4(x, y, z);
    }

    public override bool CanConvert(Type objectType) {
        return objectType == typeof(Vector4) || objectType == typeof(Vector4?);
    }
}

public class QuaternionConverter : JsonConverter {
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
        Quaternion vector = (Quaternion)value;

        writer.WriteStartObject();
        writer.WritePropertyName("x");
        writer.WriteValue(vector.x);
        writer.WritePropertyName("y");
        writer.WriteValue(vector.y);
        writer.WritePropertyName("z");
        writer.WriteValue(vector.z);
        writer.WritePropertyName("w");
        writer.WriteValue(vector.w);
        writer.WriteEndObject();
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
        JObject jsonObject = JObject.Load(reader);
        float x = (float)jsonObject["x"];
        float y = (float)jsonObject["y"];
        float z = (float)jsonObject["z"];
        float w = (float)jsonObject["w"];
        return new Quaternion(x, y, z, w);
    }

    public override bool CanConvert(Type objectType) {
        return objectType == typeof(Quaternion) || objectType == typeof(Quaternion?);
    }
}