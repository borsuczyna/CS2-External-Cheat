using Newtonsoft.Json;

namespace CS2.Core;

public class Offsets
{
    private readonly string _url = "https://raw.githubusercontent.com/a2x/cs2-dumper/main/output/";
    private readonly string _directoryPath = "data";
    private string? _filePath;
    private dynamic? _offsets;

    public Offsets(string fileName)
    {
        _url += fileName;
        _filePath = Path.Combine(_directoryPath, fileName);
        LoadOffsets();
    }

    public async Task UpdateOffsets()
    {
        if (_filePath == null)
        {
            throw new Exception("File path not set.");
        }

        try
        {
            Console.WriteLine("Updating offsets...");

            // Ensure the directory exists
            if (!Directory.Exists(_directoryPath))
            {
                Directory.CreateDirectory(_directoryPath);
                Console.WriteLine($"Created directory: {_directoryPath}");
            }

            // Download the file
            using (HttpClient client = new HttpClient())
            {
                Console.WriteLine("Fetching the file...");
                string content = await client.GetStringAsync(_url);

                // Save the file
                await File.WriteAllTextAsync(_filePath, content);
                Console.WriteLine($"Offsets updated and saved to: {_filePath}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while updating offsets: {ex.Message}");
        }
    }

    public void LoadOffsets()
    {
        if (_filePath == null)
        {
            throw new Exception("File path not set.");
        }

        try
        {
            UpdateOffsets().Wait();

            string content = File.ReadAllText(_filePath);
            _offsets = JsonConvert.DeserializeObject(content);
            Console.WriteLine("Offsets loaded successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while loading offsets: {ex.Message}");
        }
    }

    // make new, examples:
    // Read<string>("client.dll:dwEntityList");
    // Read<string>("client.dll:dwEntityList:some-key");
    // it can have more than 2 parts
    public T Read<T>(string key)
    {
        if (_offsets == null)
        {
            throw new Exception("Offsets not loaded.");
        }

        string[] parts = key.Split(':');
        dynamic? current = _offsets;
        foreach (string part in parts)
        {
            if (current == null)
            {
                throw new Exception($"Key not found: {key}");
            }

            current = current[part];
        }

        return current.ToObject<T>();
    }
}