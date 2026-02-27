using Microsoft.AspNetCore.SignalR.Client;
using TaskManager.Application.TodoItems.DTOs;
using static System.Net.WebRequestMethods;

namespace TaskManager.Presentation.Services
{
    public class TodoItemStateService 
    {
        private HubConnection _hubConnection;
        public event Action? OnStateChanged;
        private readonly HttpClient _http;

        public HubConnectionState HubState => _hubConnection?.State ?? HubConnectionState.Disconnected;

        public bool IsConnected => HubState == HubConnectionState.Connected;

        public List<TodoItemEntry> Items { get; private set; }

        public TodoItemStateService(HttpClient http)
        {
            _http = http; 

            _hubConnection = new HubConnectionBuilder()
                .WithUrl(("https://localhost:7109/taskhub"))
                .ConfigureLogging(logging => {
                    logging.SetMinimumLevel(LogLevel.Information);
                    logging.AddConsole(); 
                })
                .WithAutomaticReconnect()
                .Build();

            _hubConnection.On("TaskUpdated", async () =>
            {
                await LoadTodoItemsAsync();
                NotifyStateChanged();
            });
        }

        private async Task LoadTodoItemsAsync()
        {
            try
            {
                var response = await _http.GetFromJsonAsync<List<TodoItemEntry>>("api/todoitems/MyAssignedTasks");

                // Update the "Countertop" (the list the UI binds to)
                Items = response ?? new();

                // Shout to the UI that data has changed
                NotifyStateChanged();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading tasks: {ex.Message}");
            }
        }

        public async Task InitializeAsync()
        {
            if (_hubConnection.State == HubConnectionState.Disconnected)
            {
                await _hubConnection.StartAsync();
            }
        }

        private void NotifyStateChanged() => OnStateChanged?.Invoke();

        public async Task EnsureConnectedAsync()
        {
            if (_hubConnection.State == HubConnectionState.Disconnected)
            {
                try
                {
                    await _hubConnection.StartAsync();
                    Console.WriteLine("Starting SignalR");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"SignalR Connection Error: {ex.Message}");
                }
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_hubConnection is not null)
            {
                await _hubConnection.DisposeAsync();
            }
        }

    }
}
