using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

/// <summary>
/// Fork from mtusk CurrencyTextBox
/// https://github.com/mtusk/wpf-currency-textbox
/// 
/// Fork 2016 by Derek Tremblay (Abbaye) 
/// derektremblay666@gmail.com
/// </summary>
namespace CurrencyTextBoxControl
{
    public class CurrencyTextBox : TextBox
    {
        #region Global variables / Event

        private List<decimal> _undoList = new List<decimal>();
        private List<decimal> _redoList = new List<decimal>();
        private bool _isUndoEnabled = true;
        private Popup _popup = null;
        private Label _PopupLabel = null;
        private decimal _numberBeforePopup;

        //private CurrencyTextBox _popupCtb = null;

        public event EventHandler NumberChanged;
        #endregion Global variables

        #region Dependency Properties
        public static readonly DependencyProperty NumberProperty = DependencyProperty.Register(
            "Number",
            typeof(decimal),
            typeof(CurrencyTextBox),
            new FrameworkPropertyMetadata(0M, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                new PropertyChangedCallback(NumberPropertyChanged),
                new CoerceValueCallback(NumberPropertyCoerceValue)),
            new ValidateValueCallback(NumberPropertyValidated));

        private static bool NumberPropertyValidated(object value)
        {
            return value is decimal;
        }

        private static object NumberPropertyCoerceValue(DependencyObject d, object baseValue)
        {
            CurrencyTextBox ctb = d as CurrencyTextBox;
            decimal value = (decimal)baseValue;

            //Check maximum value
            if (value > ctb.MaximumValue && ctb.MaximumValue > 0)
                return ctb.MaximumValue;
            else
            //Check minimum value
            if (value < ctb.MinimumValue && ctb.MinimumValue < 0)
                return ctb.MinimumValue;
            else
                return value;
        }

        private static void NumberPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CurrencyTextBox ctb = d as CurrencyTextBox;

            //Color update
            if (ctb.Number < 0)
                ctb.Foreground = Brushes.Red;
            else
                ctb.Foreground = Brushes.Black;

            //Launch event
            if (ctb.NumberChanged != null)
                ctb.NumberChanged(ctb, new EventArgs());
        }

        public decimal Number
        {
            get { return (decimal)GetValue(NumberProperty); }
            set { SetValue(NumberProperty, value); }
        }
        
        public bool IsCalculPanelMode
        {
            get { return (bool)GetValue(IsCalculPanelModeProperty); }
            set { SetValue(IsCalculPanelModeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsCalculPanelMode.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsCalculPanelModeProperty =
            DependencyProperty.Register("IsCalculPanelMode", typeof(bool), typeof(CurrencyTextBox), new PropertyMetadata(false));
        
        public bool CanShowAddPanel
        {
            get { return (bool)GetValue(CanShowAddPanelProperty); }
            set { SetValue(CanShowAddPanelProperty, value); }
        }
        
        /// <summary>
        /// Set for enabling the calcul panel
        /// </summary>
        public static readonly DependencyProperty CanShowAddPanelProperty =
            DependencyProperty.Register("CanShowAddPanel", typeof(bool), typeof(CurrencyTextBox), new PropertyMetadata(false));

        public static readonly DependencyProperty MaximumValueProperty =
            DependencyProperty.Register(
                "MaximumValue",
                typeof(decimal),
                typeof(CurrencyTextBox),
                new FrameworkPropertyMetadata(0M, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    new PropertyChangedCallback(MaximumValuePropertyChanged),
                    new CoerceValueCallback(MaximumCoerceValue)), new ValidateValueCallback(MaximumValidateValue));

        private static bool MaximumValidateValue(object value)
        {
            return (decimal)value <= decimal.MaxValue / 2; //&& (decimal)value >= 0;
        }

        private static object MaximumCoerceValue(DependencyObject d, object baseValue)
        {
            CurrencyTextBox ctb = d as CurrencyTextBox;

            if (ctb.MaximumValue > decimal.MaxValue / 2)
                return decimal.MaxValue / 2;

            return baseValue;
        }

        private static void MaximumValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CurrencyTextBox ctb = d as CurrencyTextBox;

            if (ctb.Number > (decimal)e.NewValue)
                ctb.Number = (decimal)e.NewValue;
        }

        public decimal MaximumValue
        {
            get { return (decimal)GetValue(MaximumValueProperty); }
            set { SetValue(MaximumValueProperty, value); }
        }

        public decimal MinimumValue
        {
            get { return (decimal)GetValue(MinimumValueProperty); }
            set { SetValue(MinimumValueProperty, value); }
        }

        public static readonly DependencyProperty MinimumValueProperty =
            DependencyProperty.Register("MinimumValue",
                typeof(decimal),
                typeof(CurrencyTextBox),
                new FrameworkPropertyMetadata(0M, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    new PropertyChangedCallback(MinimumValuePropertyChanged),
                    new CoerceValueCallback(MinimumCoerceValue)),
                new ValidateValueCallback(MinimumValidateValue));

        private static bool MinimumValidateValue(object value)
        {
            return (decimal)value >= decimal.MinValue / 2; //&& (decimal)value <= 0;
        }

        private static object MinimumCoerceValue(DependencyObject d, object baseValue)
        {
            CurrencyTextBox ctb = d as CurrencyTextBox;

            if (ctb.MinimumValue < decimal.MinValue / 2)
                return decimal.MinValue / 2;

            return baseValue;
        }

        private static void MinimumValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CurrencyTextBox ctb = d as CurrencyTextBox;

            if (ctb.Number < (decimal)e.NewValue)
                ctb.Number = (decimal)e.NewValue;
        }

        public static readonly DependencyProperty StringFormatProperty = DependencyProperty.Register(
            "StringFormat",
            typeof(string),
            typeof(CurrencyTextBox),
            new FrameworkPropertyMetadata("C2", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, StringFormatPropertyChanged, new CoerceValueCallback(StringFormatCoerceValue)),
            new ValidateValueCallback(StringFormatValidateValue));
        
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
                val == "N0" || val == "N" || val == "N1" || val == "N2" || val == "N3" || val == "N4" || val == "N5" || val == "N6";
        }

        public string StringFormat
        {
            get { return (string)GetValue(StringFormatProperty); }
            set { SetValue(StringFormatProperty, value); }
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
        #endregion Dependency Properties

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
            textBinding.StringFormat = StringFormat;

            BindingOperations.SetBinding(this, TextBox.TextProperty, textBinding);

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

        #region Events
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var tb = sender as TextBox;

            if (Number < 0 && tb.Text.EndsWith(")"))
                tb.CaretIndex = tb.Text.Length - 1;
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
            if (IsNumericKey(e.Key))
            {
                e.Handled = true;

                AddUndoInList(Number);
                InsertKey(e.Key);
            }
            else if (IsBackspaceKey(e.Key))
            {
                e.Handled = true;

                AddUndoInList(Number);
                RemoveRightMostDigit();
            }
            else if (IsUpKey(e.Key))
            {
                e.Handled = true;

                AddUndoInList(Number);
                AddOneDigit();

                //if the key is repeated add more digit
                if (e.IsRepeat)
                {
                    AddUndoInList(Number);
                    AddOneDigit(10);
                }
            }
            else if (IsDownKey(e.Key))
            {
                e.Handled = true;

                AddUndoInList(Number);
                SubstractOneDigit();

                //if the key is repeated substract more digit
                if (e.IsRepeat)
                {
                    AddUndoInList(Number);
                    SubstractOneDigit(10);
                }
            }
            else if (IsCtrlZKey(e.Key))
            {
                e.Handled = true;

                Undo();
            }
            else if (IsCtrlYKey(e.Key))
            {
                e.Handled = true;

                Redo();
            }
            else if (IsEnterKey(e.Key))
            {
                e.Handled = true;

                if (!IsCalculPanelMode)
                {
                    AddUndoInList(Number);
                    ShowAddPanel();
                }
                else
                    ((Popup)((Grid)Parent).Parent).IsOpen = false;                
            }
            else if (IsDeleteKey(e.Key))
            {
                e.Handled = true;

                AddUndoInList(Number);
                Clear();
            }
            else if (IsSubstractKey(e.Key))
            {
                e.Handled = true;

                AddUndoInList(Number);
                InvertValue();
            }
            else if (IsIgnoredKey(e.Key))
            {
                e.Handled = false;
            }
            else if (IsCtrlCKey(e.Key))
            {
                e.Handled = true;

                CopyToClipBoard();
            }
            else if (IsCtrlVKey(e.Key))
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

        private void CopyPasteEventHandler(object sender, DataObjectEventArgs e)
        {
            // cancel copy and paste
            e.CancelCommand();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// TEST METHOD FOR BUILD END SHOW POPUP
        /// </summary>
        private void ShowAddPanel()
        {
            if (CanShowAddPanel)
            {                
                //Initialize somes Child object
                Grid grid = new Grid();
                _PopupLabel = new Label();
                CurrencyTextBox ctb = new CurrencyTextBox();
                _popup = new Popup();

                //ColumnDefinition
                ColumnDefinition c1 = new ColumnDefinition();
                c1.Width = new GridLength(20, GridUnitType.Auto);
                ColumnDefinition c2 = new ColumnDefinition();
                c2.Width = new GridLength(80, GridUnitType.Star);
                grid.ColumnDefinitions.Add(c1);
                grid.ColumnDefinitions.Add(c2);
                Grid.SetColumn(_PopupLabel, 0);
                Grid.SetColumn(ctb, 1);

                //Set object properties                                         
                ctb.CanShowAddPanel = false;
                ctb.IsCalculPanelMode = true;
                ctb.StringFormat = this.StringFormat;
                ctb.NumberChanged += Ctb_NumberChanged;
                ctb.Background = Brushes.WhiteSmoke;
                grid.Background = Brushes.White;

                _popup.Width = this.ActualWidth;
                _popup.Height = 32;
                _popup.PopupAnimation = PopupAnimation.Fade;
                _popup.Placement = PlacementMode.Bottom;
                _popup.PlacementTarget = this;
                _popup.StaysOpen = false;
                _popup.Closed += _popup_Closed;

                _numberBeforePopup = Number;
                _PopupLabel.Content = "ADD:";
                
                //Add object 
                grid.Children.Add(_PopupLabel);
                grid.Children.Add(ctb);
                _popup.Child = grid;
                
                //Open popup    
                _popup.IsOpen = true;

                ctb.Focus();
            }
        }

        private void _popup_Closed(object sender, EventArgs e)
        {
            this.Focus();
        }
       
        private void Ctb_NumberChanged(object sender, EventArgs e)
        {
            CurrencyTextBox ctb = sender as CurrencyTextBox;

            Number = _numberBeforePopup + ctb.Number;

            if (ctb.Number >= 0)
                _PopupLabel.Content = "ADD:";
            else
                _PopupLabel.Content = "REMOVE:";
        }

        /// <summary>
        /// Insert number from key
        /// </summary>
        private void InsertKey(Key key)
        {
            //Max length fix
            if (MaxLength != 0 && Number.ToString().Length > MaxLength)
                return;

            try
            {
                if (IsNumericKey(key))
                    // Push the new number from the right
                    if (Number < 0)
                        Number = (Number * 10M) - (GetDigitFromKey(key) / GetDivider());
                    else
                        Number = (Number * 10M) + (GetDigitFromKey(key) / GetDivider());
            }
            catch (OverflowException ex)
            {
                if (Number < 0)
                    Number = (decimal.MinValue);
                else
                    Number = (decimal.MaxValue);
            }

        }

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

        /// <summary>
        /// Get a decimal for adjust digit when a key was inserted
        /// </summary>
        /// <returns></returns>
        private decimal GetDivider()
        {
            switch (GetBindingExpression(TextBox.TextProperty).ParentBinding.StringFormat)
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
            }

            return 1M;
        }

        /// <summary>
        /// Get substract number for adjust comma in RemoveRightMostDigit
        /// </summary>
        private int GetSubstract()
        {
            switch (GetBindingExpression(TextBox.TextProperty).ParentBinding.StringFormat)
            {
                case "N0":
                case "C0": return 1;
                case "N":
                case "C": return 3;
                case "N1":
                case "C1": return 2;
                case "N2":
                case "C2": return 3;
                case "N3":
                case "C3": return 4;
                case "N4":
                case "C4": return 5;
                case "N5":
                case "C5": return 6;
                case "N6":
                case "C6": return 7;
            }

            return 1;
        }

        /// <summary>
        /// Check if is a numeric key as pressed
        /// </summary>
        private bool IsNumericKey(Key key)
        {
            return key == Key.D0 || key == Key.D1 || key == Key.D2 || key == Key.D3 || key == Key.D4 || key == Key.D5 || key == Key.D6 || key == Key.D7 || key == Key.D8 || key == Key.D9 ||
                key == Key.NumPad0 || key == Key.NumPad1 || key == Key.NumPad2 || key == Key.NumPad3 || key == Key.NumPad4 || key == Key.NumPad5 || key == Key.NumPad6 || key == Key.NumPad7 || key == Key.NumPad8 || key == Key.NumPad9;
        }

        private bool IsBackspaceKey(Key key) { return key == Key.Back; }

        private bool IsSubstractKey(Key key) { return key == Key.Subtract || key == Key.OemMinus; }

        private bool IsDeleteKey(Key key) { return key == Key.Delete; }

        private bool IsIgnoredKey(Key key) { return key == Key.Tab; } //|| key == Key.Enter; }

        private bool IsUpKey(Key key) { return key == Key.Up; }

        private bool IsDownKey(Key key) { return key == Key.Down; }

        private bool IsEnterKey(Key key) { return key == Key.Enter; }

        private static bool IsCtrlCKey(Key key) { return key == Key.C && Keyboard.Modifiers == ModifierKeys.Control; }

        private static bool IsCtrlZKey(Key key) { return key == Key.Z && Keyboard.Modifiers == ModifierKeys.Control; }

        private static bool IsCtrlYKey(Key key) { return key == Key.Y && Keyboard.Modifiers == ModifierKeys.Control; }

        private static bool IsCtrlVKey(Key key) { return key == Key.V && Keyboard.Modifiers == ModifierKeys.Control; }

        /// <summary>
        /// Delete the right digit of number property
        /// </summary>
        /// <param name="tb"></param>
        private void RemoveRightMostDigit()
        {
            try
            {
                //Fix the number then dont have a comma
                if (Number.ToString().LastIndexOf(",") == -1)
                    Number = Convert.ToDecimal(Number.ToString() + GetNumberAdjuster());

                //Remove the right most digit after is fixed
                string numberstring = Number.ToString().Replace(",", "");
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
        /// </summary>
        /// <returns></returns>
        private string GetNumberAdjuster()
        {
            switch (GetBindingExpression(TextBox.TextProperty).ParentBinding.StringFormat)
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
        /// <param name="number"></param>
        private void AddUndoInList(decimal number, bool clearRedo = true)
        {
            _undoList.Add(number);

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
        public new bool IsUndoEnabled
        {
            get { return _isUndoEnabled; }
            set { this._isUndoEnabled = value; }
        }

        /// <summary>
        /// Clear the undo list
        /// </summary>
        public void ClearUndoList()
        {
            _undoList.Clear();
        }

        /// <summary>
        /// Check if the control can undone to a previous value
        /// </summary>
        /// <returns></returns>
        public new bool CanUndo()
        {

            if (IsUndoEnabled)
                return _undoList.Count > 0;
            else
                return false;
        }

        /// <summary>
        /// Not implemented actually
        /// </summary>
        public new void LockCurrentUndoUnit()
        {
            //this.AppendText
            throw new NotImplementedException();
        }
        #endregion Undo/Redo

        #region Public Methods

        /// <summary>
        /// Reset the number to zero.
        /// </summary>
        public new void Clear() { Number = 0M; }

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
        public void InvertValue() { Number *= -1; }

        /// <summary>
        /// Add one digit to the property number
        /// </summary>
        /// <param name="repeat">Repeat add</param>
        public void AddOneDigit(int repeat = 1)
        {
            for (int i = 0; i < repeat; i++)
                switch (GetBindingExpression(TextBox.TextProperty).ParentBinding.StringFormat)
                {
                    case "N0":
                    case "C0":
                        Number = decimal.Add(Number, 1M);
                        break;
                    case "N":
                    case "C":
                        Number = decimal.Add(Number, 0.01M);
                        break;
                    case "N1":
                    case "C1":
                        Number = decimal.Add(Number, 0.1M);
                        break;
                    case "N2":
                    case "C2":
                        Number = decimal.Add(Number, 0.01M);
                        break;
                    case "N3":
                    case "C3":
                        Number = decimal.Add(Number, 0.001M);
                        break;
                    case "N4":
                    case "C4":
                        Number = decimal.Add(Number, 0.0001M);
                        break;
                    case "N5":
                    case "C5":
                        Number = decimal.Add(Number, 0.00001M);
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
            for (int i = 0; i < repeat; i++)
                switch (GetBindingExpression(TextBox.TextProperty).ParentBinding.StringFormat)
                {
                    case "N0":
                    case "C0":
                        Number = decimal.Subtract(Number, 1M);
                        break;
                    case "N":
                    case "C":
                        Number = decimal.Subtract(Number, 0.01M);
                        break;
                    case "N1":
                    case "C1":
                        Number = decimal.Subtract(Number, 0.1M);
                        break;
                    case "N2":
                    case "C2":
                        Number = decimal.Subtract(Number, 0.01M);
                        break;
                    case "N3":
                    case "C3":
                        Number = decimal.Subtract(Number, 0.001M);
                        break;
                    case "N4":
                    case "C4":
                        Number = decimal.Subtract(Number, 0.0001M);
                        break;
                    case "N5":
                    case "C5":
                        Number = decimal.Subtract(Number, 0.00001M);
                        break;
                    case "N6":
                    case "C6":
                        Number = decimal.Subtract(Number, 0.000001M);
                        break;
                }
        }


        /// <summary>
        /// Not implemented actually
        /// </summary>
        public void AppendText()
        {
            throw new NotImplementedException();
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
                Number = decimal.Parse(Clipboard.GetText());
            }
            catch { }
        }

        /// <summary>
        /// Copy the property Number to Control
        /// </summary>
        private void CopyToClipBoard()
        {
            Clipboard.Clear();
            Clipboard.SetText(Number.ToString());
        }
        #endregion Clipboard
    }
}