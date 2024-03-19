using Chase.CommonLib;
using Chase.CommonLib.Math;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RhinoDB.Database;

/// <summary>
/// Represents a database.
/// </summary>
public class Database
{
    /// <summary>
    /// Represents the unique identifier for a database.
    /// </summary>
    [JsonProperty("id")]
    public Guid Id { get; set; }

    /// <summary>
    /// Represents the name of a database.
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; private set; }

    /// <summary>
    /// Gets the creation time of the database.
    /// </summary>
    /// <remarks>
    /// This property represents the date and time when the database was created.
    /// </remarks>
    /// <seealso cref="Database"/>
    [JsonProperty("creation")]
    public DateTime CreationTime { get; private set; }

    /// <summary>
    /// Gets or sets the last modified time of the database.
    /// </summary>
    /// <remarks>
    /// This property represents the time when the database was last modified.
    /// It is automatically updated whenever the database is marked as dirty and saved to disk.
    /// </remarks>
    [JsonProperty("last_modified")]
    public DateTime LastModified { get; private set; }

    /// <summary>
    /// Gets or sets a value indicating whether the Database is dirty.
    /// </summary>
    [JsonIgnore]
    public bool IsDirty { get; private set; } = false;

    /// <summary>
    /// Represents a database path.
    /// </summary>
    [JsonIgnore]
    public string FilePath { get; private set; }

    /// <summary>
    /// Represents a timer used to track the dirty state of the database and trigger saving operations.
    /// </summary>
    [JsonIgnore] private AdvancedTimer? _timer = null;


    /// <summary>
    /// Represents a database.
    /// </summary>
    public Database(string name)
    {
        Id = Guid.NewGuid();
        Name = name;
        CreationTime = DateTime.Now;
        LastModified = DateTime.Now;
        FilePath = GetDatabasePath(Id);
        MarkDirty();
    }

    /// <summary>
    /// Marks the database as dirty.
    /// If the timer is null, creates a new timer with interval of 30 minutes, sets the timer Elapsed event to save the database, and starts the timer.
    /// If the timer is not null, stops the timer if it is running, or starts the timer if it is not running.
    /// Sets the IsDirty flag to true.
    /// </summary>
    /// <remarks>
    /// The database will be saved to disk if it is marked as dirty when the timer Elapsed event is triggered.
    /// </remarks>
    /// <seealso cref="Database.Save"/>
    public void MarkDirty()
    {
        if (_timer == null)
        {
            _timer = new AdvancedTimer(TimeSpan.FromMinutes(30));
            _timer.Elapsed += async (s, e) => await Save();
            _timer.Start();
        }
        else
        {
            if (_timer.IsRunning)
                _timer.Reset();
            else
                _timer.Start();
        }

        IsDirty = true;
    }

    /// <summary>
    /// Saves the database to disk if it is marked as dirty.
    /// </summary>
    /// <returns>A task representing the asynchronous save operation.</returns>
    private async Task Save()
    {
        if (IsDirty)
        {
            // Save the database to disk.
            IsDirty = false;
            _timer?.Stop();
            LastModified = DateTime.Now;
            Directory.CreateDirectory(FilePath);
            await File.WriteAllTextAsync(GetDatabaseManifestPath(Id), ToString());
            if (!DatabaseList.Instance.DatabaseExists(this.Id)) DatabaseList.Instance.AddDatabase(Id, Name);
        }
    }

    /// <summary>
    /// Loads a database with the given name.
    /// </summary>
    /// <param name="id">The ID of the database.</param>
    /// <returns>The loaded database, or null if the database does not exist.</returns>
    public static Database? Load(Guid id)
    {
        return File.Exists(GetDatabaseManifestPath(id)) ? JsonConvert.DeserializeObject<Database>(File.ReadAllText(GetDatabaseManifestPath(id))) : null;
    }

    /// <summary>
    /// Retrieves the path to the database manifest file.
    /// </summary>
    /// <param name="id">The ID of the database.</param>
    /// <returns>The path to the database manifest file.</returns>
    private static string GetDatabaseManifestPath(Guid id)
        => Path.Combine(GetDatabasePath(id), "manifest");

    /// <summary>
    /// Gets the file path for a database with the given ID.
    /// </summary>
    /// <param name="id">The ID of the database.</param>
    /// <returns>The file path of the database.</returns>
    private static string GetDatabasePath(Guid id)
    {
        string idString = id.ToString();
        return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "Databases", idString[..2], idString);
    }


    /// <summary>
    /// Converts the Database object to a JObject.
    /// </summary>
    /// <returns>A JObject representing the Database object.</returns>
    /// <remarks>
    /// The JObject is created using the Newtonsoft.Json library's JObject.FromObject() method, which serializes the Database object to a JSON string and then converts it to a JObject.
    /// </remarks>
    /// <seealso cref="Newtonsoft.Json.Linq.JObject"/>
    /// <seealso cref="Database"/>
    public JObject ToObject()
    {
        return JObject.FromObject(this);
    }

    /// <summary>
    /// Returns a string representation of the current instance using JSON serialization.
    /// </summary>
    /// <returns>
    /// A string representation of the current instance.
    /// </returns>
    public override string ToString() => JsonConvert.SerializeObject(this);
}