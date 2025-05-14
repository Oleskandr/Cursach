using System.Net.Sockets;

namespace Cursach;

// Основне вікно програми, що реалізує інтерфейси IPConnectionWindow та ISetSizeAddTitle
// IPConnectionWindow — абстрактний клас для роботи з підключенням до сервера
// ISetSizeAddTitle — інтерфейс для налаштування розміру вікна та додавання заголовка
public sealed partial class MainWindow : IPConnectionWindow, ISetSizeAddTitle
{
    // Вікно для входу в систему
    private LoginWindow loginWindow;
    
    // Вікно для чату
    private ChatWindow chatWindow;
    
    // TCP клієнт для підключення до сервера
    private TcpClient tcpClient = null;
    
    // Потік для взаємодії з мережею
    private NetworkStream networkStream;
    
    // Джерело для скасування асинхронних операцій (наприклад, для переривання з'єднання)
    private CancellationTokenSource cancellationTokenSource;

    // Публічні змінні для передачі даних між вікнами (наприклад, логін та пароль)
    
    // Ім'я користувача для аутентифікації
    public string UserName { get; set; }

    // Пароль користувача для аутентифікації
    public string Password { get; set; }

    // Повідомлення, яке відправляється в чат
    public string Message { get; set; }

    // Синглтон для доступу до екземпляра MainWindow
    public static MainWindow Instance { get; private set; }

    // Час останнього повідомлення в чаті
    public DateTime LastMessageTime { get; set; }

    // Останнє повідомлення, яке було надіслано або отримано
    public string LastMessage { get; set; }
}
