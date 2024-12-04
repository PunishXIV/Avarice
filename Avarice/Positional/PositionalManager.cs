using CsvHelper;
using System.Globalization;
using System.IO;
using System.Net.Http;

namespace Avarice.Positional;

public class PositionalManager
{
	private const string SheetUrl = "https://docs.google.com/spreadsheets/d/1UchGyajO-AG6gQwXQT1bsb3sh2ucwOU_vuqT8FRR8Ac/gviz/tq?tqx=out:csv&sheet=main1";
	private readonly string _filePath = Path.Combine(Svc.PluginInterface.AssemblyLocation.DirectoryName!, "positionals.csv");

	private readonly HttpClient _client;
	private readonly Dictionary<int, PositionalAction> _actionStore;

	public PositionalManager()
	{
		_client = new HttpClient();
		_actionStore = new Dictionary<int, PositionalAction>();
		Get();
		Load();
	}

	public void Reset()
	{
		Get();
		Load();
	}

	private void Get()
	{
		string text = _client.GetAsync(SheetUrl).Result.Content.ReadAsStringAsync().Result;
		if (!File.Exists(_filePath) || File.ReadAllText(_filePath) != text)
		{
			File.WriteAllText(_filePath, text);
		}
	}

	private void Load()
	{
		_actionStore.Clear();
		using StreamReader reader = new(_filePath);
		using CsvReader csv = new(reader, CultureInfo.InvariantCulture);

		foreach (PositionalRecord record in csv.GetRecords<PositionalRecord>())
		{
			if (!_actionStore.TryGetValue(record.Id, out PositionalAction action))
			{
				action = new PositionalAction
				{
					Id = record.Id,
					ActionName = record.ActionName,
					ActionPosition = record.ActionPosition,
					Positionals = [],
				};
				_actionStore.Add(record.Id, action);
			}

			PositionalParameters parameters = new()
			{
				Percent = record.Percent,
				IsHit = record.IsHit == "TRUE",
				Comment = record.Comment,
			};
			action.Positionals.Add(record.Percent, parameters);
		}
	}

	public bool IsPositionalHit(int actionId, int percent)
	{
		if (!_actionStore.TryGetValue(actionId, out PositionalAction action))
		{
			return false;
		}

		if (!action.Positionals.TryGetValue(percent, out PositionalParameters parameters))
		{
			return false;
		}

		return parameters.IsHit;
	}

	public PositionalParameters GetPositionalParameters(int actionId, int percent)
	{
		if (!_actionStore.TryGetValue(actionId, out PositionalAction action))
		{
			return null;
		}

		return action.Positionals.GetValueOrDefault(percent);
	}

	public bool IsPositional(int actionId)
	{
		return _actionStore.ContainsKey(actionId);
	}
}