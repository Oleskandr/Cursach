using System.Windows;
using System.Windows.Controls;

namespace Cursach;

// Основне вікно програми, яке реалізує інтерфейси IPConnectionWindow та ISetSizeAddTitle
// ISetSizeAddTitle — інтерфейс для налаштування розміру вікна та додавання заголовка
public class AcceptWindow : Window, ISetSizeAddTitle
{
    // Поля для елементів інтерфейсу
    private TextBox acceptTextBox;  // Поле для введення імені користувача

    // Конструктор вікна чату
    public AcceptWindow()
    {
        SetSize(250, 250);  // Встановлюємо розмір вікна
        SetupUI();  // Налаштовуємо інтерфейс
    }

    // Реалізація методу з інтерфейсу для налаштування розміру та заголовка вікна
    public void SetSize(double width, double height)
    {
        this.Width = width;
        this.Height = height;
        this.Title = "Accept";
    }

    private void SetupUI()
    {
        // Створення Grid
        var grid = new Grid();
        this.Content = grid;  // Встановлюємо Grid як вміст вікна

        // Можна додатково налаштувати розміри для Grid, якщо потрібно
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        // Створення текстового поля для вітання
        acceptTextBox = new TextBox();
        acceptTextBox.Margin = new Thickness(10);
        acceptTextBox.Text = "Вітаю, ти успішно залогінився!";
        acceptTextBox.IsReadOnly = true;  // Поле тільки для читання
        acceptTextBox.HorizontalAlignment = HorizontalAlignment.Center;  // Центрування по горизонталі
        acceptTextBox.VerticalAlignment = VerticalAlignment.Center;      // Центрування по вертикалі

        // Додаємо текстове поле в Grid
        grid.Children.Add(acceptTextBox);
    }
}
