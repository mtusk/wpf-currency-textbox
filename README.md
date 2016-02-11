# WPF Currency TextBox
A WPF TextBox for entering a currency value, similar to how a cash register works.

![example](example.gif?raw=true)

## Features
- Numbers typed are pushed in from the right. If we start with the default value 0.00, and start typing the numbers 123, the value updates as such: 0.00 => 0.01 => 0.12 => 1.23
- If we press the backspace key, the numbers are shifted right: 1.23 => 0.12 => 0.01 => 0.00
- If we press the delete key, the value is reset to 0.00.
- If we press the minus key, the value becomes negative.
- Copy and paste are disabled (both via context menu and keyboard shortcuts)
- This control's template can be customized to change the appearance.
- Supports data validation.


