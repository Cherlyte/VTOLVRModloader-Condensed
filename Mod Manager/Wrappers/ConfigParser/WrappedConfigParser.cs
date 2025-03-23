using System.Linq;
using Mod_Manager.Abstractions.ConfigParser;

namespace Mod_Manager.Wrappers.ConfigParser;

internal sealed class WrappedConfigParser : IConfigParser
{
    private Salaros.Configuration.ConfigParser _parser;

    public void SetFile(string filePath) => _parser = new Salaros.Configuration.ConfigParser(filePath);

    public bool GetValue(string sectionName, string keyName, bool defaultValue) =>
        _parser.GetValue(sectionName, keyName, defaultValue);
    
    public string GetValue(string sectionName, string keyName, string defaultValue) =>
        _parser.GetValue(sectionName, keyName, defaultValue);

    public bool SectionExists(string sectionName) => 
        _parser.Sections.Any(cs => cs.SectionName.Equals(sectionName));

    public bool SetValue(string sectionName, string keyName, bool value) =>
        _parser.SetValue(sectionName, keyName, value);
    
    public bool SetValue(string sectionName, string keyName, string defaultValue) =>
        _parser.SetValue(sectionName, keyName, defaultValue);

    public bool Save() => _parser.Save();
}