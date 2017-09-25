// Fork from mtusk CurrencyTextBox
// https://github.com/mtusk/wpf-currency-textbox
// 
// Fork 2016-2017 by Derek Tremblay (Abbaye) 
// derektremblay666@gmail.com

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace CurrencyTextBoxControl
{
    public class CurrencyTextBox : TextBox
    {
        #region Global variables / Event

        private readonly List<decimal> _undoList = new List<decimal>();
        private readonly List<decimal> _redoList = new List<decimal>();
        private Popup _popup;
        private Label _popupLabel;
        private decimal _numberBeforePopup;
       
        //Event
        public event EventHandler PopupClosed;
        public event EventHandler NumberChanged;
        #endregion Global variables

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
            var textBinding = new Binding
            {
                Path = new PropertyPath("Number"),
                RelativeSource = new RelativeSource(RelativeSourceMode.Self),
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                StringFormat = StringFormat
            };

            BindingOperations.SetBinding(this, TextProperty, textBinding);

            // Disable copy/paste
            DataObject.AddCopyingHandler(this, CopyPasteEventHandler);
            DataObject.AddPastingHandler(this, CopyPasteEventHandler);

            //Events
            CaretIndex = Text.Length;
            PreviewKeyDown += TextBox_PreviewKeyDown;
            PreviewMouseDown += TextBox_PreviewMouseDown;
            PreviewMouseUp += TextBox_PreviewMouseUp;
            TextChanged += TextBox_TextChanged;

            //Disable contextmenu
            ContextMenu = null;

        }
        #endregion Constructor

        #region Dependency Properties

        public static readonly DependencyProperty NumberProperty = DependencyProperty.Register(
            nameof(Number), typeof(decimal), typeof(CurrencyTextBox),
            new FrameworkPropertyMetadata(0M, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                NumberPropertyChanged, NumberPropertyCoerceValue), NumberPropertyValidated);

        private static bool NumberPropertyValidated(object value) => value is decimal;

        private static object NumberPropertyCoerceValue(DependencyObject d, object baseValue)
        {
            if (d is CurrencyTextBox ctb)
            {
                var value = (decimal) baseValue;

                //Check maximum value
                if (value > ctb.MaximumValue && ctb.MaximumValue > 0)
                    return ctb.MaximumValue;

                if (value < ctb.MinimumValue && ctb.MinimumValue < 0)
                    return ctb.MinimumValue;

                return value;
            }

            return baseValue;
        }

        private static void NumberPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CurrencyTextBox ctb)
            {
                //Update IsNegative
                ctb.SetValue(IsNegativeProperty, ctb.Number < 0);

                //Launch event
                ctb.NumberChanged?.Invoke(ctb, new EventArgs());
            }
        }

        public decimal Number
        {
            get => (decimal)GetValue(NumberProperty);
            set => SetValue(NumberProperty, value);
        }

        public static readonly DependencyProperty IsNegativeProperty =
            DependencyProperty.Register(nameof(IsNegative), typeof(bool), typeof(CurrencyTextBox), new PropertyMetadata(false));

        public bool IsNegative => (bool) GetValue(IsNegativeProperty);

        public bool IsCalculPanelMode
        {
            get => (bool)GetValue(IsCalculPanelModeProperty);
            set => SetValue(IsCalculPanelModeProperty, value);
        }

        // Using a DependencyProperty as the backing store for IsCalculPanelMode.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsCalculPanelModeProperty =
            DependencyProperty.Register(nameof(IsCalculPanelMode), typeof(bool), typeof(CurrencyTextBox), new PropertyMetadata(false));
        
        public bool CanShowAddPanel
        {
            get => (bool)GetValue(CanShowAddPanelProperty);
            set => SetValue(CanShowAddPanelProperty, value);
        }
        
        /// <summary>
        /// Set for enabling the calcul panel
        /// </summary>
        public static readonly DependencyProperty CanShowAddPanelProperty =
            DependencyProperty.Register(nameof(CanShowAddPanel), typeof(bool), typeof(CurrencyTextBox), new PropertyMetadata(false));

        public static readonly DependencyProperty MaximumValueProperty =
            DependencyProperty.Register(nameof(MaximumValue), typeof(decimal), typeof(CurrencyTextBox),
                new FrameworkPropertyMetadata(0M, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    MaximumValuePropertyChanged, MaximumCoerceValue), MaximumValidateValue);

        private static bool MaximumValidateValue(object value) => (decimal)value <= decimal.MaxValue / 2;

        private static object MaximumCoerceValue(DependencyObject d, object baseValue)
        {
            var ctb = d as CurrencyTextBox;

            if (ctb.MaximumValue > decimal.MaxValue / 2)
                return decimal.MaxValue / 2;

            return baseValue;
        }

        private static void MaximumValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctb = d as CurrencyTextBox;

            if (ctb.Number > (decimal)e.NewValue)
                ctb.Number = (decimal)e.NewValue;
        }

        public decimal MaximumValue
        {
            get => (decimal)GetValue(MaximumValueProperty);
            set => SetValue(MaximumValueProperty, value);
        }

        public decimal MinimumValue
        {
            get => (decimal)GetValue(MinimumValueProperty);
            set => SetValue(MinimumValueProperty, value);
        }

        public static readonly DependencyProperty MinimumValueProperty =
            DependencyProperty.Register(nameof(MinimumValue), typeof(decimal), typeof(CurrencyTextBox),
                new FrameworkPropertyMetadata(0M, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    MinimumValuePropertyChanged, MinimumCoerceValue), MinimumValidateValue);

        private static bool MinimumValidateValue(object value)
        {
            return (decimal)value >= decimal.MinValue / 2; //&& (decimal)value <= 0;
        }

        private static object MinimumCoerceValue(DependencyObject d, object baseValue)
        {
            var ctb = d as CurrencyTextBox;

            if (ctb.MinimumValue < decimal.MinValue / 2)
                return decimal.MinValue / 2;

            return baseValue;
        }

        private static void MinimumValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctb = d as CurrencyTextBox;

            if (ctb.Number < (decimal)e.NewValue)
                ctb.Number = (decimal)e.NewValue;
        }

        public static readonly DependencyProperty StringFormatProperty = DependencyProperty.Register(
            nameof(StringFormat), typeof(string), typeof(CurrencyTextBox),
            new FrameworkPropertyMetadata("C2", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                StringFormatPropertyChanged, StringFormatCoerceValue), StringFormatValidateValue);
        
        private static object StringFormatCoerceValue(DependencyObject d, object baseValue)
        {
            return ((string)baseValue).ToUpper();
        }

        /// <summary>
        /// Validate the StringFormat
        /// </summary>
        private static bool StringFormatValidateValue(object value)
        {
            var val = value.ToString().ToUpper();

            return val == "C0" || val == "C" || val == "C1" || val == "C2" || val == "C3" || val == "C4" || val == "C5" || val == "C6" ||
                val == "N0" || val == "N" || val == "N1" || val == "N2" || val == "N3" || val == "N4" || val == "N5" || val == "N6" ||
                val == "P0" || val == "P" || val == "P1" || val == "P2" || val == "P3" || val == "P4" || val == "P5" || val == "P6";
        }

        public string StringFormat
        {
            get => (string)GetValue(StringFormatProperty);
            set => SetValue(StringFormatProperty, value);
        }

        private static void StringFormatPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            // Update the Text binding with the new StringFormat
            var textBinding = new Binding
            {
                Path = new PropertyPath("Number"),
                RelativeSource = new RelativeSource(RelativeSourceMode.Self),
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                StringFormat = (string)e.NewValue
            };

            BindingOperations.SetBinding(obj, TextProperty, textBinding);
        }
        
        public int UpDownRepeat
        {
            get => (int)GetValue(UpDownRepeatProperty);
            set => SetValue(UpDownRepeatProperty, value);
        }

        /// <summary>
        /// Set the Up/down value when key repeated
        /// </summary>
        public static readonly DependencyProperty UpDownRepeatProperty =
            DependencyProperty.Register(nameof(UpDownRepeat), typeof(int), typeof(CurrencyTextBox), new PropertyMetadata(10));
        
        #endregion Dependency Properties
        
        #region Events
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var tb = sender as TextBox;

            if (Number < 0 && tb.Text.EndsWith(")"))
                tb.CaretIndex = tb.Text.Length - 1;
            else if (tb.Text.EndsWith("%"))
                tb.CaretIndex = tb.Text.Length - 2;
            else
                tb.CaretIndex = tb.Text.Length;
        }

        private void TextBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            // Prevent changing the caret index
            e.Handled = true;
            (sender as TextBox).Focus();
        }

        private void TextBox_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            // Prevent changing the caret index
            e.Handled = true;
            (sender as TextBox).Focus();
        }

        /// <summary>
        /// Action when is key pressed
        /// </summary>
        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (IsReadOnly)
            {
                e.Handled = true;
                return;
            }

            if (KeyValidator.IsNumericKey(e.Key))
            {
                e.Handled = true;

                AddUndoInList(Number);
                InsertKey(e.Key);
            }
            else if (KeyValidator.IsBackspaceKey(e.Key))
            {
                e.Handled = true;

                AddUndoInList(Number);
                RemoveRightMostDigit();
            }
            else if (KeyValidator.IsUpKey(e.Key))
            {
                e.Handled = true;

                AddUndoInList(Number);
                AddOneDigit();

                //if the key is repeated add more digit
                if (e.IsRepeat)
                {
                    AddUndoInList(Number);
                    AddOneDigit(UpDownRepeat);
                }
            }
            else if (KeyValidator.IsDownKey(e.Key))
            {
                e.Handled = true;

                AddUndoInList(Number);
                SubstractOneDigit();

                //if the key is repeated substract more digit
                if (e.IsRepeat)
                {
                    AddUndoInList(Number);
                    SubstractOneDigit(UpDownRepeat);
                }
            }
            else if (KeyValidator.IsCtrlZKey(e.Key))
            {
                e.Handled = true;

                Undo();
            }
            else if (KeyValidator.IsCtrlYKey(e.Key))
            {
                e.Handled = true;

                Redo();
            }
            else if (KeyValidator.IsEnterKey(e.Key))
            {
                if (!IsCalculPanelMode)
                {
                    AddUndoInList(Number);
                    ShowAddPopup();
                }
                else
                {
                    ((Popup)((Grid)Parent).Parent).IsOpen = false;

                    if (PopupClosed != null)
                    {
                        e.Handled = true;
                        PopupClosed(this, new EventArgs());
                    }
                }
            }
            else if (KeyValidator.IsDeleteKey(e.Key))
            {
                e.Handled = true;

                AddUndoInList(Number);
                Clear();
            }
            else if (KeyValidator.IsSubstractKey(e.Key))
            {
                e.Handled = true;

                AddUndoInList(Number);
                InvertValue();
            }
            else if (KeyValidator.IsIgnoredKey(e.Key))
            {
                e.Handled = false;
            }
            else if (KeyValidator.IsCtrlCKey(e.Key))
            {
                e.Handled = true;

                CopyToClipBoard();
            }
            else if (KeyValidator.IsCtrlVKey(e.Key))
            {
                e.Handled = true;

                AddUndoInList(Number);
                PasteFromClipBoard();
            }
            else
            {
                e.Handled = true;
            }
        }

        // cancel copy and paste
        private void CopyPasteEventHandler(object sender, DataObjectEventArgs e) => e.CancelCommand();

        #endregion

        #region Private Methods       
        private void Ctb_NumberChanged(object sender, EventArgs e)
        {
            var ctb = sender as CurrencyTextBox;

            Number = _numberBeforePopup + ctb.Number;

            _popupLabel.Content = ctb.Number >= 0 ? "+" : "-";
        }

        /// <summary>
        /// Insert number from key
        /// </summary>
        private void InsertKey(Key key)
        {
            //Max length fix
            if (MaxLength != 0 && Number.ToString(CultureInfo.CurrentCulture).Length > MaxLength)
                return;

            try
            {
                // Push the new number from the right
                if (KeyValidator.IsNumericKey(key))
                    Number = Number < 0
                        ? Number * 10M - GetDigitFromKey(key) / GetDivider()
                        : Number * 10M + GetDigitFromKey(key) / GetDivider();
            }
            catch (OverflowException)
            {
                Number = Number < 0 ? decimal.MinValue : decimal.MaxValue;
            }

        }

        /// <summary>
        /// Get the digit from key
        /// </summary>        
        private static decimal GetDigitFromKey(Key key)
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
                default: throw new ArgumentOutOfRangeException($"Invalid key: {key}");
            }
        }

        /// <summary>
        /// Get a decimal for adjust digit when a key was inserted
        /// </summary>
        /// <returns></returns>
        private decimal GetDivider()
        {
            switch (GetBindingExpression(TextProperty).ParentBinding.StringFormat)
            {
                case "N0":
                case "C0": return 1M;
                case "N":
                case "C": return 100M;
                case "N1":
                case "C1": return 10M;
                case "N2":
                case "C2": return 100M;
                case "N3":
                case "C3": return 1000M;
                case "N4":
                case "C4": return 10000M;
                case "N5":
                case "C5": return 100000M;
                case "N6":
                case "C6": return 1000000M;
                case "P0": return 100M;
                case "P": return 10000M;
                case "P1": return 1000M;
                case "P2": return 10000M;
                case "P3": return 100000M;
                case "P4": return 1000000M;
                case "P5": return 10000000M;
                case "P6": return 100000000M;
            }

            return 1M;
        }

        /// <summary>
        /// Get substract number for adjust comma in RemoveRightMostDigit
        /// </summary>
        private int GetSubstract()
        {
            switch (GetBindingExpression(TextProperty).ParentBinding.StringFormat)
            {
                case "P0": return 3;
                case "N0":
                case "C0": return 1;
                case "P": return 5;
                case "N":
                case "C": return 3;
                case "P1": return 4;
                case "N1":
                case "C1": return 2;
                case "P2": return 5;
                case "N2":
                case "C2": return 3;
                case "P3": return 6;
                case "N3":
                case "C3": return 4;
                case "P4": return 7;
                case "N4":
                case "C4": return 5;
                case "P5": return 8;
                case "N5":
                case "C5": return 6;
                case "P6": return 9;
                case "N6":
                case "C6": return 7;
            }

            return 1;
        }

        /// <summary>
        /// Get the number of digit .
        /// </summary>
        private int GetDigitCount()
        {
            switch (GetBindingExpression(TextProperty).ParentBinding.StringFormat)
            {
                case "N0":
                case "C0": return 0;
                case "N":
                case "C": return 2;
                case "N1":
                case "C1": return 1;
                case "N2":
                case "C2": return 2;
                case "N3":
                case "C3": return 3;
                case "N4":
                case "C4": return 4;
                case "N5":
                case "C5": return 5;
                case "N6":
                case "C6": return 6;
            }

            return 1;
        }
        
        /// <summary>
        /// Delete the right digit of number property
        /// </summary>
        private void RemoveRightMostDigit()
        {
            try
            {
                //Fix the number then dont have a comma
                if (Number.ToString(CultureInfo.CurrentCulture).LastIndexOf(",", StringComparison.Ordinal) == -1)
                    Number = Convert.ToDecimal(Number + GetNumberAdjuster());

                //Remove the right most digit after is fixed
                var numberstring = Number.ToString(CultureInfo.CurrentCulture).Replace(",", "");
                numberstring = numberstring.Insert(numberstring.Length - GetSubstract(), ",");
                Number = Convert.ToDecimal(numberstring.Remove(numberstring.Length - 1));
            }
            catch
            {
                Clear();
            }
        }

        /// <summary>
        /// Get number adjuster to fix when you delete right most digit
        /// Not used with percent stringformat
        /// </summary>
        /// <returns></returns>
        private string GetNumberAdjuster()
        {
            switch (GetBindingExpression(TextProperty).ParentBinding.StringFormat)
            {
                case "N0":
                case "C0": return "";
                case "N":
                case "C": return ",00";
                case "N1":
                case "C1": return ",0";
                case "N2":
                case "C2": return ",00";
                case "N3":
                case "C3": return ",000";
                case "N4":
                case "C4": return ",0000";
                case "N5":
                case "C5": return ",00000";
                case "N6":
                case "C6": return ",000000";
            }

            return "";
        }
        #endregion Privates methodes

        #region Undo/Redo 

        /// <summary>
        /// Add undo to the list
        /// </summary>
        private void AddUndoInList(decimal number, bool clearRedo = true)
        {
            //Clear first item when undolimit is reach
            if (_undoList.Count == UndoLimit)  
                _undoList.RemoveRange(0, 1);            

            //Add item to undo list
            _undoList.Add(number);

            //Clear redo when needed
            if (clearRedo)
                _redoList.Clear();
        }

        /// <summary>
        /// Undo the to the previous value
        /// </summary>
        public new void Undo()
        {
            if (CanUndo())
            {
                Number = _undoList[_undoList.Count - 1];

                _redoList.Add(_undoList[_undoList.Count - 1]);
                _undoList.RemoveAt(_undoList.Count - 1);
            }
        }

        /// <summary>
        /// Redo to the value previously undone. The list is clear when key is handled
        /// </summary>
        public new void Redo()
        {
            if (_redoList.Count > 0)
            {
                AddUndoInList(Number, false);
                Number = _redoList[_redoList.Count - 1];
                _redoList.RemoveAt(_redoList.Count - 1);
            }
        }

        /// <summary>
        /// Get or set for indicate if control CanUndo
        /// </summary>
        public new bool IsUndoEnabled { get; set; } = true;

        /// <summary>
        /// Clear the undo list
        /// </summary>
        public void ClearUndoList() => _undoList.Clear();

        /// <summary>
        /// Check if the control can undone to a previous value
        /// </summary>
        /// <returns></returns>
        public new bool CanUndo() => IsUndoEnabled && _undoList.Count > 0;

        #endregion Undo/Redo

        #region Public Methods

        /// <summary>
        /// Reset the number to zero.
        /// </summary>
        public new void Clear() => Number = 0M;

        /// <summary>
        /// Set number to positive
        /// </summary>
        public void SetPositive() { if (Number < 0) Number *= -1; }

        /// <summary>
        /// Set number to negative
        /// </summary>
        public void SetNegative() { if (Number > 0) Number *= -1; }

        /// <summary>
        /// Alternate value to Negative-Positive and Positive-Negative
        /// </summary>
        public void InvertValue() => Number *= -1;

        /// <summary>
        /// Add one digit to the property number
        /// </summary>
        /// <param name="repeat">Repeat add</param>
        public void AddOneDigit(int repeat = 1)
        {
            for (var i = 0; i < repeat; i++)
                switch (GetBindingExpression(TextProperty).ParentBinding.StringFormat)
                {
                    case "P0":
                        Number = decimal.Add(Number, 0.01M);
                        break;
                    case "N0":
                    case "C0":
                        Number = decimal.Add(Number, 1M);
                        break;
                    case "P":
                        Number = decimal.Add(Number, 0.0001M);
                        break;
                    case "N":
                    case "C":
                        Number = decimal.Add(Number, 0.01M);
                        break;
                    case "P1":
                        Number = decimal.Add(Number, 0.001M);
                        break;
                    case "N1":
                    case "C1":
                        Number = decimal.Add(Number, 0.1M);
                        break;
                    case "P2":
                        Number = decimal.Add(Number, 0.0001M);
                        break;
                    case "N2":
                    case "C2":
                        Number = decimal.Add(Number, 0.01M);
                        break;
                    case "P3":
                        Number = decimal.Add(Number, 0.00001M);
                        break;
                    case "N3":
                    case "C3":
                        Number = decimal.Add(Number, 0.001M);
                        break;
                    case "P4":
                        Number = decimal.Add(Number, 0.000001M);
                        break;
                    case "N4":
                    case "C4":
                        Number = decimal.Add(Number, 0.0001M);
                        break;
                    case "P5":
                        Number = decimal.Add(Number, 0.0000001M);
                        break;
                    case "N5":
                    case "C5":
                        Number = decimal.Add(Number, 0.00001M);
                        break;
                    case "P6":
                        Number = decimal.Add(Number, 0.00000001M);
                        break;
                    case "N6":
                    case "C6":
                        Number = decimal.Add(Number, 0.000001M);
                        break;
                }
        }

        /// <summary>
        /// Substract one digit to the property number
        /// </summary>
        /// <param name="repeat">Repeat substract</param>
        public void SubstractOneDigit(int repeat = 1)
        {
            for (var i = 0; i < repeat; i++)
                switch (GetBindingExpression(TextProperty).ParentBinding.StringFormat)
                {
                    case "P0":
                        Number = decimal.Subtract(Number, 0.01M);
                        break;
                    case "N0":
                    case "C0":
                        Number = decimal.Subtract(Number, 1M);
                        break;
                    case "P":
                        Number = decimal.Subtract(Number, 0.0001M);
                        break;
                    case "N":
                    case "C":
                        Number = decimal.Subtract(Number, 0.01M);
                        break;
                    case "P1":
                        Number = decimal.Subtract(Number, 0.001M);
                        break;
                    case "N1":
                    case "C1":
                        Number = decimal.Subtract(Number, 0.1M);
                        break;
                    case "P2":
                        Number = decimal.Subtract(Number, 0.0001M);
                        break;
                    case "N2":
                    case "C2":
                        Number = decimal.Subtract(Number, 0.01M);
                        break;
                    case "P3":
                        Number = decimal.Subtract(Number, 0.00001M);
                        break;
                    case "N3":
                    case "C3":
                        Number = decimal.Subtract(Number, 0.001M);
                        break;
                    case "P4":
                        Number = decimal.Subtract(Number, 0.000001M);
                        break;
                    case "N4":
                    case "C4":
                        Number = decimal.Subtract(Number, 0.0001M);
                        break;
                    case "P5":
                        Number = decimal.Subtract(Number, 0.0000001M);
                        break;
                    case "N5":
                    case "C5":
                        Number = decimal.Subtract(Number, 0.00001M);
                        break;
                    case "P6":
                        Number = decimal.Subtract(Number, 0.00000001M);
                        break;
                    case "N6":
                    case "C6":
                        Number = decimal.Subtract(Number, 0.000001M);
                        break;
                }
        }

        #endregion Other function

        #region Clipboard
        /// <summary>
        /// Paste if is a number on clipboard
        /// </summary>
        private void PasteFromClipBoard()
        {
            try
            {
                switch (GetBindingExpression(TextProperty).ParentBinding.StringFormat)
                {                    
                    case "P0": 
                    case "P": 
                    case "P1": 
                    case "P2": 
                    case "P3": 
                    case "P4": 
                    case "P5": 
                    case "P6":
                        Number = decimal.Parse(Clipboard.GetText());
                        break;
                    default:
                        Number = Math.Round(decimal.Parse(Clipboard.GetText()), GetDigitCount());
                        break;
                }
                
            }
            catch
            {
                // ignored
            }
        }

        /// <summary>
        /// Copy the property Number to Control
        /// </summary>
        private void CopyToClipBoard()
        {
            Clipboard.Clear();
            Clipboard.SetText(Number.ToString(CultureInfo.CurrentCulture));
        }
        #endregion Clipboard

        #region Add/remove value Popup 
        /// <summary>
        /// Show popup for add/remove value
        /// </summary>
        private void ShowAddPopup()
        {
            if (CanShowAddPanel)
            {
                //Initialize somes Child object
                var grid = new Grid {Background = Brushes.White};

                var ctbPopup = new CurrencyTextBox
                {
                    CanShowAddPanel = false,
                    IsCalculPanelMode = true,
                    StringFormat = StringFormat,
                    Background = Brushes.WhiteSmoke
                };

                _popup = new Popup
                {
                    Width = ActualWidth,
                    Height = 32,
                    PopupAnimation = PopupAnimation.Fade,
                    Placement = PlacementMode.Bottom,
                    PlacementTarget = this,
                    StaysOpen = false,
                    Child = grid,
                    IsOpen = true
                };

                //ColumnDefinition
                var c1 = new ColumnDefinition {Width = new GridLength(20, GridUnitType.Auto)};
                var c2 = new ColumnDefinition {Width = new GridLength(80, GridUnitType.Star)};
                grid.ColumnDefinitions.Add(c1);
                grid.ColumnDefinitions.Add(c2);
                Grid.SetColumn(_popupLabel, 0);
                Grid.SetColumn(ctbPopup, 1);

                //Set object properties                                         
                ctbPopup.NumberChanged += Ctb_NumberChanged;
                ctbPopup.PopupClosed += CtbPopup_PopupClosed;

                _numberBeforePopup = Number;
                _popupLabel = new Label { Content = "+" };

                //Add object 
                grid.Children.Add(_popupLabel);
                grid.Children.Add(ctbPopup);

                ctbPopup.Focus();
            }
        }

        private void CtbPopup_PopupClosed(object sender, EventArgs e) => Focus();
        #endregion Add/remove value Popup
    }
}