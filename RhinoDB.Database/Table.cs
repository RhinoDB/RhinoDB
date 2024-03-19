using Chase.CommonLib.Math;
using Newtonsoft.Json;

namespace RhinoDB.Database;

public class Table
{
    /// <summary>
    /// Represents a unique identifier for an object.
    /// </summary>
    [JsonProperty("id")]
    public Guid Id { get; set; }

    /// <summary>
    /// Represents the Name property of a Table object.
    /// </summary>
    /// <value>The name of the Table.</value>
    [JsonProperty("name")]
    public string Name { get; set; }

    /// <summary>
    /// Represents the path to the file associated with a Table or Database.
    /// </summary>
    [JsonIgnore] public string FilePath { get; set; }

    /// <summary>
    /// Gets or sets the creation time of a table or a database.
    /// </summary>
    [JsonProperty("creation")] public DateTime CreationTime { get; set; }

    /// <summary>
    /// Gets or sets the last modified date of the object.
    /// </summary>
    /// <value>The last modified date.</value>
    [JsonProperty("last_modified")] public DateTime LastModified { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the object is marked as dirty.
    /// </summary>
    /// <remarks>
    /// An object is considered dirty if its state has been modified since the last save or load operation.
    /// </remarks>
    [JsonIgnore] public bool IsDirty { get; set; }

    /// <summary>
    /// Represents a table in the database.
    /// </summary>
    [JsonIgnore] public Database Database { get; private set; }

    private AdvancedTimer? _timer;
}