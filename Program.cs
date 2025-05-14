using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Text.Json.Nodes;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Text.RegularExpressions;

// Клас, що представляє запис користувача
public class User
{
    public int RecordID { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
}

// Клас, що представляє повідомлення чату
public class ChatMessage
{
    public string From { get; set; }
    public string To { get; set; }
    public string Message { get; set; }
    public DateTime Time { get; set; }
}

// Клас для десеріалізації повідомлень з JSON
public class Message
{
    public string From { get; set; }
    public string To { get; set; }
    public string Content { get; set; }
    public string Timestamp { get; set; }
}

// Статичний клас для роботи з JSON даними
public static class JsonReader
{
    // Метод для отримання даних з JSON файлу за вказаними ключами
    public static JsonNode? GetJsonData(string filePath, params string[] keys)
    {
        try
        {
            string json = File.ReadAllText(filePath);
            JsonNode? node = JsonNode.Parse(json);

            // Проходимо по JSON об'єкту, використовуючи надані ключі
            foreach (string key in keys)
            {
                if (node == null)
                    return null;
                
                // Обробка індексації масиву з числовими ключами
                if (node is JsonArray array && int.TryParse(key, out int index))
                {
                    node = index >= 0 && index < array.Count ? array[index] : null;
                }
                // Обробка доступу до властивостей об'єкта
                else if (node is JsonObject obj)
                {
                    node = obj.TryGetPropertyValue(key, out var value) ? value : null;
                }
                else
                    return null;
            }

            return node;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading JSON file: {ex.Message}");
            return null;
        }
    }

    // Метод для отримання JSON значення як рядка
    public static string? GetJsonValueAsString(string filePath, params string[] keys)
    {
        var node = GetJsonData(filePath, keys);

        if (node == null)
            return null;

        // Повертаємо різні рядкові представлення в залежності від типу вузла
        return node switch
        {
            JsonArray => node.ToJsonString(),
            JsonObject => node.ToJsonString(),
            _ => node.ToString()
        };
    }

    // Метод для завантаження користувачів з JSON файлу
    public static List<User> LoadUsers(string filePath)
    {
        string json = File.ReadAllText(filePath);
        return JsonConvert.DeserializeObject<List<User>>(json);
    }

    // Метод для отримання імен користувачів з JSON файлу
    public static List<string> GetUserNames(string filePath)
    {
        try
        {
            string json = File.ReadAllText(filePath);
            var users = JsonConvert.DeserializeObject<List<User>>(json);
            return users?.Select(u => u.UserName).ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading JSON: {ex.Message}");
            return null;
        }
    }

    // Метод для перевірки облікових даних користувача
    public static bool ValidateCredentials(string message, List<User> users)
    {
        string[] parts = message.Split(", ");
        string username = null, password = null;

        // Видобуваємо ім'я користувача та пароль з повідомлення
        foreach (string part in parts)
        {
            if (part.StartsWith("Username:"))
                username = part.Substring("Username:".Length).Trim();
            else if (part.StartsWith("Password:"))
                password = part.Substring("Password:".Length).Trim();
        }

        // Перевіряємо облікові дані щодо списку користувачів
        foreach (User user in users)
        {
            if (user.UserName == username && user.Password == password)
                return true;
        }

        return false;
    }
}

// Головний клас програми
public class Program {
    
    private const string chats = "chats.json"; 

    public static void Main()
    {
        // Головний цикл програми
        while(true) {
        Console.WriteLine("Enter a command to proccess:\n|EXUSER - expect user for 60 seconds\n|ENDCMD - end program");
        string typeInput = Console.ReadLine();
        
        // Команда для очікування з'єднання користувача
        if (typeInput == "EXUSER") {
        int port = 8001;
        IPAddress localAddr = IPAddress.Any;
        // Створюємо TCP сокет
        Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        try {
            // Прив'язуємо сокет до локальної кінцевої точки та очікуємо вхідні з'єднання
            serverSocket.Bind(new IPEndPoint(localAddr, port));
            serverSocket.Listen(1);
            Console.WriteLine("Waiting for a client connection...");
            
            // Встановлюємо параметри тайм-ауту сокета (60 секунд)
            serverSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 60000); // 60 секунд у мілісекундах
            serverSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, 60000);
            
            // Перевіряємо наявність з'єднання протягом 60 секунд
            bool clientConnected = serverSocket.Poll(60000000, SelectMode.SelectRead); // 60 секунд у мікросекундах
            
            // Обробка з'єднання клієнта
            if (clientConnected) {
                Socket clientSocket = serverSocket.Accept();
                Console.WriteLine("Client connected.");
            byte[] buffer = new byte[1024]; 
            var users = JsonReader.LoadUsers("records.json");
            string filePath = "records.json";
            string response;
            byte[] responseBytes;
            
            // Цикл комунікації з клієнтом
            while (true)
            {
                try
                {
                    int bytesReceived = clientSocket.Receive(buffer);   
                    // Перевіряємо, чи клієнт відключився
                    if (bytesReceived == 0)
                    {
                        Console.WriteLine("Client disconnected gracefully.");
                        break;
                    }
                    string receivedText = Encoding.UTF8.GetString(buffer, 0, bytesReceived);
                    Console.WriteLine("Received: " + receivedText);
                    
                    // Обробка команд клієнта
                    if (receivedText == "DISCONNECT") break;
                    else if (receivedText == "GET_USERS")
                    {
                        // Отримуємо та надсилаємо список імен користувачів
                        var usernames = JsonReader.GetUserNames(filePath);
                        if (usernames != null && usernames.Any())
                        {
                            response = string.Join(", ", usernames);
                            responseBytes = Encoding.UTF8.GetBytes(response);
                        }
                        else
                        {
                            responseBytes = Encoding.UTF8.GetBytes("Type value \"UserName\" is empty");
                        }
                        clientSocket.Send(responseBytes);
                    }
                    else if (receivedText.StartsWith("GET_CHAT"))
                    {
                        // Витягуємо імена користувачів за допомогою регулярного виразу
                        Regex regex = new Regex(@"GET_CHAT \[User: (.*?), Second person:  (.*?)\]");
                        Match match = regex.Match(receivedText);

                        if (match.Success)
                        {
                            string userName = match.Groups[1].Value;
                            string selectedUser = match.Groups[2].Value;

                            Console.WriteLine($"Запит на чат між {userName} та {selectedUser}");

                            // Отримуємо та надсилаємо повідомлення чату
                            string chatMessages = GetChatMessages(userName, selectedUser, chats);
                            responseBytes = Encoding.UTF8.GetBytes(chatMessages);
                            clientSocket.Send(responseBytes);
                        }
                        else
                        {
                            responseBytes = Encoding.UTF8.GetBytes("ERROR: Невірний формат запиту чату");
                            clientSocket.Send(responseBytes);
                        }
                    }
                    else if (receivedText.StartsWith("SEND_MESSAGE"))
                    {
                        // Обробляємо надсилання повідомлення
                        ProcessSendMessage(receivedText, "chats.json");
                        responseBytes = Encoding.UTF8.GetBytes("ACK: Message processed");
                        clientSocket.Send(responseBytes);
                    }
                    else if (receivedText.StartsWith("DELETE_LAST_MESSAGE"))
                    {
                        // Видаляємо останнє повідомлення
                        ProcessDeleteLastMessage(receivedText, chats);
                        responseBytes = Encoding.UTF8.GetBytes("ACK: Last message deleted");
                        clientSocket.Send(responseBytes);
                    }
                    else
                    {
                        // Перевіряємо облікові дані користувача
                        bool isValid = JsonReader.ValidateCredentials(receivedText, users);
                        response = isValid ? "accept" : "reject";   
                        responseBytes = Encoding.UTF8.GetBytes(response);
                        clientSocket.Send(responseBytes);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception: " + ex.Message);
                    break;
                }}
                // Закриваємо з'єднання з клієнтом
                clientSocket.Shutdown(SocketShutdown.Both);
                clientSocket.Close();
            }            
            else {
                Console.WriteLine("Клієнт не підключився за 60 секунд");
            }
        }
        catch (SocketException se) {
            // Обробка тайм-ауту з'єднання
            if (se.SocketErrorCode == SocketError.TimedOut) {
                Console.WriteLine("Клієнт не підключився за 60 секунд");
            } else {
                Console.WriteLine("Socket Exception: " + se.Message);
            }
        }
        catch (Exception ex) {
            Console.WriteLine("Exception: " + ex.Message);
        }
        finally {
            serverSocket.Close();
        }}
        // Команда для завершення програми
        else if (typeInput == "ENDCMD")
        {
            Console.WriteLine("Server end work!");
            break;
        }
        else
        {
            Console.WriteLine("Type of command unknown! Try again!");
        }
        }
    }

    // Метод для отримання повідомлень чату між двома користувачами
    private static string GetChatMessages(string userName, string selectedUser, string jsonFilePath)
    {
        try
        {
            // Перевіряємо чи існує JSON файл
            if (!File.Exists(jsonFilePath))
            {
                return "ERROR: Файл з повідомленнями не знайдено";
            }

            // Читаємо та десеріалізуємо JSON файл
            string jsonContent = File.ReadAllText(jsonFilePath);
            List<Message> allMessages = JsonConvert.DeserializeObject<List<Message>>(jsonContent);

            if (allMessages == null)
            {
                return "ERROR: Помилка десеріалізації JSON";
            }

            // Фільтруємо повідомлення між вказаними користувачами
            List<Message> chatMessages = new List<Message>();
            foreach (Message msg in allMessages)
            {
                bool fromToMatch = msg.From.Equals(userName, StringComparison.OrdinalIgnoreCase) && 
                                  msg.To.Equals(selectedUser, StringComparison.OrdinalIgnoreCase);
                bool toFromMatch = msg.From.Equals(selectedUser, StringComparison.OrdinalIgnoreCase) && 
                                  msg.To.Equals(userName, StringComparison.OrdinalIgnoreCase);

                if (fromToMatch || toFromMatch)
                {
                    chatMessages.Add(msg);
                }
            }

            // Створюємо результуючий рядок
            if (chatMessages.Count == 0)
            {
                return "Повідомлень не знайдено";
            }

            // Формуємо відформатований список повідомлень
            StringBuilder chatBuilder = new StringBuilder();
            foreach (Message msg in chatMessages)
            {
                chatBuilder.AppendLine($"[{msg.Timestamp}] {msg.From}: {msg.Content}");
            }

            return chatBuilder.ToString();
        }
        catch (Exception e)
        {
            return $"ERROR: {e.Message}";
        }
    }

    // Метод для обробки та збереження нового повідомлення
    private static void ProcessSendMessage(string receivedText, string filePath)
    {
        try
        {
            // Аналізуємо вхідне повідомлення за допомогою регулярного виразу
            Regex regex = new Regex(@"SEND_MESSAGE \[From: (.*?), To: (.*?), Message: (.*?)\]");
            Match match = regex.Match(receivedText);

            if (!match.Success)
            {
                Console.WriteLine("ERROR: Invalid message format");
                return;
            }

            // Створюємо об'єкт повідомлення
            Message newMessage = new Message
            {
                From = match.Groups[1].Value.Trim(),
                To = match.Groups[2].Value.Trim(),
                Content = match.Groups[3].Value.Trim(),
                Timestamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss")
            };

            // Перевіряємо обов'язкові поля
            if (string.IsNullOrEmpty(newMessage.From) || 
                string.IsNullOrEmpty(newMessage.To) || 
                string.IsNullOrEmpty(newMessage.Content))
            {
                Console.WriteLine("ERROR: Required fields are missing");
                return;
            }

            // Робота з JSON файлом
            List<Message> messages = new List<Message>();

            // Завантажуємо існуючі повідомлення, якщо файл існує
            if (File.Exists(filePath))
            {
                string existingJson = File.ReadAllText(filePath);
                messages = JsonConvert.DeserializeObject<List<Message>>(existingJson) ?? new List<Message>();
            }

            // Додаємо нове повідомлення
            messages.Add(newMessage);

            // Серіалізуємо з правильними налаштуваннями
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            string updatedJson = JsonConvert.SerializeObject(messages, settings);
            File.WriteAllText(filePath, updatedJson);

            Console.WriteLine($"Message from {newMessage.From} to {newMessage.To} saved successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception in ProcessSendMessage: {ex.Message}");
        }
    }

    // Метод для видалення останнього повідомлення між двома користувачами
    private static void ProcessDeleteLastMessage(string receivedText, string jsonFilePath)
    {
        try
        {
            // Аналізуємо вхідне повідомлення
            var parts = receivedText.Split(new[] { '[', ']' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2) return;

            // Витягуємо параметри користувачів
            var parameters = parts[1].Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
            string fromUser = parameters[0].Replace("From: ", "").Trim();
            string toUser = parameters[1].Replace("To: ", "").Trim();

            // Читаємо JSON файл
            var jsonContent = File.ReadAllText(jsonFilePath);
            var allMessages = JsonConvert.DeserializeObject<List<Message>>(jsonContent) 
                             ?? new List<Message>();

            // Знаходимо останнє повідомлення між цими користувачами
            var lastMessage = allMessages
                .Where(m => (m.From == fromUser && m.To == toUser) || 
                           (m.From == toUser && m.To == fromUser))
                .LastOrDefault();

            // Видаляємо повідомлення, якщо знайдено
            if (lastMessage != null)
            {
                allMessages.Remove(lastMessage);
                File.WriteAllText(jsonFilePath, 
                    JsonConvert.SerializeObject(allMessages, Formatting.Indented));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    // Допоміжний метод для створення унікального ключа чату
    private string GetChatKey(string user1, string user2)
    {
        // Сортуємо імена користувачів для забезпечення узгодженого ключа незалежно від порядку
        var users = new[] { user1, user2 }.OrderBy(u => u).ToArray();
        return $"{users[0]}_{users[1]}"; // Приклад: "Alice_Bob"
    }
}
 
