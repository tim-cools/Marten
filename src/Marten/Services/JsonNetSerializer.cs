﻿using System;
using System.IO;
using Baseline;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Marten.Services
{
    public class JsonNetSerializer : ISerializer
    {
        private readonly JsonSerializer _clean = new JsonSerializer
        {
            TypeNameHandling =  TypeNameHandling.None,
            DateFormatHandling = DateFormatHandling.IsoDateFormat
        };

        // SAMPLE: newtonsoft-configuration
        private readonly JsonSerializer _serializer = new JsonSerializer
        {
            TypeNameHandling = TypeNameHandling.Auto,

            // ISO 8601 formatting of DateTime's is mandatory
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead
        };
        // ENDSAMPLE

        /// <summary>
        /// Customize the inner Newtonsoft formatter. 
        /// </summary>
        /// <param name="configure"></param>
        public void Customize(Action<JsonSerializer> configure)
        {
            configure(_clean);
            configure(_serializer);

            _clean.TypeNameHandling = TypeNameHandling.None;
        }

        public string ToJson(object document)
        {
            var writer = new StringWriter();
            _serializer.Serialize(writer, document);

            return writer.ToString();
        }

        public T FromJson<T>(string json)
        {
            return _serializer.Deserialize<T>(new JsonTextReader(new StringReader(json)));
        }

        public T FromJson<T>(Stream stream)
        {
            return _serializer.Deserialize<T>(new JsonTextReader(new StreamReader(stream)));
        }

        public object FromJson(Type type, string json)
        {
            return _serializer.Deserialize(new JsonTextReader(new StringReader(json)), type);
        }

        public T FromJson<T>(TextReader reader)
        {
            return _serializer.Deserialize<T>(new JsonTextReader(reader));
        }

        public object FromJson(Type type, TextReader reader)
        {
            return _serializer.Deserialize(new JsonTextReader(reader), type);
        }

        public string ToCleanJson(object document)
        {
            var writer = new StringWriter();

            _clean.Serialize(writer, document);

            return writer.ToString();
        }

        private EnumStorage _enumStorage = EnumStorage.AsInteger;
        
        /// <summary>
        /// Specify whether .Net Enum values should be stored as integers or strings
        /// within the Json document. Default is AsInteger.
        /// </summary>
        public EnumStorage EnumStorage
        {
            get
            {
                return _enumStorage;
            }
            set
            {
                _enumStorage = value;

                if (value == EnumStorage.AsString)
                {
                    var converter = new StringEnumConverter();
                    _serializer.Converters.Add(converter);
                    _clean.Converters.Add(converter);
                }
                else
                {
                    _serializer.Converters.RemoveAll(x => x is StringEnumConverter);
                    _clean.Converters.RemoveAll(x => x is StringEnumConverter);
                }
            }
        }
    }
}