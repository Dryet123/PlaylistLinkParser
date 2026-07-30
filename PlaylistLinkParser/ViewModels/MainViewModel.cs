using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.Input;
using PlaylistLinkParser.Models;
using PlaylistLinkParser.Services;

namespace PlaylistLinkParser.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly IParserService? _parserService;
    private string _url = string.Empty;
    private PlaylistInfo _currentPlaylist = new();
    private bool _isLoading;
    private string _statusMessage = string.Empty;
    private Bitmap? _playlistImage;

    public string Url
    {
        get => _url;
        set => SetProperty(ref _url, value);
    }

    public PlaylistInfo CurrentPlaylist
    {
        get => _currentPlaylist;
        set => SetProperty(ref _currentPlaylist, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public Bitmap? PlaylistImage
    {
        get => _playlistImage;
        set => SetProperty(ref _playlistImage, value);
    }

    public ObservableCollection<TrackInfo> Tracks { get; } = new();

    public ICommand ParseCommand { get; }

    public MainViewModel()
    {
    }

    public MainViewModel(IParserService parserService)
    {
        _parserService = parserService;
        ParseCommand = new AsyncRelayCommand(ParsePlaylistAsync);
    }

    private async Task ParsePlaylistAsync()
    {
        if (_parserService == null || string.IsNullOrWhiteSpace(Url))
        {
            StatusMessage = "Please enter a valid URL.";
            return;
        }

        try
        {
            IsLoading = true;
            StatusMessage = "Loading playlist data...";

            Tracks.Clear();
            PlaylistImage = null;

            var (playlist, tracks) = await _parserService.ParsePlaylistAsync(Url);

            CurrentPlaylist = playlist;
            
            if (!string.IsNullOrEmpty(playlist.AvatarUrl))
            {
                PlaylistImage = await ImageHelper.LoadFromWebAsync(playlist.AvatarUrl);
            }

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