using System;
using System.IO;
using System.Text;
using System.Text.Json;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using UrbanXX.IO.GeoJSON.Converters;

namespace UrbanXX.IO.GeoJSON
{
    public class GeoJsonReader
    {
        public GeoJsonConverterFactory ConverterFactory { get; }

        public JsonSerializerOptions SerializerOptions { get; }


        /// <summary>
        /// Creates an instance of this class using the defaults.
        /// </summary>
        public GeoJsonReader()
        {
            // Using default settings.
            ConverterFactory = new GeoJsonConverterFactory();

            SerializerOptions = new JsonSerializerOptions { ReadCommentHandling = JsonCommentHandling.Skip };
            SerializerOptions.Converters.Add(ConverterFactory);
        }


        /// <summary>
        /// Creates an instance of this class using the provided <see cref="GeoJsonConverterFactory"/> and
        /// <see cref="JsonSerializerOptions"/>.
        /// </summary>
        /// <param name="factory">The factory to use when creating geometries.</param>
        public GeoJsonReader(GeoJsonConverterFactory factory)
        {
            ConverterFactory = factory;

            SerializerOptions = new JsonSerializerOptions { ReadCommentHandling = JsonCommentHandling.Skip };
            SerializerOptions.Converters.Add(ConverterFactory);
        }


        /// <summary>
        /// Reading the specified json.
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="geoJson">The geoJson.</param>
        /// <returns>The deserialized object.</returns>
        public T Read<T>(string geoJson)
            where T:class
        {
            if(geoJson == null)
                throw new ArgumentNullException(nameof(geoJson));

            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(geoJson));
            return Deserialize<T>(ms, SerializerOptions);
        }


        /// <summary>
        /// Helper method for reading specified memory stream.
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="stream">The memory stream.</param>
        /// <param name="options">Serializer option.</param>
        /// <returns>The deserialized object.</returns>
        private T Deserialize<T>(MemoryStream stream, JsonSerializerOptions options)
            where T:class
        {
            var b = new ReadOnlySpan<byte>(stream.ToArray());
            var r = new Utf8JsonReader(b);

            // we are at None
            r.Read();
            var res = JsonSerializer.Deserialize<T>(ref r, options);

            return res;
        }

        public static FeatureCollection GetFeatureCollectionFromJson(string json)
        {

            GeoJsonConverterFactory gf = new GeoJsonConverterFactory();

            var reader = new GeoJsonReader(gf);
            reader.SerializerOptions.IgnoreNullValues = true;

            var collection = reader.Read<FeatureCollection>(json);

            return collection;
        }

        public static GeometryCollection GetGeometriesFromFeatureCollection(FeatureCollection collection)
        {
            // Handle empty FeacutureCollection.
            if (collection.Count == 0)
            {
                return null;
            }

            Geometry[] geoms = new Geometry[collection.Count];

            for (int i = 0; i < collection.Count; i++)
            {
                geoms[i] = collection[i].Geometry;
            }

            // Important: need to inherit geometry factory from collection.
            return new GeometryCollection(geoms, collection[0].Geometry.Factory);
        }

        public static FeatureCollection ParseJSON(string jsonFilePath)
        {
            StreamReader sr = File.OpenText(jsonFilePath);

            var feactureCollection = GeoJsonReader.GetFeatureCollectionFromJson(sr.ReadToEnd());
            return feactureCollection;
        }
        public static string[] ParseJSON(string jsonFilePath, string name)
        {
            StreamReader sr = File.OpenText(jsonFilePath);

            var feactureCollection = GeoJsonReader.GetFeatureCollectionFromJson(sr.ReadToEnd());
            var jsonResult = new string[feactureCollection.Count];
            for (int i = 0; i < feactureCollection.Count; i++)
            {
                jsonResult[i] = feactureCollection[i].Attributes[name].ToString();
            }

            return jsonResult;
        }
    }
}
