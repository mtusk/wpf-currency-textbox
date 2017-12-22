![example](Logo.png?raw=true)

[![NuGet](https://img.shields.io/badge/Nuget-1.4.3-brightgreen.svg)](https://www.nuget.org/packages/CurrencyTextBox/)
[![NetFramework](https://img.shields.io/badge/.Net%20Framework-4.5-green.svg)](https://www.microsoft.com/net/download/windows)
[![License](https://img.shields.io/badge/License-MIT-blue.svg)](https://github.com/abbaye/wpf-currency-textbox/blob/ExpendedFunctionality/LICENSE)

A WPF TextBox for entering a currency/numeric/percent value, similar to how a cash register works.

## You want to say thank or just like it ?

CurrencyTextBox control is totaly free and can be used in all project you want like open source and commercial applications. I make it in my free time and it's a fork from mtusk with a lot new of features... Please hit ⭐️ button if you like it and I will be very happy ;) I accept help contribution...

## Samples of use screenshot
![example](CurrencyTextBoxSample.png?raw=true)

## Features
- Numbers typed are pushed in from the right. If we start with the default value 0.00, and start typing the numbers 123, the value updates as such: 0.00 => 0.01 => 0.12 => 1.23
- You can set Maximum and Minimum value.
- If we press the backspace key, the numbers are shifted right: 1.23 => 0.12 => 0.01 => 0.00
- If we press the delete key, the value is reset to 0.00.
- If we press the minus key, the value becomes negative.
- If we press down/up key, the value are increase/decrease
- Support stringformat C to C6.
- Support stringformat N to N6.
- Support stringformat P to P6.
- Copy and paste decimal value. ctrl+c / ctrl+v
- Undo/Redo value with default ctrl+z / ctrl+y
- This control's template can be customized to change the appearance.
- Supports data validation.
- Auto color (Red/Black) when negative or positive value 
- ...

## Release Notes
New in version 1.4.0
- ADD support of percent string format (P to P6)

New in version 1.3.1
- BUG FIX When popup closed the window do not freeze
- Little desing update un popup
- Add PopupClosed event

New in version 1.3.0 
- Add support of Undolimit
- Add functionality ADD/REMOVE popup (Enter Key when CanShowAddPanel = true) 

New in version 1.2.0 
- Add support of numeric value (Stringformat N to N6) 

New in version 1.1.1 
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


<currency:CurrencyTextBox x:Name="myCurrencyTextBox" Number="{Binding Number, UpdateSourceTrigger=PropertyChanged, ValidatesOnDataErrors=True}">
      <currency:CurrencyTextBox.Style>
          <Style TargetType="{x:Type currency:CurrencyTextBox}">
              <Style.Triggers>
                  <Trigger Property="Validation.HasError" Value="True">
                      <Setter Property="ToolTip" Value="{Binding (Validation.Errors).CurrentItem.ErrorContent, RelativeSource={RelativeSource self}}" />
                  </Trigger>
              </Style.Triggers>
          </Style>
      </currency:CurrencyTextBox.Style>
</currency:CurrencyTextBox>
```
