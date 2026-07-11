<!--[![KGy SOFT .net](https://user-images.githubusercontent.com/27336165/124292367-c93f3d00-db55-11eb-8003-6d943ee7d7fa.png)](https://kgysoft.net)-->
# KGy SOFT WinForms Libraries

KGy SOFT WinForms Libraries offer advanced Windows Forms controls and other useful components for .NET Framework and .NET Core applications. The libraries support every Windows version starting with Windows XP, and every platform target starting with .NET Framework 3.5 and .NET Core 3.0.<!-- With some limitations, Mono/Linux is also supported.-->

<!--[![Website](https://img.shields.io/website/https/kgysoft.net/winforms.svg)](https://kgysoft.net/winforms)-->
[![Online Help](https://img.shields.io/website/https/koszeggy.github.io/docs/winforms.svg?label=online%20help&up_message=available)](https://koszeggy.github.io/docs/winforms)
[![GitHub Repo](https://img.shields.io/github/repo-size/koszeggy/KGySoft.WinForms.svg?label=github)](https://github.com/koszeggy/KGySoft.WinForms)
[![Nuget](https://img.shields.io/nuget/vpre/KGySoft.WinForms.svg)](https://www.nuget.org/packages/KGySoft.WinForms)

## Table of Contents:
1. [Download](#download)
   - [Example Application](#example-application)
2. [Documentation](#documentation)
3. [Release Notes](#release-notes)
4. [Examples](#examples)
   - [Advanced Common Controls](#advanced-common-controls)
   - [New Control Types](#new-control-types)
   - [Other Components](#other-components)
5. [License](#license)

## Download

The binaries can be downloaded as a NuGet package directly from [nuget.org](https://www.nuget.org/packages/KGySoft.WinForms)

However, the preferred way is to install the package in VisualStudio either by looking for the `KGySoft.WinForms` package in the Nuget Package Manager GUI, or by sending the following command at the Package Manager Console prompt:

    PM> Install-Package KGySoft.WinForms
    
 ### Example Application

<p align="center">
  <img alt="A TaskDialog in the KGySoft.WinForms.Example application" src="https://github.com/user-attachments/assets/ad99b5f6-84ae-4306-b610-68549eb348ce"/>
  <br/><em>Try the Examples application from the <a href="KGySoft.WinForms.Example">KGySoft.WinForms.Example</a> folder or download it at <a href="https://github.com/koszeggy/KGySoft.WinForms/releases">Releases</a>.</em>
</p> 

<!--## Project Site

Find the project site at [kgysoft.net](https://kgysoft.net/winforms/).
-->

## Documentation

* You can find the online KGy SOFT WinForms Libraries documentation [here](https://koszeggy.github.io/docs/winforms).
* See [this](https://koszeggy.github.io/docs) link to access the online documentation of all KGy SOFT libraries.

## Release Notes

See the [change log](https://github.com/koszeggy/KGySoft.WinForms/blob/master/KGySoft.WinForms/changelog.txt).

## Examples

### Advanced Common Controls

<details>
<summary><strong>Overview</strong></summary><p/>

The KGy SOFT WinForms libraries contain several advanced controls that are all derived from the standard Windows Forms controls, but offer additional features and fixes for common issues. These controls include: [`AdvancedButton`](https://koszeggy.github.io/docs/winforms/html/T_KGySoft_WinForms_Controls_AdvancedButton.htm), [`AdvancedCheckBox`](https://koszeggy.github.io/docs/winforms/html/T_KGySoft_WinForms_Controls_AdvancedCheckBox.htm), [`AdvancedComboBox`](https://koszeggy.github.io/docs/winforms/html/T_KGySoft_WinForms_Controls_AdvancedComboBox.htm), [`AdvancedDateTimePicker`](https://koszeggy.github.io/docs/winforms/html/T_KGySoft_WinForms_Controls_AdvancedDateTimePicker.htm), [`AdvancedLabel`](https://koszeggy.github.io/docs/winforms/html/T_KGySoft_WinForms_Controls_AdvancedLabel.htm), [`AdvancedProgressBar`](https://koszeggy.github.io/docs/winforms/html/T_KGySoft_WinForms_Controls_AdvancedProgressBar.htm), [`AdvancedRadioButton`](https://koszeggy.github.io/docs/winforms/html/T_KGySoft_WinForms_Controls_AdvancedRadioButton.htm) and [`AdvancedTextBox`](https://koszeggy.github.io/docs/winforms/html/T_KGySoft_WinForms_Controls_AdvancedTextBox.htm).

Exception with [`AdvancedProgressBar`](https://koszeggy.github.io/docs/winforms/html/T_KGySoft_WinForms_Controls_AdvancedProgressBar.htm), they all support custom disabled colors (which is normally not adjustable) and fixed auto font scaling when the application has per-monitor DPI awareness enabled. Additionally, [`AdvancedButton`](https://koszeggy.github.io/docs/winforms/html/T_KGySoft_WinForms_Controls_AdvancedButton.htm), [`AdvancedCheckBox`](https://koszeggy.github.io/docs/winforms/html/T_KGySoft_WinForms_Controls_AdvancedCheckBox.htm), [`AdvancedRadioButton`](https://koszeggy.github.io/docs/winforms/html/T_KGySoft_WinForms_Controls_AdvancedRadioButton.htm) and [`AdvancedLabel`](https://koszeggy.github.io/docs/winforms/html/T_KGySoft_WinForms_Controls_AdvancedLabel.htm) support buffered fading animations with every flat style. See more details in the following sections.

> 💡 **Tip**<p/>
> Try the example application from the [KGySoft.WinForms.Example](KGySoft.WinForms.Example) folder or download its binaries at the [Releases](https://github.com/koszeggy/KGySoft.WinForms/releases) page.

</details>

<details>
<summary><strong>Advanced Base Controls</strong><a id="advanced-base-controls"/></summary><p/>

To implement your own advanced controls, you can derive from the [`BaseControl`](https://koszeggy.github.io/docs/winforms/html/T_KGySoft_WinForms_Controls_BaseControl.htm), [`BaseUserControl`](https://koszeggy.github.io/docs/winforms/html/T_KGySoft_WinForms_Controls_BaseUserControl.htm) and [`BaseForm`](https://koszeggy.github.io/docs/winforms/html/T_KGySoft_WinForms_Forms_BaseForm.htm) classes. Most notable features:
* They remove all event subscriptions when disposed.
* All have an [`InvokeOnUIThread`](https://koszeggy.github.io/docs/winforms/html/M_KGySoft_WinForms_Forms_BaseForm_InvokeOnUIThread.htm) method. It is similar as combining `InvokeRequired` and `Invoke`, but works correctly even when the control is not created yet, in which case `InvokeRequired` cannot be trusted.
* They all have an [`IsDesignMode`](https://koszeggy.github.io/docs/winforms/html/P_KGySoft_WinForms_Forms_BaseForm_IsDesignMode.htm) property, which is similar to `DesignMode`, but works correctly in all cases, even in the constructor and in virtual methods called from the constructor.
* [`BaseControl`](https://koszeggy.github.io/docs/winforms/html/T_KGySoft_WinForms_Controls_BaseControl.htm) provides an event for horizontal scrolling, which is not available in the standard `Control` class.
* [`BaseForm`](https://koszeggy.github.io/docs/winforms/html/T_KGySoft_WinForms_Forms_BaseForm.htm) and [`BaseUserControl`](https://koszeggy.github.io/docs/winforms/html/T_KGySoft_WinForms_Controls_BaseUserControl.htm) have a [`DynamicStringLocalization`](https://koszeggy.github.io/docs/winforms/html/P_KGySoft_WinForms_Forms_BaseForm_DynamicStringLocalization.htm) property that allows enabling simple localization of the controls' localizable string properties directly from .resx files. Localizations for non-existing translations can be automatically generated, and changes can be applied at runtime without restarting the application.
* [`BaseForm`](https://koszeggy.github.io/docs/winforms/html/T_KGySoft_WinForms_Forms_BaseForm.htm) provides several events and overridable methods to support per-monitor DPI awareness for all target platforms, including .NET Framework 3.5 and 4.x. These can be useful even if you target newer platforms where the standard `Form` class already supports per-monitor DPI awareness, because the standard implementation has some issues, especially when the application has older awareness settings.

<p align="center">
  <img alt="Editing self resources in the KGySoft.WinForms.Example application" src="https://github.com/user-attachments/assets/9100c489-2f99-4956-aa60-49037fe7903f"/>
  <br/><em>Editing newly generated resources for a new language in the <a href="KGySoft.WinForms.Example">KGySoft.WinForms.Example</a> application</em>
</p>

</details>

<details>
<summary><strong><a href="https://koszeggy.github.io/docs/winforms/html/T_KGySoft_WinForms_Controls_AdvancedButton.htm"><code>AdvancedButton</code></a></strong><a id="advancedbutton"/></summary><p/>

[`AdvancedButton`](https://koszeggy.github.io/docs/winforms/html/T_KGySoft_WinForms_Controls_AdvancedButton.htm) supports the following additional features compared to the standard `Button` control:
* Adjustable disabled colors
* Buffered fading animations for any `FlatStyle` (only for Windows Vista and later when visual styles are enabled)
* Elevated mode (system shield icon)
* Consistent font scaling on all platforms when per-monitor DPI awareness is enabled
* Small visual enhancements, especially in high contrast mode
* Different text rendering quality options

<p align="center">
  <img alt="AdvancedButton in the KGySoft.WinForms.Example application" src="https://github.com/user-attachments/assets/de1dd6e4-6a85-4002-ac9f-633191ba4656"/>
  <br/><em><a href="https://koszeggy.github.io/docs/winforms/html/T_KGySoft_WinForms_Controls_AdvancedButton.htm"><code>AdvancedButton</code></a> in the <a href="KGySoft.WinForms.Example">KGySoft.WinForms.Example</a> application</em>
</p>

</details>

<details>
<summary><strong><a href="https://koszeggy.github.io/docs/winforms/html/T_KGySoft_WinForms_Controls_AdvancedCheckBox.htm" target="_blank"><code>AdvancedCheckBox</code></a>/<a href="https://koszeggy.github.io/docs/winforms/html/T_KGySoft_WinForms_Controls_AdvancedRadioButton.htm" target="_blank"><code>AdvancedRadioButton</code></a></strong><a id="advancedcheckbox-advancedradiobutton"/></summary><p/>

[`AdvancedCheckBox`](https://koszeggy.github.io/docs/winforms/html/T_KGySoft_WinForms_Controls_AdvancedCheckBox.htm) and [`AdvancedRadioButton`](https://koszeggy.github.io/docs/winforms/html/T_KGySoft_WinForms_Controls_AdvancedRadioButton.htm) support the following additional features compared to the standard `CheckBox`/`RadioButton` controls:
* Adjustable disabled colors
* Fixed `AutoSize` behavior when the control is docked (see also [`AdvancedLabel`](#advancedlabel))
* Buffered fading animations for any `FlatStyle` (only for Windows Vista and later when visual styles are enabled)
* Consistent font scaling on all platforms when per-monitor DPI awareness is enabled
* Visual enhancements, especially in high contrast mode and with high DPI
* Different text and visual rendering quality options

<p align="center">
  <img alt="AdvancedRadioButton in the KGySoft.WinForms.Example application" src="https://github.com/user-attachments/assets/59d07bd2-ac94-4229-82e4-7bcb076b73b8"/>
  <br/><em><code>RadioButton</code> vs. <a href="https://koszeggy.github.io/docs/winforms/html/T_KGySoft_WinForms_Controls_AdvancedRadioButton.htm" target="_blank"><code>AdvancedRadioButton</code></a> on Windows 11 with high DPI (150%), targeting .NET 9.0</em>
</p>

</details>

<details>
<summary><strong><a href="https://koszeggy.github.io/docs/winforms/html/T_KGySoft_WinForms_Controls_AdvancedComboBox.htm" target="_blank"><code>AdvancedComboBox</code></a></strong><a id="advancedcombobox"/></summary><p/>

[`AdvancedComboBox`](https://koszeggy.github.io/docs/winforms/html/T_KGySoft_WinForms_Controls_AdvancedComboBox.htm) supports the following most notable additional features compared to the standard `ComboBox` control:
* Adjustable disabled colors
* Read-only mode (similarly to a `TextBox`)
* Consistent font scaling on all platforms when per-monitor DPI awareness is enabled
* Other minor enhancements and fixes

<p align="center">
  <img alt="AdvancedComboBox in the KGySoft.WinForms.Example application" src="KGySoft.WinForms/Help/AdvancedComboBox.gif"/>
  <br/><em><a href="https://koszeggy.github.io/docs/winforms/html/T_KGySoft_WinForms_Controls_AdvancedComboBox.htm" target="_blank"><code>AdvancedComboBox</code></a> in the <a href="KGySoft.WinForms.Example">KGySoft.WinForms.Example</a> application</em>
</p>

</details>

<details>
<summary><strong><a href="https://koszeggy.github.io/docs/winforms/html/T_KGySoft_WinForms_Controls_AdvancedDateTimePicker.htm" target="_blank"><code>AdvancedDateTimePicker</code></a></strong><a id="advanceddatetimepicker"/></summary><p/>

The regular `DateTimePicker` control does not allow custom colors, it does not even have publicly browsable `BackColor` and `ForeColor` properties. [`AdvancedDateTimePicker`](https://koszeggy.github.io/docs/winforms/html/T_KGySoft_WinForms_Controls_AdvancedDateTimePicker.htm) addresses this issue, allowing even custom disabled colors. When visual styles are enabled and the control is in focus, it switches back to the default rendering. Additionally, it fixes a sort of rendering issues, especially in high DPI scenarios.

<p align="center">
  <img alt="AdvancedDateTimePicker in the KGySoft.WinForms.Example application" src="https://github.com/user-attachments/assets/559d88ee-ab03-4fec-81a0-3434ec366f06"/>
  <br/><em><code>DateTimePicker</code> vs. <a href="https://koszeggy.github.io/docs/winforms/html/T_KGySoft_WinForms_Controls_AdvancedDateTimePicker.htm" target="_blank"><code>AdvancedDateTimePicker</code></a> on Windows 11 with high DPI (125%), targeting .NET 9.0</em>
</p>

</details>

<details>
<summary><strong><a href="https://koszeggy.github.io/docs/winforms/html/T_KGySoft_WinForms_Controls_AdvancedLabel.htm" target="_blank"><code>AdvancedLabel</code></a></strong><a id="advancedlabel"/></summary><p/>

[`AdvancedLabel`](https://koszeggy.github.io/docs/winforms/html/T_KGySoft_WinForms_Controls_AdvancedLabel.htm) supports the following additional features compared to the standard `Label`/`LinkLabel` controls:
* Adjustable disabled colors
* Fixed `AutoSize` behavior when the control is docked
* Buffered fading animations (only for Windows Vista and later when visual styles are enabled). By default, affects toggling the `Enabled` state only, but can be applied to any visual change.
* Consistent font scaling on all platforms when per-monitor DPI awareness is enabled
* Different text and visual rendering quality options
* Auto hyperlink recognition in text (when allowed)
* Visual enhancements, such as richer `BorderStyle` options or fixed hand cursor for hyperlinks
<p align="center">
  <img alt="AdvancedLabel in the KGySoft.WinForms.Example application" src="KGySoft.WinForms/Help/AdvancedLabel.png"/>
  <br/><em>Some <code>Label</code> vs. <a href="https://koszeggy.github.io/docs/winforms/html/T_KGySoft_WinForms_Controls_AdvancedLabel.htm" target="_blank"><code>AdvancedLabel</code></a> differences</em>
</p>

</details>

<details>
<summary><strong><a href="https://koszeggy.github.io/docs/winforms/html/T_KGySoft_WinForms_Controls_AdvancedPanel.htm" target="_blank"><code>AdvancedPanel</code></a></strong><a id="advancedpanel"/></summary><p/>

Just a `Panel`, whose `BorderStyle` has a wider range of options than the standard `Panel` control. See the [`AdvancedLabel`](#advancedlabel) for a visual comparison that has the same variety of border styles.

</details>

<details>
<summary><strong><a href="https://koszeggy.github.io/docs/winforms/html/T_KGySoft_WinForms_Controls_AdvancedProgressBar.htm" target="_blank"><code>AdvancedProgressBar</code></a></strong><a id="advancedprogressbar"/></summary><p/>

[`AdvancedProgressBar`](https://koszeggy.github.io/docs/winforms/html/T_KGySoft_WinForms_Controls_AdvancedProgressBar.htm) supports four different styles, including the system default style. Additionally, it supports paused and error states, as well as custom coloring for non-system styles. When visual styles are not available, every style is rendered with the classic appearance.

<p align="center">
  <img alt="AdvancedProgressBar in the KGySoft.WinForms.Example application" src="https://github.com/user-attachments/assets/9bc2c6ce-ab55-4446-b186-c764652cac14"/>
  <br/><em>The various styles of <a href="https://koszeggy.github.io/docs/winforms/html/T_KGySoft_WinForms_Controls_AdvancedProgressBar.htm" target="_blank"><code>AdvancedProgressBar</code></a></em>
</p>

</details>

<details>
<summary><strong><a href="https://koszeggy.github.io/docs/winforms/html/T_KGySoft_WinForms_Controls_AdvancedTextBox.htm" target="_blank"><code>AdvancedTextBox</code></a>/<a href="https://koszeggy.github.io/docs/winforms/html/T_KGySoft_WinForms_Controls_DecimalTextBox.htm" target="_blank"><code>DecimalTextBox</code></a></strong><a id="advancedtextbox-decimaltextbox"/></summary><p/>

Just like other advanced controls, [`AdvancedTextBox`](https://koszeggy.github.io/docs/winforms/html/T_KGySoft_WinForms_Controls_AdvancedTextBox.htm) supports adjustable disabled colors and consistent font scaling on all platforms when per-monitor DPI awareness is enabled. Additionally, it fixes some minor issues regarding corrupted fonts, `AcceptsTab` and `AcceptsReturn` handling in read-only mode, or fixing Ctrl+A (select all) behavior when auto appending is enabled.

[`DecimalTextBox`](https://koszeggy.github.io/docs/winforms/html/T_KGySoft_WinForms_Controls_DecimalTextBox.htm) is a specialized version of [`AdvancedTextBox`](https://koszeggy.github.io/docs/winforms/html/T_KGySoft_WinForms_Controls_AdvancedTextBox.htm) that can be used as a numeric input control. A fixed number of fractional digits can be specified, which can be even negative, allowing rounding to whole values. When entering a value, you can use multiplier keys `t`, `m`, `y` for thousand, million, and billion (yard) multipliers, respectively. For example, entering `1.5m` will result in `1,500,000`.

</details>

### New Control Types

<details open>
<summary><strong><a href="https://koszeggy.github.io/docs/winforms/html/T_KGySoft_WinForms_Controls_ImageViewer.htm" target="_blank"><code>ImageViewer</code></a></strong><a id="imageviewer"/></summary><p/>

The [`ImageViewer`](https://koszeggy.github.io/docs/winforms/html/T_KGySoft_WinForms_Controls_ImageViewer.htm) control is like a `PictureBox`, but it supports free zooming and panning. It can toggle smoothing, which can be used even for metafiles to apply antialiasing.

<p align="center">
  <img alt="ImageViewer in the KGySoft.WinForms.Example application" src="https://github.com/user-attachments/assets/8f6b75bd-9200-4fff-bf1e-0336350f302e"/>
  <br/><em>The <a href="https://koszeggy.github.io/docs/winforms/html/T_KGySoft_WinForms_Controls_ImageViewer.htm" target="_blank"><code>ImageViewer</code></a> control in the <a href="KGySoft.WinForms.Example">KGySoft.WinForms.Example</a> application</em>
</p>
</details>

<details>
<summary><strong><a href="https://koszeggy.github.io/docs/winforms/html/T_KGySoft_WinForms_Controls_CheckGroupBox.htm" target="_blank"><code>CheckGroupBox</code></a></strong><a id="checkgroupbox"/></summary><p/>

The [`CheckGroupBox`](https://koszeggy.github.io/docs/winforms/html/T_KGySoft_WinForms_Controls_CheckGroupBox.htm) control is a specialized `GroupBox` that has a check box in the header. It can be used to toggle the enabled state of the controls in the group box.

<p align="center">
  <img alt="CheckGroupBox in the KGySoft.Drawing.ImagingTools application" src="https://github.com/user-attachments/assets/2e456ff2-5e9d-4115-a6b2-9b3d77fd7050"/>
  <br/><em>The <a href="https://koszeggy.github.io/docs/winforms/html/T_KGySoft_WinForms_Controls_CheckGroupBox.htm" target="_blank"><code>CheckGroupBox</code></a> can be used to toggle the enabled status of every control in the group box</em>
</p>

</details>

<details>
<summary><strong><a href="https://koszeggy.github.io/docs/winforms/html/T_KGySoft_WinForms_Controls_CommandLinkButton.htm" target="_blank"><code>CommandLinkButton</code></a></strong><a id="commandlinkbutton"/></summary><p/>

A command link is a special button that can be used to present a choice of action to the user. It is practically a `Button`, but it has a larger size, and it can display an icon and a description text below the main text. The [`CommandLinkButton`](https://koszeggy.github.io/docs/winforms/html/T_KGySoft_WinForms_Controls_CommandLinkButton.htm) control is a specialized button that supports this functionality.

When [`FlatStyle`](https://koszeggy.github.io/docs/winforms/html/P_KGySoft_WinForms_Controls_CommandLinkButton_FlatStyle.htm) is set to `System`, the control is rendered by the system on Windows Vista and later, when visual styles are enabled. Otherwise, a compatible rendering is used. The actual appearance can be quite different on different Windows versions, with or without visual styles and with high contrast mode enabled.

In most cases, the [`CommandLinkButton`](https://koszeggy.github.io/docs/winforms/html/T_KGySoft_WinForms_Controls_CommandLinkButton.htm) control is used in a [task dialog](#task-dialog), which is a specialized dialog that can display command links. But if you need to use a custom dialog with command links that cannot be achieved by task dialogs, you can use the [`CommandLinkButton`](https://koszeggy.github.io/docs/winforms/html/T_KGySoft_WinForms_Controls_CommandLinkButton.htm) control directly.

<p align="center">
  <img alt="CommandLinkButtons in the KGySoft.WinForms.Example application" src="https://github.com/user-attachments/assets/6a3a6abc-14c1-4bc4-8ce0-04009fba3f74"/>
  <br/><em><a href="https://koszeggy.github.io/docs/winforms/html/T_KGySoft_WinForms_Controls_CommandLinkButton.htm" target="_blank"><code>CommandLinkButton</code></a> controls in the <a href="KGySoft.WinForms.Example">KGySoft.WinForms.Example</a> application on Windows 11, with visual styles enabled</em>
</p>

</details>

### Other Components

<details>
<summary><strong><a href="https://koszeggy.github.io/docs/winforms/html/T_KGySoft_WinForms_Components_AdvancedErrorProvider.htm" target="_blank"><code>AdvancedErrorProvider</code></a></strong><a id="task-dialog"/></summary><p/>

When the regular `ErrorProvider` component uses data binding by setting its `DataSource` property, it works only if the data bound items implement `IDataErrorInfo`. The [`AdvancedErrorProvider`](https://koszeggy.github.io/docs/winforms/html/T_KGySoft_WinForms_Components_AdvancedErrorProvider.htm) component offers a [`SetMessage`](https://koszeggy.github.io/docs/winforms/html/E_KGySoft_WinForms_Components_AdvancedErrorProvider_SetMessage.htm) event, which can be used to provide messages for any validation technique with data binding.

When used with custom icons, it also can ensure correct scaling, depending on the current DPI settings.

<p align="center">
  <img alt="AdvancedErrorProvider" src="KGySoft.WinForms/Help/AdvancedErrorProvider.png"/>
  <br/><em>Data bound <a href="https://koszeggy.github.io/docs/winforms/html/T_KGySoft_WinForms_Components_AdvancedErrorProvider.htm" target="_blank"><code>AdvancedErrorProvider</code></a> component using <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_ComponentModel_ValidatingObjectBase.htm"><code>ValidatingObjectBase</code></a> items in the <a href="KGySoft.WinForms.Example">KGySoft.WinForms.Example</a> application on Windows 11</em>
</p>

</details>

<details>
<summary><strong>Task Dialogs</strong><a id="task-dialog"/></summary><p/>

Task dialogs are specialized dialogs that can be used to present elaborate information to the user, and can offer possible actions in different ways. A task dialog can display a title, a main instruction, a message, a concealable detailed description, footer text, and can have command links, radio buttons, a check box and buttons. It can also display icons (a main icon and a footer icon), a progress bar, and supports using a timer. The task dialog is a native Windows component on Windows Vista and later, but the KGy SOFT [`TaskDialog`](https://koszeggy.github.io/docs/winforms/html/T_KGySoft_WinForms_Components_TaskDialog.htm) supports compatibility mode on earlier Windows versions or when visual styles are not enabled.

<p align="center">
  <img alt="A native TaskDialog in the KGySoft.WinForms.Example application" src="https://github.com/user-attachments/assets/e32433f5-9d96-4d43-abb4-4a1254ab069a"/>
  <br/><em>A <a href="https://koszeggy.github.io/docs/winforms/html/T_KGySoft_WinForms_Components_TaskDialog.htm" target="_blank"><code>TaskDialog</code></a> in the <a href="KGySoft.WinForms.Example">KGySoft.WinForms.Example</a> application on Windows 11 with visual styles using native rendering.</em>
</p>

> ⚠️ **Warning**<p/>
> .NET 5 also introduced task dialogs, so when targeting .NET 5 or later, referencing the KGy SOFT version requires using aliases or fully qualified type names. You might want to use the KGy SOFT's [`TaskDialog`](https://koszeggy.github.io/docs/winforms/html/T_KGySoft_WinForms_Components_TaskDialog.htm) even when targeting .NET 5 or later for the additional features available in compatible mode only, such as the custom icons for the command links and buttons, tool tips for buttons and radio buttons, fixed rendering in high-contrast mode, richer result when copying the dialog content to the clipboard, and more.

<p align="center">
  <img alt="A TaskDialog in the KGySoft.WinForms.Example application using compatible rendering on Windows XP" src="https://github.com/user-attachments/assets/8ee02c8a-f7c9-44bd-a61d-ac51c2d25661"/>
  <br/><em>The same dialog as above, using compatible rendering on Windows XP.</em>
</p>

By default, the task dialog uses the native Windows component when available, and switches to compatible rendering otherwise, or when the configuration of your [`TaskDialog`](https://koszeggy.github.io/docs/winforms/html/T_KGySoft_WinForms_Components_TaskDialog.htm) uses some compatible-mode-only features (e.g. button icons or localization of standard buttons). You can also force compatible mode by setting the [`ForceCompatibilityMode`](https://koszeggy.github.io/docs/winforms/html/P_KGySoft_WinForms_Components_TaskDialog_ForceCompatibilityMode.htm) property to `true`, which unlocks some improvements compared to the native dialog (e.g. tool tips, rendering fixes, better RTL mode support, richer clipboard content, etc.).

<p align="center">
  <img alt="A TaskDialog with custom icons in the KGySoft.WinForms.Example application using compatible rendering on Windows 11" src="https://github.com/user-attachments/assets/50d4d0ed-7cf7-426c-832a-26dd584a10b6"/>
  <br/><em>Compatibility mode allows some non-native features like custom command link icons.</em>
</p>

</details>

<details>
<summary><strong>Message Dialogs</strong><a id="message-dialogs"/></summary><p/>

The static [`Dialogs`](https://koszeggy.github.io/docs/winforms/html/T_KGySoft_WinForms_Forms_Dialogs.htm) class provides several static methods to display message dialogs of information, warning, error and confirmation messages. By default, these methods use the standard `MessageBox.Show` method internally, but you can opt-in to use the KGy SOFT [`TaskDialog`](https://koszeggy.github.io/docs/winforms/html/T_KGySoft_WinForms_Components_TaskDialog.htm) instead, by setting the [`UseTaskDialogs`](https://koszeggy.github.io/docs/winforms/html/P_KGySoft_WinForms_Forms_Dialogs_UseTaskDialogs.htm) property to `true`. Using task dialogs instead of message boxes can be beneficial when the application has per-monitor DPI awareness enabled, because the standard message box is adjusted to the DPI of the primary display in the moment of starting the application, and it may not scale correctly on other displays or when the DPI changes later.

<p align="center">
  <img alt="Dialogs.InfoMessage message when it just uses MessageBox" src="https://github.com/user-attachments/assets/98000ddd-c3a1-4702-b8d8-8a2050c7e7b8"/>
  <br/><em><a href="https://koszeggy.github.io/docs/winforms/html/M_KGySoft_WinForms_Forms_Dialogs_InfoMessage.htm" target="_blank"><code>Dialogs.InfoMessage</code></a> example with the default behavior when it just uses <code>MessageBox.Show</code>.</em>
</p>

</details>

<details>
<summary><strong>Input Dialog</strong><a id="input-dialog"/></summary><p/>

The static [`Dialogs`](https://koszeggy.github.io/docs/winforms/html/T_KGySoft_WinForms_Forms_Dialogs.htm) class has various [`InputDialog`](https://koszeggy.github.io/docs/winforms/html/Overload_KGySoft_WinForms_Forms_Dialogs_InputDialog.htm) overloads that can be used to display a dialog for entering a single line of text. A caption and an optional prompt text can be specified along with a default value, which is pre-selected in the input field.

<p align="center">
  <img alt="Dialogs.InputDialog example" src="https://github.com/user-attachments/assets/700f9176-ca15-484f-8554-6fd7713d622b"/>
  <br/><em><a href="https://koszeggy.github.io/docs/winforms/html/M_KGySoft_WinForms_Forms_Dialogs_InputDialog_4.htm" target="_blank"><code>Dialogs.InputDialog</code></a> example on Windows 11.</em>
</p>

</details>

## License
KGy SOFT WinForms Libraries are under the [KGy SOFT License 1.0](https://github.com/koszeggy/KGySoft.WinForms/blob/master/LICENSE), which is a permissive GPL-like license. It allows you to copy and redistribute the material in any medium or format for any purpose, even commercially. The only thing is not allowed is to distribute a modified material as yours: though you are free to change and re-use anything, do that by giving appropriate credit. See the [LICENSE](https://github.com/koszeggy/KGySoft.WinForms/blob/master/LICENSE) file for details.

<!-----
See the complete KGy SOFT WinForms Libraries documentation with even more examples at the [docs](https://koszeggy.github.io/docs/winforms) site.-->

<!--[![KGy SOFT .net](https://user-images.githubusercontent.com/27336165/124292367-c93f3d00-db55-11eb-8003-6d943ee7d7fa.png)](https://kgysoft.net)
-->
