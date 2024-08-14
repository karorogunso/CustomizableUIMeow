﻿using CustomizableUIMeow.Model;
using CustomizableUIMeow.Model.ConfigClass;
using CustomizableUIMeow.Parser.SimpleTag.TagParser;
using CustomizableUIMeow.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;
using static Org.BouncyCastle.Crypto.Digests.SkeinEngine;
using Exiled.API.Features;

namespace CustomizableUIMeow.Parser.TagParser.Custom
{
    public class CustomTagParser
    {
        private bool _initialized = false;

        private List<CustomHints> _customTags = new List<CustomHints>();

        [TagParser("CustomHints")]
        public string Hints(TagParserParameter parameter)
        {
            if(!_initialized)
                Initialize();

            if (!parameter.Arguments.TryDequeue(out var arg))
                return null;

            var customTag = _customTags.FirstOrDefault(x => x.TagName == arg);

            if (customTag == null)
                return null;

            //Initialize the tag if it's the first time
            if(customTag.NextUpdate == DateTime.MinValue)
            {
                customTag.NextUpdate = DateTime.Now.AddSeconds(customTag.SwitchInterval);
            }

            //Update the tag if it's time
            if(DateTime.Now > customTag.NextUpdate)
            {
               customTag.NextUpdate = DateTime.Now.AddSeconds(customTag.SwitchInterval);
               customTag.LastIndex = (customTag.LastIndex + 1) % customTag.TagContent.Count;
            }

            return customTag.TagContent[customTag.LastIndex];
        }

        private void Initialize()
        {
            var tagList = new List<CustomHints>();
            var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .Build();

            foreach (var file in FileReader.ReadFile(FileReader.FileType.CustomTags))
            {
                try
                {
                    var tagConfig = deserializer.Deserialize<CustomHintConfig>(file.Item2);

                    var tag = new CustomHints
                    {
                        TagName = tagConfig.TagName,
                        SwitchInterval = tagConfig.SwitchInterval,
                        TagContent = tagConfig.TagContent
                    };

                    _customTags.Add(tag);
                }
                catch(Exception ex)
                {
                    Log.Error($"Error while parsing custom tags. File: {file.Item1} Exception: {ex}");
                }
            }

            _initialized = true;
        }
    }
}