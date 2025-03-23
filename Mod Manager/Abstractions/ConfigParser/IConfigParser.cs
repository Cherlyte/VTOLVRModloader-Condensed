namespace Mod_Manager.Abstractions.ConfigParser;

internal interface IConfigParser
{
    void SetFile(string filePath);
    bool GetValue(string sectionName, string keyName, bool defaultValue);
    string GetValue(string sectionName, string keyName, string defaultValue);
    bool SectionExists(string sectionName);
    bool SetValue(string sectionName, string keyName, bool value);
    bool SetValue(string sectionName, string keyName, string defaultValue);
    bool Save();
}