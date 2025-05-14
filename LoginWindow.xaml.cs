using System.Windows;
using System.Windows.Controls;

namespace Cursach;

// Основне вікно програми, яке реалізує інтерфейси IPConnectionWindow та ISetSizeAddTitle
// ISetSizeAddTitle — інтерфейс для налаштування розміру вікна та додавання заголовка
public class LoginWindow : Window, ISetSizeAddTitle
{
    // Поля для елементів інтерфейсу
    private TextBox usernameTextBox;  // Поле для введення імені користувача
    private PasswordBox passwordTextBox;  // Поле для введення паролю
    private Button loginButton;  // Кнопка для здійснення входу

    // Конструктор вікна, ініціалізує інтерфейс та встановлює розмір
    public LoginWindow()
    {
        SetupUI();  // Налаштовуємо інтерфейс користувача
        SetSize(300, 200);  // Встановлюємо розміри вікна
    }

    // Реалізація методу з інтерфейсу для налаштування розміру та заголовка вікна
    public void SetSize(double width, double height)
    {
        this.Width = width;
        this.Height = height;
        this.Title = "Login";
    }

    // Налаштування елементів інтерфейсу (TextBox, PasswordBox, Label, Button)
    private void SetupUI()
    {
        Grid grid = new Grid();  // Створюємо контейнер Grid для розміщення елементів
        grid.Margin = new Thickness(10);  // Встановлюємо відступи для Grid

        // Додаємо стовпці для міток та полів вводу
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });  // Для міток
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });  // Для полів вводу

        // Додаємо рядки для кожного елемента інтерфейсу
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });  // Рядок для "Username"
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });  // Рядок для "Password"
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });  // Рядок для кнопки входу

        // Логін
        var usernameLabel = new Label { Content = "Username:", Margin = new Thickness(0, 0, 5, 5) };
        Grid.SetRow(usernameLabel, 0);
        Grid.SetColumn(usernameLabel, 0);
        grid.Children.Add(usernameLabel);

        usernameTextBox = new TextBox { Margin = new Thickness(0, 0, 0, 5) };  // Текстове поле для введення імені користувача
        Grid.SetRow(usernameTextBox, 0);
        Grid.SetColumn(usernameTextBox, 1); 
        grid.Children.Add(usernameTextBox);

        // Пароль
        var passwordLabel = new Label { Content = "Password:", Margin = new Thickness(0, 0, 5, 5) };
        Grid.SetRow(passwordLabel, 1);
        Grid.SetColumn(passwordLabel, 0);
        grid.Children.Add(passwordLabel); 

        passwordTextBox = new PasswordBox { Margin = new Thickness(0, 0, 0, 5) };  // Поле для введення паролю
        Grid.SetRow(passwordTextBox, 1);
        Grid.SetColumn(passwordTextBox, 1); 
        grid.Children.Add(passwordTextBox);

        // Кнопка входу
        loginButton = new Button
        {
            Content = "Login",
            Width = 80, 
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 10, 0, 0) 
        };
        Grid.SetRow(loginButton, 2); 
        Grid.SetColumnSpan(loginButton, 2);
        loginButton.Click += LoginButton_Click;  // Обробник події натискання кнопки
        grid.Children.Add(loginButton);  // Додаємо кнопку до Grid

        Content = grid;  // Встановлюємо Grid як вміст вікна
    }

    // Обробник події натискання кнопки входу
    private void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        string username = usernameTextBox.Text;  // Отримуємо введене ім'я користувача
        string password = passwordTextBox.Password;  // Отримуємо введений пароль

        // Перевірка на порожні поля вводу
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            MessageBox.Show("Будь ласка, введіть ім'я користувача та пароль", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;  // Якщо поля порожні, зупиняємо виконання
        }

        // Передача введених даних в основне вікно
        if (MainWindow.Instance != null)
        {
            MainWindow.Instance.UserName = username;  // Передаємо ім'я користувача в основне вікно
            MainWindow.Instance.Password = password;  // Передаємо пароль в основне вікно
            MainWindow.Instance.Login();  // Викликаємо метод входу
        }
    }
}
