using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using ReactiveUI;
using PlaylistLinkParser.Models;
using PlaylistLinkParser.Services;

namespace PlaylistLinkParser.ViewModels;

public class MainViewModel : ReactiveObject
{
    private readonly ParserService _parserService;
    private string _url = string.Empty;
    private PlaylistInfo _currentPlaylist = new();
    private bool _isLoading;
    private string _statusMessage = string.Empty;

    public string Url
    {
        get => _url;
        set => this.RaiseAndSetIfChanged(ref _url, value);
    }

    public PlaylistInfo CurrentPlaylist
    {
        get => _currentPlaylist;
        set => this.RaiseAndSetIfChanged(ref _currentPlaylist, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => this.RaiseAndSetIfChanged(ref _isLoading, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
    }

    public ObservableCollection<TrackInfo> Tracks { get; } = new();

    public ICommand ParseCommand { get; }

    public MainViewModel()
    {
        _parserService = new ParserService();
        ParseCommand = ReactiveCommand.CreateFromTask(ParsePlaylistAsync);
    }

    private async Task ParsePlaylistAsync()
    {
        if (string.IsNullOrWhiteSpace(Url))
        {
            StatusMessage = "Please enter a valid URL.";
            return;
        }

        try
        {
            IsLoading = true;
            StatusMessage = "Loading playlist data...";

            Tracks.Clear();

            var (playlist, tracks) = await _parserService.ParsePlaylistAsync(Url);

            CurrentPlaylist = playlist;
            foreach (var track in tracks)
            {
                Tracks.Add(track);
            }

            StatusMessage = $"Successfully loaded {tracks.Count} tracks.";
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }
}