using System.Collections.ObjectModel;
using System.Windows.Input;
using AdvancedCalculator.Application.Services;
using AdvancedCalculator.Core.Interfaces;
using AdvancedCalculator.Core.Models;
using AdvancedCalculator.UI.Helpers;

namespace AdvancedCalculator.UI.ViewModels;

public class HistoryViewModel : ViewModelBase
{
    private readonly IHistoryRepository _historyRepository;
    private readonly IClipboardService _clipboardService;

    private string _searchQuery = "";
    private CalculationRecord? _selectedRecord;

    public ObservableCollection<CalculationRecord> Records { get; } = new();

    public string SearchQuery
    {
        get => _searchQuery;
        set
        {
            if (SetProperty(ref _searchQuery, value))
            {
                _ = SearchHistoryAsync();
            }
        }
    }

    public CalculationRecord? SelectedRecord
    {
        get => _selectedRecord;
        set => SetProperty(ref _selectedRecord, value);
    }

    public ICommand RefreshCommand { get; }
    public ICommand ClearAllCommand { get; }
    public ICommand DeleteRecordCommand { get; }
    public ICommand TogglePinCommand { get; }
    public ICommand CopyResultCommand { get; }
    public ICommand ExportCsvCommand { get; }
    public ICommand ExportTextCommand { get; }

    public event Action<CalculationRecord>? RequestLoadCalculation;

    public HistoryViewModel(IHistoryRepository historyRepository, IClipboardService clipboardService)
    {
        _historyRepository = historyRepository;
        _clipboardService = clipboardService;

        RefreshCommand = new RelayCommand(async () => await LoadHistoryAsync());
        ClearAllCommand = new RelayCommand(async () =>
        {
            await _historyRepository.ClearAllAsync();
            await LoadHistoryAsync();
        });
        DeleteRecordCommand = new RelayCommand(async param =>
        {
            if (param is CalculationRecord rec)
            {
                await _historyRepository.DeleteAsync(rec.Id);
                await LoadHistoryAsync();
            }
        });
        TogglePinCommand = new RelayCommand(async param =>
        {
            if (param is CalculationRecord rec)
            {
                await _historyRepository.TogglePinAsync(rec.Id, !rec.IsPinned);
                await LoadHistoryAsync();
            }
        });
        CopyResultCommand = new RelayCommand(param =>
        {
            if (param is CalculationRecord rec)
            {
                _clipboardService.SetText(rec.Result);
            }
        });
        ExportCsvCommand = new RelayCommand(ExportCsv);
        ExportTextCommand = new RelayCommand(ExportText);

        _ = LoadHistoryAsync();
    }

    public async Task LoadHistoryAsync()
    {
        var list = await _historyRepository.GetAllAsync();
        Records.Clear();
        foreach (var r in list)
        {
            Records.Add(r);
        }
    }

    public async Task SearchHistoryAsync()
    {
        var list = await _historyRepository.SearchAsync(SearchQuery);
        Records.Clear();
        foreach (var r in list)
        {
            Records.Add(r);
        }
    }

    public void OnRecordDoubleClicked(CalculationRecord record)
    {
        RequestLoadCalculation?.Invoke(record);
    }

    private string _exportStatus = "";
    public string ExportStatus
    {
        get => _exportStatus;
        set => SetProperty(ref _exportStatus, value);
    }

    private void ExportCsv()
    {
        string csv = HistoryExportService.ExportToCsv(Records);
        _clipboardService.SetText(csv);
        ExportStatus = "History exported (CSV) to clipboard!";
    }

    private void ExportText()
    {
        string text = HistoryExportService.ExportToText(Records);
        _clipboardService.SetText(text);
        ExportStatus = "History exported (Text) to clipboard!";
    }
}
