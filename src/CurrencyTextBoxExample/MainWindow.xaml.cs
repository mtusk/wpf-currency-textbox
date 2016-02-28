using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;

namespace CurrencyTextBoxExample
{
    public partial class MainWindow : Window, IDataErrorInfo
    {
        private decimal _number = 1.23M;
        public decimal Number
        {
            get
            {
                return _number;
            }
            set
            {
                _number = value;
            }
        }

        private List<string> _stringFormats;
        public List<string> StringFormats
        {
            get
            {
                if (_stringFormats == null)
                {
                    _stringFormats = new List<string>() { "C0", "C", "C1", "C2", "C3", "C4", "C5", "C6", "N0", "N", "N1", "N2", "N3", "N4", "N5", "N6", "P0", "P", "P1", "P2", "P3", "P4", "P5", "P6"};
                }

                return _stringFormats;
            }
            set
            {
                _stringFormats = value;
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            
            this.DataContext = this;
        }

        public string Error
        {
            get { throw new System.NotImplementedException(); }
        }

        public string this[string columnName]
        {
            get
            {
                if (columnName == "Number" &&
                    (_number < 0 || _number > 10))
                {
                    return "Number must be between zero and ten.";
                }
                else
                {
                    return null;
                }
            }
        }
    }
}
