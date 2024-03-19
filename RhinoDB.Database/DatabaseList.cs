using System.IO.MemoryMappedFiles;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RhinoDB.Database;

public class DatabaseList
{
    public static DatabaseList Instance = Instance ?? new DatabaseList();
    public Dictionary<Guid, string> DatabaseIdsMap = [];
    public Dictionary<string, Guid> DatabaseNamesMap = [];
    private readonly string _manifestFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "db-list");


    private DatabaseList()
    {
        Load();
    }

    /// <summary>
    /// Loads the database list from the manifest file.
    /// </summary>
    public void Load()
    {
        if (!File.Exists(_manifestFile))
        {
            Save();
            return;
        }

        string json = File.ReadAllText(_manifestFile);
        var list = JsonConvert.DeserializeObject<Dictionary<Guid, string>>(json) ?? [];
        DatabaseIdsMap = list;
        DatabaseNamesMap = list.ToDictionary(x => x.Value, x => x.Key);
    }

    /// <summary>
    /// Saves the database list to the manifest file.
    /// </summary>
    public void Save()
    {
        string json = JsonConvert.SerializeObject(DatabaseIdsMap);
        File.WriteAllText(_manifestFile, json);
    }

    /// <summary>
    /// Add a database to the list of databases.
    /// </summary>
    /// <param name="id">The unique identifier of the database.</param>
    /// <param name="name">The name of the database.</param>
    public void AddDatabase(Guid id, string name)
    {
        DatabaseIdsMap.Add(id, name);
        DatabaseNamesMap.Add(name, id);
        Save();
    }

    /// <summary>
    /// Removes a database from the database list.
    /// </summary>
    /// <param name="id">The unique identifier of the database to remove.</param>
    public void RemoveDatabase(Guid id)
    {
        string name = DatabaseIdsMap[id];
        DatabaseIdsMap.Remove(id);
        DatabaseNamesMap.Remove(name);
        Save();
    }

    /// <summary>
    /// Renames a database identified by its ID to a new name.
    /// </summary>
    /// <param name="id">The ID of the database to be renamed.</param>
    /// <param name="newName">The new name for the database.</param>
    public void RenameDatabase(Guid id, string newName)
    {
        string oldName = DatabaseIdsMap[id];
        DatabaseIdsMap[id] = newName;
        DatabaseNamesMap.Remove(oldName);
        DatabaseNamesMap.Add(newName, id);
        Save();
    }

    /// <summary>
    /// Retrieves the database ID for a given database name.
    /// </summary>
    /// <param name="name">The name of the database.</param>
    /// <returns>The ID of the database.</returns>
    public Guid GetDatabaseId(string name)
    {
        return DatabaseNamesMap[name];
    }

    /// <summary>
    /// Get the name of the database associated with the specified ID.
    /// </summary>
    /// <param name="id">The ID of the database.</param>
    /// <returns>The name of the database.</returns>
    public string GetDatabaseName(Guid id)
    {
        return DatabaseIdsMap[id];
    }

    /// <summary>
    /// Checks whether a database with the specified ID exists in the database list.
    /// </summary>
    /// <param name="id">The ID of the database to check.</param>
    /// <returns>
    /// <c>true</c> if a database with the specified ID exists; otherwise, <c>false</c>.
    /// </returns>
    public bool DatabaseExists(Guid id)
    {
        return DatabaseIdsMap.ContainsKey(id);
    }

    /// <summary>
    /// Checks if a database with the specified ID exists.
    /// </summary>
    /// <param name="id">The ID of the database to check.</param>
    /// <returns>
    /// <c>true</c> if a database with the specified ID exists; otherwise, <c>false</c>.
    /// </returns>
    public bool DatabaseExists(string name)
    {
        return DatabaseNamesMap.ContainsKey(name);
    }

    /// <summary>
    /// Checks if a database exists in the database list.
    /// </summary>
    /// <param name="id">The ID of the database.</param>
    /// <returns>True if the database exists, false otherwise.</returns>
    public bool DatabaseExists(Guid id, string name)
    {
        return DatabaseIdsMap.ContainsKey(id) && DatabaseNamesMap.ContainsKey(name);
    }

    /// <summary>
    /// Converts the DatabaseIdsMap dictionary to a JObject.
    /// </summary>
    /// <returns>A JObject representation of the DatabaseIdsMap dictionary.</returns>
    public JObject ToObject()
    {
        return JObject.FromObject(DatabaseIdsMap);
    }

    /// <summary>
    /// Converts the DatabaseList object to its equivalent string representation.
    /// </summary>
    /// <returns>The string representation of the DatabaseList object.</returns>
    public override string ToString() => JsonConvert.SerializeObject(DatabaseIdsMap);
}