[![KGy SOFT .net](https://user-images.githubusercontent.com/27336165/124292367-c93f3d00-db55-11eb-8003-6d943ee7d7fa.png)](https://kgysoft.net)

# KGy SOFT WinForms Libraries

KGy SOFT WinForms Libraries offer advanced Windows Forms controls and other useful components for .NET Framework and .NET Core applications. The libraries support every Windows version starting with Windows XP, and every platform target starting with .NET Framework 3.5 and .NET Core 3.0.<!-- With some limitations, Mono/Linux is also supported.-->

<!--[![Website](https://img.shields.io/website/https/kgysoft.net/winforms.svg)](https://kgysoft.net/winforms)-->
<!--[![Online Help](https://img.shields.io/website/https/docs.kgysoft.net/winforms.svg?label=online%20help&up_message=available)](https://docs.kgysoft.net/winforms)-->
[![GitHub Repo](https://img.shields.io/github/repo-size/koszeggy/KGySoft.WinForms.svg?label=github)](https://github.com/koszeggy/KGySoft.WinForms)
[![Nuget](https://img.shields.io/nuget/vpre/KGySoft.WinForms.svg)](https://www.nuget.org/packages/KGySoft.WinForms)

## Table of Contents:
1. [Download](#download)
   - [Download Binaries](#download-binaries)
   - [Example Application](#example-application)
4. [Release Notes](#release-notes)
5. [Examples](#examples)
   - [Advanced Common Controls](#advanced-common-controls)
   - [New Control Types](#new-control-types)
   - [Advanced Dialogs](#advanced-dialogs)
6. [License](#license)
<!--2. [Project Site](#project-site)
3. [Documentation](#documentation)-->

## Download:

### Download Binaries:

The binaries can be downloaded as a NuGet package directly from [nuget.org](https://www.nuget.org/packages/KGySoft.WinForms)

However, the preferred way is to install the package in VisualStudio either by looking for the `KGySoft.WinForms` package in the Nuget Package Manager GUI, or by sending the following command at the Package Manager Console prompt:

    PM> Install-Package KGySoft.WinForms
    
### Example Application:

<p align="center">
  <img alt="A TaskDialog in the KGySoft.WinForms.Example application" src="https://github.com/user-attachments/assets/ad99b5f6-84ae-4306-b610-68549eb348ce"/>
  <br/><em>Try the Examples application from the <a href="KGySoft.WinForms.Example">KGySoft.WinForms.Example</a> folder or download it from the <a href="https://github.com/koszeggy/KGySoft.WinForms/releases">Releases</a>.</em>
</p>

<!--## Project Site

Find the project site at [kgysoft.net](https://kgysoft.net/winforms/).

## Documentation

* You can find the online KGy SOFT Core Libraries documentation [here](https://docs.kgysoft.net/winforms).
* See [this](https://docs.kgysoft.net) link to access the online documentation of all KGy SOFT libraries.
-->

## Release Notes

See the [change log](https://github.com/koszeggy/KGySoft.WinForms/blob/master/KGySoft.WinForms/changelog.txt).

## Examples

### Advanced Common Controls

<details>
<summary><strong>Overview</strong></summary><p/>

The KGy SOFT WinForms libraries contain several advanced controls that are all derived from the standard Windows Forms controls, but offer with additional features and fixes for common issues. These controls include: `AdvancedButton`, `AdvancedCheckBox`, `AdvancedComboBox`, `AdvancedDateTimePicker`, `AdvancedLabel`, `AdvancedProgressBar`, `AdvancedRadioButton` and `AdvancedTextBox`.

Exception with `AdvancedProgressBar`, they all support custom disabled colors (which is normally not adjustable) and fixed auto scaling when the application has per-monitor DPI awareness enabled. Additionally, `AdvancedButton`, `AdvancedCheckBox`, `AdvancedRadioButton` and `AdvancedLabel` support buffered fading animations with every flat style. See more details in the following sections.

</details>

<details>
<summary><strong>Advanced Base Controls</strong><a id="advanced-base-controls"/></summary><p/>

To implement your own advanced controls, you can derive from the `BaseControl`, `BaseUserControl` and `BaseForm` classes. Most notable features:
* They remove all event subscriptions when disposed.
* All have an `InvokeOnUIThread` method. It is similar as combining `InvokeRequired` and `Invoke`, but works correctly even when the control is not created yet, in which case `InvokeRequired` cannot be trusted.
* They all have an `IsDesignMode` property, which is similar to `DesignMode`, but works correctly in all cases, even in the constructor an in virtual methods called from the constructor.
* `BaseControl` provides an event for horizontal scrolling, which is not available in the standard `Control` class.
* `BaseForm` and `BaseUserControl` have a `DynamicStringLocalization` property that allows enabling simple localization of the controls localizable string properties directly from .resx files. Localizations for non-existing translations can be automatically generated, and changes can be applied at runtime without restarting the application.
* `BaseForm` provides several events and overridable methods to support per-monitor DPI awareness for all target platforms, including .NET Framework 3.5 and 4.x. These can be useful even if you target newer platforms where the standard `Form` class already supports per-monitor DPI awareness, because the standard implementation has some issues, especially when the application has older awareness settings.

<p align="center">
  <img alt="Editing self resources in the KGySoft.WinForms.Example application" src="https://github.com/user-attachments/assets/45e10d0d-1ae3-49fb-a3aa-93058c17bfa2"/>
  <br/><em>Editing newly generated resources for a new language in the KGySoft.WinForms.Example application</em>
</p>

</details>

<details>
<summary><strong><code>AdvancedButton</code></strong><a id="advancedbutton"/></summary><p/>
TODO
</details>

<details>
<summary><strong><code>AdvancedCheckBox</code></strong><a id="advancedcheckbox"/></summary><p/>
TODO
</details>

<details>
<summary><strong><code>AdvancedComboBox</code></strong><a id="advancedcombobox"/></summary><p/>
TODO
</details>

<details>
<summary><strong><code>AdvancedDateTimePicker</code></strong><a id="advanceddatetimepicker"/></summary><p/>
TODO
</details>

<details>
<summary><strong><code>AdvancedLabel</code></strong><a id="advancedlabel"/></summary><p/>
TODO
</details>

<details>
<summary><strong><code>AdvancedPanel</code></strong><a id="advancedpanel"/></summary><p/>
TODO
</details>

<details>
<summary><strong><code>AdvancedProgressBar</code></strong><a id="advancedprogressbar"/></summary><p/>
TODO
</details>

<details>
<summary><strong><code>AdvancedRadioButton</code></strong><a id="advancedradiobutton"/></summary><p/>
TODO
</details>

<details>
<summary><strong><code>AdvancedTextBox</code></strong><a id="advancedtextbox"/></summary><p/>
TODO
</details>

### New Control Types

<details open>
<summary><strong><code>ImageViewer</code></strong><a id="imageviewer"/></summary><p/>
TODO
</details>

<details>
<summary><strong><code>CheckGroupBox</code></strong><a id="checkgroupbox"/></summary><p/>
TODO
</details>

<details>
<summary><strong><code>CommandLinkButton</code></strong><a id="commandlinkbutton"/></summary><p/>
TODO
</details>

### Advanced Dialogs

<details>
<summary><strong>Task Dialog</strong><a id="task-dialog"/></summary><p/>
TODO
</details>

<details>
<summary><strong>Message Dialogs</strong><a id="message-dialogs"/></summary><p/>
TODO
</details>

<details>
<summary><strong>Input Dialog</strong><a id="input-dialog"/></summary><p/>
TODO
</details>

## License
KGy SOFT Core Libraries are under the [KGy SOFT License 1.0](https://github.com/koszeggy/KGySoft.CoreLibraries/blob/master/LICENSE), which is a permissive GPL-like license. It allows you to copy and redistribute the material in any medium or format for any purpose, even commercially. The only thing is not allowed is to distribute a modified material as yours: though you are free to change and re-use anything, do that by giving appropriate credit. See the [LICENSE](https://github.com/koszeggy/KGySoft.CoreLibraries/blob/master/LICENSE) file for details.

---

See the complete KGy SOFT Core Libraries documentation with even more examples at [docs.kgysoft.net](https://docs.kgysoft.net/corelibraries).

[![KGy SOFT .net](https://user-images.githubusercontent.com/27336165/124292367-c93f3d00-db55-11eb-8003-6d943ee7d7fa.png)](https://kgysoft.net)
