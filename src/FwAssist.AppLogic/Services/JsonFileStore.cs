using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace FwAssist.AppLogic.Services;

/// <summary>
/// One small JSON file holding one value, as Lagebuch keeps them: an installed Kartenpaket's
/// catalogue entry, for one.
/// <para>
/// A missing or corrupt file reads as "no value". Only <see cref="JsonException"/> is swallowed; an
/// <see cref="IOException"/> still surfaces, because a file that cannot be read at all is a
/// different problem from one holding nothing useful.
/// </para>
/// </summary>
/// <typeparam name="T">The payload persisted as the file's entire contents.</typeparam>
internal sealed class JsonFileStore<T>
{
    private readonly string _path;

    public JsonFileStore(string path) => _path = path;

    /// <summary>
    /// Reads the stored payload, or returns <c>false</c> when the file is missing, unparseable, or
    /// holds a JSON <c>null</c>.
    /// </summary>
    public bool TryRead([MaybeNullWhen(false)] out T value)
    {
        value = default;
        if (!File.Exists(_path))
        {
            return false;
        }

        try
        {
            value = JsonSerializer.Deserialize<T>(File.ReadAllText(_path));
        }
        catch (JsonException)
        {
            return false;
        }

        return value is not null;
    }

    /// <summary>
    /// Write-then-rename: a crash between the two leaves either the old file untouched or the new
    /// one fully in place, never a half-written file. <c>overwrite: true</c> also replaces a stale
    /// <c>*.tmp</c> left behind by a process that died between the write and the move.
    /// </summary>
    public void Write(T value)
    {
        var tmpPath = _path + ".tmp";
        File.WriteAllText(tmpPath, JsonSerializer.Serialize(value));
        File.Move(tmpPath, _path, overwrite: true);
    }
}
