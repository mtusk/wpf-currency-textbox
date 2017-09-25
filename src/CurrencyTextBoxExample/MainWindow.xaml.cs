using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;

namespace CurrencyTextBoxExample
{
    public partial class MainWindow : IDataErrorInfo
    {
        public decimal Number { get; set; } = 1.23M;

        private List<string> _stringFormats;
        public List<string> StringFormats
        {
            get => _stringFormats ?? (_stringFormats = new List<string>
            {
                "C0",
                "C",
                "C1",
                "C2",
                "C3",
                "C4",
                "C5",
                "C6",
                "N0",
                "N",
                "N1",
                "N2",
                "N3",
                "N4",
                "N5",
                "N6",
                "P0",
                "P",
                "P1",
                "P2",
                "P3",
                "P4",
                "P5",
                "P6"
            });

            set => _stringFormats = value;
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        public string Error => throw new System.NotImplementedException();

        public string this[string columnName] => columnName == "Number" && (Number < 0 || Number > 10)
                                                ? "Number must be between zero and ten."
                                                : null;

        private void Button_Click(object sender, RoutedEventArgs e) => MessageBox.Show("Default Button activated!");
    }
}
