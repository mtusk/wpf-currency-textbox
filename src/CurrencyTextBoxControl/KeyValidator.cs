using System.Windows.Input;

namespace CurrencyTextBoxControl
{
    public static class KeyValidator
    {
        /// <summary>
        /// Check if is a numeric key as pressed
        /// </summary>
        public static bool IsNumericKey(Key key) => key == Key.D0 || key == Key.D1 || key == Key.D2 || key == Key.D3 || key == Key.D4 || key == Key.D5 || key == Key.D6 || key == Key.D7 || key == Key.D8 || key == Key.D9 ||
                                                     key == Key.NumPad0 || key == Key.NumPad1 || key == Key.NumPad2 || key == Key.NumPad3 || key == Key.NumPad4 || key == Key.NumPad5 || key == Key.NumPad6 || key == Key.NumPad7 || key == Key.NumPad8 || key == Key.NumPad9;

        public static bool IsBackspaceKey(Key key) => key == Key.Back;

        public static bool IsSubstractKey(Key key) => key == Key.Subtract || key == Key.OemMinus;

        public static bool IsDeleteKey(Key key) => key == Key.Delete;

        public static bool IsIgnoredKey(Key key) => key == Key.Tab;

        public static bool IsUpKey(Key key) => key == Key.Up;

        public static bool IsDownKey(Key key) => key == Key.Down;

        public static bool IsEnterKey(Key key) => key == Key.Enter;

        public static bool IsCtrlCKey(Key key) => key == Key.C && Keyboard.Modifiers == ModifierKeys.Control;

        public static bool IsCtrlZKey(Key key) => key == Key.Z && Keyboard.Modifiers == ModifierKeys.Control;

        public static bool IsCtrlYKey(Key key) => key == Key.Y && Keyboard.Modifiers == ModifierKeys.Control;

        public static bool IsCtrlVKey(Key key) => key == Key.V && Keyboard.Modifiers == ModifierKeys.Control;
    }
}
