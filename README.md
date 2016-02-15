# WPF Currency/Numeric TextBox
A WPF TextBox for entering a Currency/Numeric value, similar to how a cash register works.

![example](example.gif?raw=true)

##NUGET
https://www.nuget.org/packages/CurrencyTextBox/

## Features
- Numbers typed are pushed in from the right. If we start with the default value 0.00, and start typing the numbers 123, the value updates as such: 0.00 => 0.01 => 0.12 => 1.23
- You can set Maximum and Minimum value.
- If we press the backspace key, the numbers are shifted right: 1.23 => 0.12 => 0.01 => 0.00
- If we press the delete key, the value is reset to 0.00.
- If we press the minus key, the value becomes negative.
- If we press down/up key, the value are increase/decrease slowly (keydown repeat for change value faster)
- Support stringformat C to C6.
- Support stringformat N to N6.
- Copy and paste decimal value. ctrl+c / ctrl+v
- Undo/Redo value with default ctrl+z / ctrl+y
- This control's template can be customized to change the appearance.
- Supports data validation.

##Release Notes (NUGET Package version)
New in version 1.3.0 
- Add support of Undolimit
- Add functionality ADD/REMOVE popup (Enter Key when CanShowAddPanel = true) 

New in version 1.2.0 
- Add support of numeric value (Stringformat N to N6) 

New in version 1.1.1 
- Add Up/Down key for change value slowly (keydown repeat for change value faster)
- Validate MinimumValue / MaximumValue dependency properties 
- Validate StringFormat dependency property 

New in version 1.1.0 
- Add support of Undo/Redo 
- Add Copy/Paste

## How to use
Add a reference to `CurrencyTextBoxControl.dll` from your project, then add the following namespace to your XAML:

```xaml
xmlns:currency="clr-namespace:CurrencyTextBoxControl;assembly=CurrencyTextBoxControl"
```

Insert the control like this:

```xaml
<currency:CurrencyTextBox Number="{Binding Number}" />

<currency:CurrencyTextBox Number="{Binding Number}" MaximumValue="{Binding MaximumFromDB}" MinimumValue="{Binding MininumFromDB}"/>
```
