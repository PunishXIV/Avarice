using CsvHelper;
using System.Globalization;
using System.IO;

namespace Avarice.Positional;

public class PositionalManager
{
	private readonly string _localOverridePath = Path.Combine(Svc.PluginInterface.AssemblyLocation.DirectoryName!, "positionals_local.csv");
	private readonly Dictionary<int, PositionalAction> _actionStore = new();

	public PositionalManager()
	{
		Load();
		LoadLocalOverride();
	}

	public void Reset()
	{
		Load();
		LoadLocalOverride();
	}

	private void Load()
	{
		_actionStore.Clear();
		foreach (var row in StaticData.PositionalPotencies.Records)
		{
			if (!_actionStore.TryGetValue(row.Id, out PositionalAction action))
			{
				action = new PositionalAction
				{
					Id = row.Id,
					ActionName = row.Name,
					ActionPosition = row.Position,
					Positionals = new(),
				};
				_actionStore.Add(row.Id, action);
			}

			action.Positionals[row.Percent] = new PositionalParameters
			{
				Percent = row.Percent,
				IsHit = row.IsHit,
				Comment = row.Comment,
			};
		}
	}

	private void LoadLocalOverride()
	{
		if (!File.Exists(_localOverridePath)) return;

		using StreamReader reader = new(_localOverridePath);
		using CsvReader csv = new(reader, CultureInfo.InvariantCulture);

		int count = 0;
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

			action.Positionals[record.Percent] = new PositionalParameters
			{
				Percent = record.Percent,
				IsHit = record.IsHit == "TRUE",
				Comment = record.Comment,
			};
			count++;
		}
		PluginLog.Information($"Loaded {count} positional override row(s) from positionals_local.csv");
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
