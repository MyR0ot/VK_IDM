using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VK_IDM.Interfaces
{
    public interface IDataLoaderService
    {
        List<T> ParseStatesFromJsonFile<T>(string jsonFilePath) where T : class;

        void SerializeToJsonFile<T>(List<T> states, string jsonFilePath) where T : class;
    }
}
