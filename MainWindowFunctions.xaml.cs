using System.Text;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Cursach;

// Головне вікно програми, яке реалізує інтерфейси IPConnectionWindow та ISetSizeAddTitle
public sealed partial class MainWindow : IPConnectionWindow, ISetSizeAddTitle
{
    public MainWindow()
    {
        Instance = this; // Зберігаємо посилання на поточний екземпляр
        // Встановлюємо розміри і заголовок вікна
        SetSize(400, 500);
        // Ініціалізуємо інтерфейс користувача
        SetupUI();
    }

    // Метод для встановлення розміру вікна
    public void SetSize(double width, double height)
    {
        this.Width = width;
        this.Height = height;
        this.Title = "TCP Client Connection";
    }

    // Основний метод для налаштування інтерфейсу користувача
    private void SetupUI()
    {
        // Створюємо основний Grid для розміщення елементів
        Grid mainGrid = new Grid();
        this.Content = mainGrid;
        
        // Створюємо рядки для кращого розподілу простору
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(0.5, GridUnitType.Star) });  // Перша стрічка займає 50% простору
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });  // Стрічка для IP (автоматичний розмір)
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });  // Стрічка для Порту (автоматичний розмір)
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });  // Роздільник
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(2, GridUnitType.Star) });  // Стрічка для багатого текстового поля займає більше простору
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });  // Стрічка для панелі кнопок (автоматичний розмір)
        
        // Додаємо стовпці для кращого розміщення елементів форми
        mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });  // Стовпець для лейблів (IP і Port)
        mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });  // Стовпець для текстових полів займає весь доступний простір
        
        // IP адреса - використовуємо Grid замість окремих рядків для мітки і поля
        ipLabel = new Label
        {
            Content = "IP Address:",
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(10, 15, 5, 5)
        };
        Grid.SetRow(ipLabel, 0);
        Grid.SetColumn(ipLabel, 0);
        mainGrid.Children.Add(ipLabel);

        ipTextBox = new TextBox
        {
            Margin = new Thickness(5, 15, 10, 5),
            VerticalAlignment = VerticalAlignment.Center,
            Height = 25
        };
        Grid.SetRow(ipTextBox, 0);
        Grid.SetColumn(ipTextBox, 1);
        mainGrid.Children.Add(ipTextBox);

        // Порт - аналогічно до IP адреси
        portLabel = new Label
        {
            Content = "Port:",
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(10, 5, 5, 15)
        };
        Grid.SetRow(portLabel, 1);
        Grid.SetColumn(portLabel, 0);
        mainGrid.Children.Add(portLabel);

        portTextBox = new TextBox
        {
            Margin = new Thickness(5, 5, 10, 15),
            VerticalAlignment = VerticalAlignment.Center,
            Height = 25
        };
        Grid.SetRow(portTextBox, 1);
        Grid.SetColumn(portTextBox, 1);
        mainGrid.Children.Add(portTextBox);

        // Горизонтальний роздільник для візуального відокремлення
        Border separator = new Border
        {
            BorderBrush = Brushes.LightGray,
            BorderThickness = new Thickness(0, 1, 0, 0),
            Margin = new Thickness(10, 0, 10, 10)
        };
        Grid.SetRow(separator, 2);
        Grid.SetColumnSpan(separator, 2);
        mainGrid.Children.Add(separator);

        // RichTextBox для логування займає більше вертикального простору
        logTextBox = new RichTextBox
        {
            Margin = new Thickness(10),
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            IsReadOnly = true // Додаємо readonly для логів
        };
        Grid.SetRow(logTextBox, 4);
        Grid.SetColumnSpan(logTextBox, 2);
        mainGrid.Children.Add(logTextBox);

        // Панель з кнопками
        StackPanel buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(10)
        };
        Grid.SetRow(buttonPanel, 5);
        Grid.SetColumnSpan(buttonPanel, 2);
        mainGrid.Children.Add(buttonPanel);

        // Кнопка підключення
        connectButton = new Button
        {
            Content = "Connect to Server",
            Padding = new Thickness(15, 8, 15, 8),
            Margin = new Thickness(5),
            MinWidth = 120
        };
        connectButton.Click += ConnectButton_Click;
        buttonPanel.Children.Add(connectButton);

        // Додаткова кнопка для відключення (за потреби)
        disconnectButton = new Button
        {
            Content = "Disconnect",
            Padding = new Thickness(15, 8, 15, 8),
            Margin = new Thickness(5),
            MinWidth = 120,
            IsEnabled = false
        };
        disconnectButton.Click += DisconnectButton_Click;
        buttonPanel.Children.Add(disconnectButton);
    }

    // Обробник події натискання кнопки підключення
    private async void ConnectButton_Click(object sender, RoutedEventArgs e)
    {
        string iptext = ipTextBox.Text;
        string porttext = portTextBox.Text;

        // Вимикаємо елементи управління під час з'єднання
        connectButton.IsEnabled = false;
        ipTextBox.IsEnabled = false;
        portTextBox.IsEnabled = false;

        LogMessage($"Connecting to server at {iptext}:{porttext} ...", Colors.Black);

        try
        {
            tcpClient = new TcpClient();

            var connectTask = tcpClient.ConnectAsync(iptext, int.Parse(porttext));
            await Task.WhenAny(connectTask, Task.Delay(5000));

            if (!tcpClient.Connected)
            {
                throw new TimeoutException("Connection attempt timed out after 5 seconds");
            }

            networkStream = tcpClient.GetStream();
            LogMessage("Connected successfully!", Colors.Green);
            disconnectButton.IsEnabled = true;

            // Відкриваємо вікно логіну після успішного підключення
            loginWindow = new LoginWindow();
            loginWindow.Owner = this;
            loginWindow.Show();
            loginWindow.Closed += LoginWindow_Closed;
            
            cancellationTokenSource = new CancellationTokenSource();
        }
        catch (Exception ex)
        {
            HandleConnectionError(ex);
        }
    }

    // Метод для авторизації користувача
    public void Login()
    {
        try
        {
            if (!string.IsNullOrEmpty(UserName) && !string.IsNullOrEmpty(Password))
            {
                string ReceivedData = $"Username: {UserName}, Password: {Password}";
                SendToServerLogin(ReceivedData);
            }
            else
            {
                LogMessage("No data to send.", Colors.Orange);
            }
        }
        catch (Exception ex)
        {
            LogMessage($"Error sending received data: {ex.Message}", Colors.Red);
        }
        finally
        {
            // Вимикаємо кнопку підключення після відправлення даних
            connectButton.IsEnabled = false;
            ipTextBox.IsEnabled = false;
            portTextBox.IsEnabled = false;
            disconnectButton.IsEnabled = true;
        }
    }

    // Метод для отримання списку користувачів з сервера
    public async void GetUsersFromServer()
    {
        LogMessage("Getting users from server...", Colors.Black);

        try
        {
            if (tcpClient != null && tcpClient.Connected && networkStream != null)
            {
                byte[] data = Encoding.UTF8.GetBytes("GET_USERS");
                await networkStream.WriteAsync(data, 0, data.Length);

                LogMessage($"Sent: {"GET_USERS"}", Colors.Blue);

                // Очікування відповіді від сервера
                byte[] buffer = new byte[1024];
                int bytesRead = await networkStream.ReadAsync(buffer, 0, buffer.Length);
                
                string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                
                if (response != null)
                {
                    LogMessage($"Server response: {response}", Colors.Green);
                    response = response.Trim();

                    // Фільтрація отриманих користувачів (видаляємо поточного користувача)
                    string[] users = response.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    if (UserName != users[0]) users[0] = " " + users[0];
                    string[] newUsers = users.Where(item => !string.Equals(item.Trim(), UserName, StringComparison.OrdinalIgnoreCase)).ToArray();
                    foreach (string user in newUsers)
                    {
                        if (chatWindow != null && chatWindow.userList != null)
                        {
                            chatWindow.userList.Items.Add(user);
                        }
                    }
                }
                else
                {
                    LogMessage("Failed to receive server response", Colors.Red);
                }
            }
            else
            {
                LogMessage("Cannot send data - not connected", Colors.Red);
            }
        }
        catch (Exception ex)
        {
            LogMessage($"Error sending data: {ex.Message}", Colors.Red);
            DisconnectFromServer();
        }
        finally
        {
            // Вимикаємо кнопку підключення після відправлення даних
            connectButton.IsEnabled = false;
            ipTextBox.IsEnabled = false;
            portTextBox.IsEnabled = false;
            disconnectButton.IsEnabled = true;
        }
    }

    // Метод для відправлення повідомлення
    public async void SendMessage(string selectedUser, string message)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                if (message.Length > 100)
                {
                    MessageBox.Show("Повідомлення занадто довге.");
                    return;
                }

                LastMessageTime = new DateTime();
                LogMessage($"Time sending: {LastMessageTime}", Colors.Blue);;
                chatWindow.inputBox.Clear();
                chatWindow.chatScroll.ScrollToBottom();
            }
            if (tcpClient != null && tcpClient.Connected && networkStream != null)
            {
                string query = $"SEND_MESSAGE [From: {UserName}, To: {selectedUser}, Message: {message}]";
                byte[] data = Encoding.UTF8.GetBytes(query);
                await networkStream.WriteAsync(data, 0, data.Length);

                LogMessage($"Sent: {query}", Colors.Blue);

                // Очікування підтвердження від сервера
                byte[] buffer = new byte[1024];
                int bytesRead = await networkStream.ReadAsync(buffer, 0, buffer.Length);
                string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                chatWindow.RefreshChat();
            }
            else
            {
                LogMessage("Cannot send data - not connected", Colors.Red);
            }
        }
        catch (Exception ex)
        {
            LogMessage($"Error sending data: {ex.Message}", Colors.Red);
            DisconnectFromServer();
        }
    }

    // Метод для отримання історії чату
    public async void GetChat(string selectedUser)
    {
        LogMessage("Getting chat from server...", Colors.Black);

        try
        {
            if (tcpClient != null && tcpClient.Connected && networkStream != null)
            {
                string query = $"GET_CHAT [User: {UserName}, Second person: {selectedUser}]";
                byte[] data = Encoding.UTF8.GetBytes(query);
                await networkStream.WriteAsync(data, 0, data.Length);

                LogMessage($"Sent: {query}", Colors.Blue);

                // Отримання великого блоку даних (2048 байт)
                byte[] buffer = new byte[2048];
                int bytesRead = await networkStream.ReadAsync(buffer, 0, buffer.Length);
                string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                LogMessage($"Received: Client get message", Colors.Green);
                if (response != null)
                {
                    response = response.Trim();
                    if (chatWindow != null && chatWindow.chatContent != null)
                    {
                        ProcessChatResponse(response, chatWindow.chatContent);
                    }
                }
                else
                {
                    LogMessage("Failed to receive server response", Colors.Red);
                }
            }
            else
            {
                LogMessage("Cannot send data - not connected", Colors.Red);
            }
        }
        catch (Exception ex)
        {
            LogMessage($"Error sending data: {ex.Message}", Colors.Red);
            DisconnectFromServer();
        }
    }

    // Метод для видалення останнього повідомлення
    public async void DeleteLastMessage(string selectedUser)
    {
        LogMessage($"Deleting last message ...", Colors.Black);

        try
        {
            if (tcpClient != null && tcpClient.Connected && networkStream != null)
            {
                string query = $"DELETE_LAST_MESSAGE [From: {UserName}, To: {selectedUser}]";
                byte[] data = Encoding.UTF8.GetBytes(query);
                await networkStream.WriteAsync(data, 0, data.Length);

                LogMessage($"Sent: {query}", Colors.Blue);

                // Очікування підтвердження від сервера
                byte[] buffer = new byte[1024];
                int bytesRead = await networkStream.ReadAsync(buffer, 0, buffer.Length);
                string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                LogMessage($"Received: Server delete your last message", Colors.Green);
                if (response != null)
                {

                    chatWindow.RefreshChat();
                }
                else
                {
                    LogMessage($"Complete", Colors.Red);
                    LogMessage("Failed to receive server response", Colors.Red);
                }
            }
            else
            {
                LogMessage("Cannot send data - not connected", Colors.Red);
            }
        }
        catch (Exception ex)
        {
            LogMessage($"Error sending data: {ex.Message}", Colors.Red);
            DisconnectFromServer();
        }
    }

    // Обробник відповіді від сервера з історією чату
    private void ProcessChatResponse(string response, TextBlock chatContent)
    {
        if (string.IsNullOrEmpty(response))
        {
            LogMessage("Пуста відповідь від сервера", Colors.Red);
            return;
        }

        // Очищаємо поточний вміст чату
        chatContent.Text = string.Empty;

        // Розділяємо відповідь за новим рядком для отримання окремих повідомлень
        string[] chatLines = response.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (string line in chatLines)
        {
            // Парсимо рядок у форматі [Timestamp] User: Message
            if (line.Length > 0)
            {
                // Знаходимо дату і час
                int timeEndIndex = line.IndexOf(']');
                if (timeEndIndex > 0)
                {
                    // Знаходимо ім'я користувача
                    int usernameEndIndex = line.IndexOf(':', timeEndIndex);
                    if (usernameEndIndex > 0)
                    {
                        string timestamp = line.Substring(1, timeEndIndex - 1);
                        DateTime messageTime = DateTime.Parse(timestamp);

                        string username = line.Substring(timeEndIndex + 2, usernameEndIndex - timeEndIndex - 2);
                        string messageContent = line.Substring(usernameEndIndex + 2);

                        // Додаємо повідомлення у потрібному форматі
                        chatContent.Text += $"{messageTime:yyyy-MM-dd: HH:mm}: {username}: {messageContent}\n";
                    }
                }
            }
        }
    }


    // Відправка даних логіну на сервер
    private async void SendToServerLogin(string message)
    {
        try
        {
            if (tcpClient != null && tcpClient.Connected && networkStream != null)
            {
                byte[] data = Encoding.UTF8.GetBytes(message);
                await networkStream.WriteAsync(data, 0, data.Length);

                LogMessage($"Sent: {message}", Colors.Blue);

                byte[] buffer = new byte[1024];
                int bytesRead = await networkStream.ReadAsync(buffer, 0, buffer.Length);
                string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                if (response == "accept")
                {
                    LogMessage("Server response: OK", Colors.Green);

                    acceptWindow = new AcceptWindow();
                    acceptWindow.Show();

                    // Створення нового вікна ChatWindow
                    chatWindow = new ChatWindow();
                    chatWindow.Owner = this;
                    chatWindow.Show();

                    // Відключення події закриття для loginWindow, щоб при закритті воно не викликало DisconnectFromServer
                    loginWindow.Closed -= LoginWindow_Closed;

                    // Додаємо нову подію для chatWindow
                    chatWindow.Closed += ChatWindow_Closed;

                    // Закриваємо loginWindow
                    loginWindow.Close();
                }
                else if (response == "reject")
                {
                    LogMessage("Server response: REJECT", Colors.Red);
                }
                else
                {
                    LogMessage("Failed to receive server response", Colors.Red);
                }
            }
            else
            {
                LogMessage("Cannot send data - not connected", Colors.Red);
            }
        }
        catch (Exception ex)
        {
            LogMessage($"Error sending data: {ex.Message}", Colors.Red);
            DisconnectFromServer();
        }
    }

    // Метод для відключення від сервера
    private void DisconnectFromServer()
    {
        try
        {
            cancellationTokenSource?.Cancel();

            networkStream?.Close();
            tcpClient?.Close();
            loginWindow?.Close();
            chatWindow?.Close();

            networkStream = null;
            tcpClient = null;

            Application.Current.Dispatcher.Invoke(() =>
            {
                connectButton.IsEnabled = true;
                disconnectButton.IsEnabled = false;
                ipTextBox.IsEnabled = true;
                portTextBox.IsEnabled = true;

                LogMessage("Disconnected from server", Colors.Green);
            });
        }
        catch (Exception ex)
        {
            LogMessage($"Error during disconnection: {ex.Message}", Colors.Red);
        }
    }

    // Обробка помилок підключення до серевера
    private void HandleConnectionError(Exception ex)
    {
        string errorMessage = ex.Message;

        if (ex is SocketException socketEx)
        {
            errorMessage = $"Socket error: {socketEx.Message} (Error code: {socketEx.SocketErrorCode})";
        }
        else if (ex is TimeoutException)
        {
            errorMessage = "Connection timed out. Server may be unreachable.";
        }

        LogMessage(errorMessage, Colors.Red);
        LogMessage($"Exception type: {ex.GetType().Name}", Colors.DarkRed);

        // Відновлюємо UI
        connectButton.IsEnabled = true;
        ipTextBox.IsEnabled = true;
        portTextBox.IsEnabled = true;

        // Закриваємо підключення у випадку помилки
        if (tcpClient != null)
        {
            tcpClient.Close();
            tcpClient = null;
        }
    }

    // Обробник кнопки відключення
    private void DisconnectButton_Click(object sender, RoutedEventArgs e)
    {
        disconnectButton.IsEnabled = false;

        try
        {
            LogMessage("Disconnecting from server...", Colors.Black);

            // Надсилаємо повідомлення про відключення на сервер (опціонально)
            try
            {
                if (tcpClient != null && tcpClient.Connected && networkStream != null)
                {
                    byte[] disconnectMessage = Encoding.UTF8.GetBytes("DISCONNECT");
                    networkStream.Write(disconnectMessage, 0, disconnectMessage.Length);
                    LogMessage("Disconnect notification sent to server", Colors.Blue);
                }
            }
            catch (Exception sendEx)
            {
                LogMessage($"Could not send disconnect notification: {sendEx.Message}", Colors.Orange);
            }

            DisconnectFromServer();
        }
        catch (Exception ex)
        {
            // Обробка будь-яких помилок під час процесу відключення
            LogMessage($"Error during disconnection: {ex.Message}", Colors.Red);
            LogMessage($"Exception type: {ex.GetType().Name}", Colors.DarkRed);

            // Спроба аварійного відключення
            try
            {
                cancellationTokenSource?.Cancel();

                networkStream?.Close();
                tcpClient?.Close();
                loginWindow?.Close();
                chatWindow?.Close();

                networkStream = null;
                tcpClient = null;
            }
            catch { /* Ігноруємо будь-які помилки в аварійному відключенні */ }

            connectButton.IsEnabled = true;
            disconnectButton.IsEnabled = false;
            ipTextBox.IsEnabled = true;
            portTextBox.IsEnabled = true;
        }
        finally
        {
            LogMessage("Disconnection process completed", Colors.Green);
        }
    }    

    // Метод для логування повідомлень з кольором
    private void LogMessage(string message, Color color)
    {
        TextRange tr = new TextRange(logTextBox.Document.ContentEnd, logTextBox.Document.ContentEnd);
        tr.Text = $"[{DateTime.Now:HH:mm:ss}] {message}\r\n";
        tr.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(color));
        logTextBox.ScrollToEnd();
    }

    // Обробники подій закриття вікон
    private void LoginWindow_Closed(object sender, EventArgs e)
    {
        DisconnectFromServer();
    }

    private void ChatWindow_Closed(object sender, EventArgs e)
    {
        chatWindow.Closed -= ChatWindow_Closed;
        chatWindow?.Close();
        loginWindow = new LoginWindow();
        loginWindow.Owner = this;
        loginWindow.Show();
        loginWindow.Closed += LoginWindow_Closed;
        UserName = null;
        Password = null;
        Message = null;
    }
}
