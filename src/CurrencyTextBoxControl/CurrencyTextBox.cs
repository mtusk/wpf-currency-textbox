using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace CurrencyTextBoxControl
{
    public class CurrencyTextBox : TextBox
    {
        #region Dependency Properties
        public static readonly DependencyProperty NumberProperty = DependencyProperty.Register(
            "Number",
            typeof(decimal),
            typeof(CurrencyTextBox),
            new FrameworkPropertyMetadata(0M, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public decimal Number
        {
            get
            {
                return (decimal)GetValue(NumberProperty);
            }
            set
            {
                SetValue(NumberProperty, value);
            }
        }

        public static readonly DependencyProperty StringFormatProperty = DependencyProperty.Register(
            "StringFormat",
            typeof(string),
            typeof(CurrencyTextBox),
            new FrameworkPropertyMetadata("C", StringFormatPropertyChanged));
        public string StringFormat
        {
            get
            {
                return (string)GetValue(StringFormatProperty);
            }
            set
            {
                SetValue(StringFormatProperty, value);
            }
        }

        private static void StringFormatPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            // Update the Text binding with the new StringFormat
            var textBinding = new Binding();
            textBinding.Path = new PropertyPath("Number");
            textBinding.RelativeSource = new RelativeSource(RelativeSourceMode.Self);
            textBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            textBinding.StringFormat = (string)e.NewValue;

            BindingOperations.SetBinding(obj, TextBox.TextProperty, textBinding);
        }
        #endregion

        #region Constructor
        static CurrencyTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(CurrencyTextBox),
                new FrameworkPropertyMetadata(typeof(CurrencyTextBox)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            // Bind Text to Number with the specified StringFormat
            var textBinding = new Binding();
            textBinding.Path = new PropertyPath("Number");
            textBinding.RelativeSource = new RelativeSource(RelativeSourceMode.Self);
            textBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            textBinding.StringFormat = this.StringFormat;

            BindingOperations.SetBinding(this, TextBox.TextProperty, textBinding);

            // Disable copy/paste
            DataObject.AddCopyingHandler(this, PastingEventHandler);
            DataObject.AddPastingHandler(this, PastingEventHandler);

            this.CaretIndex = this.Text.Length;
            this.PreviewKeyDown += TextBox_PreviewKeyDown;
            this.PreviewMouseDown += TextBox_PreviewMouseDown;
            this.PreviewMouseUp += TextBox_PreviewMouseUp;
            this.TextChanged += TextBox_TextChanged;
            this.ContextMenu = null;
        }
        #endregion

        #region Events
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var tb = sender as TextBox;

            if (Number < 0 && tb.GetBindingExpression(TextBox.TextProperty).ParentBinding.StringFormat == "C")
            {
                // If a negative number and a StringFormat of "C" is used, then
                // place the caret before the closing paren.
                tb.CaretIndex = tb.Text.Length - 1;
            }
            else
            {
                // Keep the caret at the end
                tb.CaretIndex = tb.Text.Length;
            }
        }

        private void TextBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            // Prevent changing the caret index
            e.Handled = true;
            (sender as TextBox).Focus();
        }

        void TextBox_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            // Prevent changing the caret index
            e.Handled = true;
            (sender as TextBox).Focus();
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (IsNumericKey(e.Key))
            {
                e.Handled = true;

                // Push the new number from the right
                if (Number < 0)
                {
                    Number = (Number * 10M) - (GetDigitFromKey(e.Key) / 100M);
                }
                else
                {
                    Number = (Number * 10M) + (GetDigitFromKey(e.Key) / 100M);
                }
            }
            else if (e.Key == Key.Back)
            {
                e.Handled = true;

                // Remove the right-most digit
                Number = (Number - (Number % 0.1M)) / 10M;
            }
            else if (e.Key == Key.Delete)
            {
                e.Handled = true;

                Number = 0M;
            }
            else if (e.Key == Key.Subtract || e.Key == Key.OemMinus)
            {
                e.Handled = true;

                Number *= -1;
            }
            else if (IsIgnoredKey(e.Key))
            {
                e.Handled = false;
            }
            else
            {
                e.Handled = true;
            }
        }

        private void PastingEventHandler(object sender, DataObjectEventArgs e)
        {
            // Prevent copy/paste
            e.CancelCommand();
        }
        #endregion

        #region Private Methods
        private decimal GetDigitFromKey(Key key)
        {
            switch (key)
            {
                case Key.D0:
                case Key.NumPad0: return 0M;
                case Key.D1:
                case Key.NumPad1: return 1M;
                case Key.D2:
                case Key.NumPad2: return 2M;
                case Key.D3:
                case Key.NumPad3: return 3M;
                case Key.D4:
                case Key.NumPad4: return 4M;
                case Key.D5:
                case Key.NumPad5: return 5M;
                case Key.D6:
                case Key.NumPad6: return 6M;
                case Key.D7:
                case Key.NumPad7: return 7M;
                case Key.D8:
                case Key.NumPad8: return 8M;
                case Key.D9:
                case Key.NumPad9: return 9M;
                default: throw new ArgumentOutOfRangeException("Invalid key: " + key.ToString());
            }
        }

        private bool IsNumericKey(Key key)
        {
            return key == Key.D0 ||
                key == Key.D1 ||
                key == Key.D2 ||
                key == Key.D3 ||
                key == Key.D4 ||
                key == Key.D5 ||
                key == Key.D6 ||
                key == Key.D7 ||
                key == Key.D8 ||
                key == Key.D9 ||
                key == Key.NumPad0 ||
                key == Key.NumPad1 ||
                key == Key.NumPad2 ||
                key == Key.NumPad3 ||
                key == Key.NumPad4 ||
                key == Key.NumPad5 ||
                key == Key.NumPad6 ||
                key == Key.NumPad7 ||
                key == Key.NumPad8 ||
                key == Key.NumPad9;
        }

        private bool IsBackspaceKey(Key key)
        {
            return key == Key.Back;
        }

        private bool IsIgnoredKey(Key key)
        {
            return key == Key.Up ||
                key == Key.Down ||
                key == Key.Tab ||
                key == Key.Enter;
        }
        #endregion
    }
}
