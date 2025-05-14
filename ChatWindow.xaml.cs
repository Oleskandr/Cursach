using System.Windows;
using System.Windows.Controls;

namespace Cursach;

public class ChatWindow : Window, ISetSizeAddTitle
{
    public ListBox userList;  // Список користувачів
    public ScrollViewer chatScroll;  // Прокручуваний контейнер для чату
    public TextBlock chatContent;  // Вміст чату
    public TextBox inputBox;  // Поле для введення повідомлення
    private Button sendButton;  // Кнопка для відправлення повідомлення
    private Button deleteButton;  // Кнопка для видалення повідомлення

    // Конструктор вікна чату
    public ChatWindow()
    {
        SetSize(800, 600);  // Встановлюємо розмір вікна
        SetupUI();  // Налаштовуємо інтерфейс
    }

    // Встановлення розміру вікна та заголовку
    public void SetSize(double width, double height)
    {
        this.Width = width;
        this.Height = height;
        this.Title = "Chat";  // Заголовок вікна
    }

    // Налаштовуємо елементи інтерфейсу (ListBox, ScrollViewer, TextBox, Buttons)
    private void SetupUI()
    {
        Grid grid = new Grid();  // Контейнер для всіх елементів
        grid.Margin = new Thickness(10);  // Встановлюємо відступи
        this.Content = grid;  // Встановлюємо Grid як вміст вікна

        // Створюємо колонки та рядки для розміщення елементів
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });  // Перша колонка займає частину простору (1 з 3)
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });  // Друга колонка займає більшу частину простору (2 з 3)
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });  // Перший рядок займає частину вертикального простору (1 з 3)
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });  // Другий рядок для TextBox (автоматична висота залежно від вмісту)
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });  // Третій рядок для кнопки "Send" (автоматична висота)
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });  // Четвертий рядок для кнопки "Delete" (автоматична висота)

        // Список користувачів (ListBox)
        userList = new ListBox();
        Grid.SetColumn(userList, 0);
        Grid.SetRow(userList, 0);
        grid.Children.Add(userList);

        if (MainWindow.Instance != null)
        {
            MainWindow.Instance.GetUsersFromServer();  // Заповнюємо список користувачів
        }

        // ScrollViewer для чату
        chatScroll = new ScrollViewer();
        chatScroll.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
        chatContent = new TextBlock();
        chatContent.TextWrapping = TextWrapping.Wrap;
        chatScroll.Content = chatContent;
        chatContent.Text = "Виберіть користувача для перегляду чату";  // Текст за замовчуванням
        chatScroll.Margin = new Thickness(5, 0, 0, 0);
        Grid.SetColumn(chatScroll, 1);
        Grid.SetRow(chatScroll, 0);
        grid.Children.Add(chatScroll);

        // TextBox для введення повідомлення
        inputBox = new TextBox();
        inputBox.Margin = new Thickness(0, 5, 0, 0);
        Grid.SetColumn(inputBox, 1);
        Grid.SetRow(inputBox, 1);
        grid.Children.Add(inputBox);

        // Кнопка "Send"
        sendButton = new Button { Content = "Send" };
        sendButton.Margin = new Thickness(0, 5, 0, 0);
        Grid.SetColumn(sendButton, 1);
        Grid.SetRow(sendButton, 2);
        grid.Children.Add(sendButton);

        // Кнопка "Delete"
        deleteButton = new Button { Content = "Delete" };
        deleteButton.Margin = new Thickness(0, 5, 0, 0);
        Grid.SetColumn(deleteButton, 1);
        Grid.SetRow(deleteButton, 3);
        grid.Children.Add(deleteButton);

        // Обробка подій
        sendButton.Click += SendButton_Click;  // Кнопка для відправлення повідомлення
        deleteButton.Click += DeleteButton_Click;  // Кнопка для видалення повідомлення
        userList.SelectionChanged += UserList_SelectionChanged;  // Вибір користувача зі списку
    }

    // Обробник натискання на кнопку "Send"
    private void SendButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            string selectedUser = userList.SelectedItem.ToString();  // Отримуємо вибраного користувача
            string message = inputBox.Text.Trim();  // Отримуємо текст повідомлення

            if (MainWindow.Instance != null)
            {
                MainWindow.Instance.Message = message;
                MainWindow.Instance.SendMessage(selectedUser, message);  // Відправляємо повідомлення
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Сталася помилка: {ex.Message}");  // Повідомлення про помилку
        }
    }

    // Обробник натискання на кнопку "Delete"
    public void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (userList.SelectedItem != null)
        {
            string selectedUser = userList.SelectedItem.ToString();  // Отримуємо вибраного користувача

            if (MainWindow.Instance != null)
            {
                MainWindow.Instance.DeleteLastMessage(selectedUser);  // Видаляємо останнє повідомлення
            }
        }
    }

    // Оновлення чату при виборі користувача
    public void RefreshChat()
    {
        if (userList.SelectedItem != null)
        {
            string selectedUser = userList.SelectedItem.ToString();  // Отримуємо вибраного користувача
            if (MainWindow.Instance != null)
            {
                MainWindow.Instance.GetChat(selectedUser);  // Отримуємо чат для вибраного користувача
            }
        }
    }

    // Обробник зміни вибору користувача в ListBox
    private void UserList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (userList.SelectedItem != null)
        {
            string selectedUser = userList.SelectedItem.ToString();  // Отримуємо вибраного користувача
            if (MainWindow.Instance != null)
            {
                MainWindow.Instance.GetChat(selectedUser);  // Отримуємо чат для вибраного користувача
            }
        }
    }
}
