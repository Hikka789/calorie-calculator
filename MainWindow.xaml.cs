using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace Fitnes
{
    public partial class MainWindow : Window
    {
        private ObservableCollection<Exercise> _selectedExercises = new ObservableCollection<Exercise>();

        public MainWindow()
        {
            InitializeComponent();
        }

        // ============ ВКЛАДКА «ПРОФИЛЬ» ============

        private void BtnCalculate_Click(object sender, RoutedEventArgs e)
        {
            if (!double.TryParse(TbWeight.Text, out double weight) ||
                !double.TryParse(TbHeight.Text, out double height) ||
                !int.TryParse(TbAge.Text, out int age))
            {
                TbCalories.Text = "Проверьте введённые данные";
                return;
            }

            double bmr = (CbGender.SelectedIndex == 0)
                ? 10 * weight + 6.25 * height - 5 * age + 5
                : 10 * weight + 6.25 * height - 5 * age - 161;

            double factor = CbActivity.SelectedIndex switch
            {
                0 => 1.2,
                1 => 1.55,
                2 => 1.725,
                _ => 1.2
            };

            TbCalories.Text = $"Норма калорий: {Math.Round(bmr * factor)} ккал/день";
        }

        private void BtnSaveProfile_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Профиль сохранён (заглушка)");
        }

        // ============ ВКЛАДКА «УПРАЖНЕНИЯ» ============

        private void CbMuscleGroup_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LbExercises == null) return;
            LbExercises.Items.Clear();

            if (CbMuscleGroup.SelectedItem is not ComboBoxItem item) return;
            string group = item.Content?.ToString() ?? string.Empty;

            string[] exercises = group switch
            {
                "Грудь" => new[] { "Жим лёжа", "Отжимания", "Разводка гантелей" },
                "Спина" => new[] { "Становая тяга", "Тяга в наклоне", "Подтягивания" },
                "Ноги" => new[] { "Приседания", "Выпады", "Жим ногами" },
                _ => Array.Empty<string>()
            };

            foreach (var name in exercises)
            {
                LbExercises.Items.Add(new CheckBox
                {
                    Content = name,
                    Tag = group
                });
            }
        }

        private void BtnAddToProgram_Click(object sender, RoutedEventArgs e)
        {
            foreach (var obj in LbExercises.Items)
            {
                if (obj is CheckBox cb && cb.IsChecked == true)
                {
                    string name = cb.Content?.ToString() ?? string.Empty;
                    if (!_selectedExercises.Any(x => x.Name == name))
                    {
                        _selectedExercises.Add(new Exercise
                        {
                            Name = name,
                            MuscleGroup = cb.Tag?.ToString() ?? string.Empty
                        });
                    }
                }
            }

            if (LbProgram != null)
                LbProgram.ItemsSource = _selectedExercises;
        }

        // ============ ВКЛАДКА «ПРОГРАММА» ============

        private void TabProgram_GotFocus(object sender, RoutedEventArgs e)
        {
            if (LbProgram != null)
                LbProgram.ItemsSource = _selectedExercises;
        }

        private void BtnRemoveExercise_Click(object sender, RoutedEventArgs e)
        {
            if (LbProgram.SelectedItem is Exercise ex)
            {
                _selectedExercises.Remove(ex);
                LbProgram.ItemsSource = _selectedExercises;
            }
        }

        private void BtnSaveProgram_Click(object sender, RoutedEventArgs e)
        {
            var json = JsonSerializer.Serialize(_selectedExercises);
            File.WriteAllText("program.json", json);
            MessageBox.Show("Программа сохранена в program.json");
        }

        private void BtnLoadProgram_Click(object sender, RoutedEventArgs e)
        {
            if (!File.Exists("program.json"))
            {
                MessageBox.Show("Файл program.json не найден");
                return;
            }

            var json = File.ReadAllText("program.json");
            // Добавили защиту от null
            var loaded = JsonSerializer.Deserialize<ObservableCollection<Exercise>>(json) ?? new ObservableCollection<Exercise>();

            _selectedExercises.Clear();
            foreach (var ex in loaded)
                _selectedExercises.Add(ex);

            LbProgram.ItemsSource = _selectedExercises;
        }
        // 1. Метод для перетаскивания окна за верхнюю панель
        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ButtonState == System.Windows.Input.MouseButtonState.Pressed)
                this.DragMove();
        }

        // 2. Метод для закрытия окна по кастомному крестику "✕"
        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}