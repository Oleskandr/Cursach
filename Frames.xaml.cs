using System.Windows;
using System.Windows.Controls;

namespace Cursach;

// Абстрактний клас IPConnectionWindow, який розширює клас Window
// Цей клас є базовим для всіх вікон, що мають елементи для підключення до мережі.
public abstract class IPConnectionWindow : Window
{
    // Етикетка для відображення "IP адреси"
    protected Label ipLabel;

    // Етикетка для відображення "Порту"
    protected Label portLabel;

    // Текстове поле для введення IP адреси
    protected TextBox ipTextBox;

    // Текстове поле для введення порту
    protected TextBox portTextBox;

    // Текстове поле для виведення журналу повідомлень або логів
    protected RichTextBox logTextBox;

    // Кнопка для підключення
    protected Button connectButton;

    // Кнопка для роз'єднання
    protected Button disconnectButton;
}

// Інтерфейс ISetSizeAddTitle
// Це інтерфейс, який задає методи для налаштування розміру вікна та заголовка
public interface ISetSizeAddTitle
{
    // Метод для встановлення розміру вікна
    // Приймає два параметри: ширину (width) та висоту (height) вікна
    public void SetSize(double width, double height);
}
