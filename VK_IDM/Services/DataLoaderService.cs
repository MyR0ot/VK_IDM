using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VK_IDM.Interfaces;

namespace VK_IDM.Services
{
    public class DataLoaderService : IDataLoaderService
    {
        private readonly JsonSerializerSettings _jsonSerializerSettings;

        public DataLoaderService()
        {
            _jsonSerializerSettings = new JsonSerializerSettings
            {
                DateFormatString = "yyyy-MM-dd",
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                },
            };

        }

        public List<T> ParseStatesFromJsonFile<T>(string jsonFilePath) where T : class
        {
            if (!File.Exists(jsonFilePath))
                throw new FileNotFoundException($"File not exist for path: {jsonFilePath}");

            var jsonData = File.ReadAllText(jsonFilePath);
            return JsonConvert.DeserializeObject<List<T>>(jsonData, _jsonSerializerSettings);
        }

        public void SerializeToJsonFile<T>(List<T> states, string jsonFilePath) where T : class
        {
            var json = JsonConvert.SerializeObject(states, _jsonSerializerSettings);
            File.WriteAllText(jsonFilePath, json);
        }
    }
}
