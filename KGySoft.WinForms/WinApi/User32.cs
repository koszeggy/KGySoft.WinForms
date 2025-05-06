#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: User32.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2025 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System;
using System.Runtime.InteropServices;

#endregion

namespace KGySoft.WinForms.WinApi
{
    internal static class User32
    {
        #region Methods

        /// <summary>
        /// The GetWindowDC function retrieves the device context (DC) for the entire window, including title bar, menus, and scroll bars.
        /// A window device context permits painting anywhere in a window, because the origin of the device context is the upper-left corner of the window instead of the client area.
        /// GetWindowDC assigns default attributes to the window device context each time it retrieves the device context. Previous attributes are lost.
        /// </summary>
        /// <param name="hWnd">Handle to the window with a device context that is to be retrieved. If this value is NULL, GetWindowDC retrieves the device context for the entire screen.</param>
        /// <returns>If the function succeeds, the return value is a handle to a device context for the specified window.
        /// If the function fails, the return value is NULL, indicating an error or an invalid hWnd parameter.
        /// </returns>
        [DllImport("user32.dll")]
        internal static extern IntPtr GetWindowDC(IntPtr hWnd);

        /// <summary>
        /// The ReleaseDC function releases a device context (DC), freeing it for use by other applications. The effect of the ReleaseDC function depends on the type of DC. It frees only common and window DCs. It has no effect on class or private DCs.
        /// </summary>
        /// <param name="hWnd">A handle to the window whose DC is to be released.</param>
        /// <param name="hDC">A handle to the DC to be released.</param>
        /// <returns>The return value indicates whether the DC was released. If the DC was released, the return value is 1.
        /// If the DC was not released, the return value is zero.</returns>
        /// <remarks>
        /// The application must call the ReleaseDC function for each call to the GetWindowDC function and for each call to the GetDC function that retrieves a common DC.
        /// An application cannot use the ReleaseDC function to release a DC that was created by calling the CreateDC function; instead, it must use the DeleteDC function. ReleaseDC must be called from the same thread that called GetDC.</remarks>
        [DllImport("user32.dll")]
        internal static extern bool ReleaseDC(IntPtr hWnd, IntPtr hDC);

        /// <summary>
        /// Changes the size, position, and Z order of a child, pop-up, or top-level window.
        /// These windows are ordered according to their appearance on the screen.
        /// The topmost window receives the highest rank and is the first window in the Z order.
        /// </summary>
        /// <param name="hWnd">A handle to the window.</param>
        /// <param name="hWndInsertAfter">A handle to the window to precede the positioned window in the Z order. This parameter must be a window handle or one of the following values.
        /// HWND_BOTTOM
        /// Places the window at the bottom of the Z order. If the hWnd parameter identifies a topmost window, the window loses its topmost status and is placed at the bottom of all other windows.
        /// HWND_NOTOPMOST
        /// Places the window above all non-topmost windows (that is, behind all topmost windows). This flag has no effect if the window is already a non-topmost window.
        /// HWND_TOP
        /// Places the window at the top of the Z order.
        /// HWND_TOPMOST
        /// Places the window above all non-topmost windows. The window maintains its topmost position even when it is deactivated.
        /// </param>
        /// <param name="X">Specifies the new position of the left side of the window, in client coordinates.</param>
        /// <param name="Y">Specifies the new position of the top of the window, in client coordinates.</param>
        /// <param name="cx">Specifies the new width of the window, in pixels.</param>
        /// <param name="cy">Specifies the new height of the window, in pixels.</param>
        /// <param name="uFlags">Specifies the window sizing and positioning flags. This parameter can be a combination of the following values.
        /// <para>SWP_ASYNCWINDOWPOS:
        /// If the calling thread and the thread that owns the window are attached to different input queues, the system posts the request to the thread that owns the window. This prevents the calling thread from blocking its execution while other threads process the request. </para>
        /// <para>SWP_DEFERERASE:
        /// Prevents generation of the WM_SYNCPAINT message. </para>
        /// <para>SWP_DRAWFRAME:
        /// Draws a frame (defined in the window's class description) around the window.</para>
        /// <para>SWP_FRAMECHANGED:
        /// Applies new frame styles set using the SetWindowLong function. Sends a WM_NCCALCSIZE message to the window, even if the window's size is not being changed. If this flag is not specified, WM_NCCALCSIZE is sent only when the window's size is being changed.</para>
        /// <para>SWP_HIDEWINDOW:
        /// Hides the window.</para>
        /// <para>SWP_NOACTIVATE:
        /// Does not activate the window. If this flag is not set, the window is activated and moved to the top of either the topmost or non-topmost group (depending on the setting of the hWndInsertAfter parameter).</para>
        /// <para>SWP_NOCOPYBITS:
        /// Discards the entire contents of the client area. If this flag is not specified, the valid contents of the client area are saved and copied back into the client area after the window is sized or repositioned.</para>
        /// <para>SWP_NOMOVE:
        /// Retains the current position (ignores X and Y parameters).</para>
        /// <para>SWP_NOOWNERZORDER:
        /// Does not change the owner window's position in the Z order.</para>
        /// <para>SWP_NOREDRAW:
        /// Does not redraw changes. If this flag is set, no repainting of any kind occurs. This applies to the client area, the nonclient area (including the title bar and scroll bars), and any part of the parent window uncovered as a result of the window being moved. When this flag is set, the application must explicitly invalidate or redraw any parts of the window and parent window that need redrawing.</para>
        /// <para>SWP_NOREPOSITION:
        /// Same as the SWP_NOOWNERZORDER flag.</para>
        /// <para>SWP_NOSENDCHANGING:
        /// Prevents the window from receiving the WM_WINDOWPOSCHANGING message.</para>
        /// <para>SWP_NOSIZE:
        /// Retains the current size (ignores the cx and cy parameters).</para>
        /// <para>SWP_NOZORDER:
        /// Retains the current Z order (ignores the hWndInsertAfter parameter).</para>
        /// <para>SWP_SHOWWINDOW
        /// Displays the window.</para>
        /// </param>
        /// <returns>If the function succeeds, the return value is nonzero.
        /// If the function fails, the return value is zero. To get extended error information, call GetLastError.
        /// </returns>
        [DllImport("user32.dll")]
        internal static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        /// <summary>
        /// The GetWindowRect function retrieves the dimensions of the bounding rectangle of the specified window.
        /// The dimensions are given in screen coordinates that are relative to the upper-left corner of the screen.
        /// </summary>
        /// <param name="hWnd">[in] Handle to the window.</param>
        /// <param name="lpRect">[out] Pointer to a structure that receives the screen coordinates of the upper-left and lower-right corners of the window.</param>
        /// <returns>If the function succeeds, the return value is nonzero.
        /// If the function fails, the return value is zero. To get extended error information, call GetLastError.
        /// </returns>
        [DllImport("user32.dll")]
        internal static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);

        /// <summary>
        /// The GetParent function retrieves a handle to the specified window's parent or owner.
        /// </summary>
        /// <param name="hWnd">[in] Handle to the window whose parent window handle is to be retrieved.</param>
        /// <returns>If the window is a child window, the return value is a handle to the parent window.
        /// If the window is a top-level window, the return value is a handle to the owner window.
        /// If the window is a top-level unowned window or if the function fails, the return value is NULL.
        /// To get extended error information, call GetLastError.
        /// For example, this would determine, when the function returns NULL, if the function failed or the window was a top-level window.</returns>
        [DllImport("user32.dll")]
        internal static extern IntPtr GetParent(IntPtr hWnd);

        /// <summary>
        /// The SetWindowText function changes the text of the specified window's title bar (if it has one).
        /// If the specified window is a control, the text of the control is changed.
        /// However, SetWindowText cannot change the text of a control in another application.
        /// </summary>
        /// <param name="hWnd">[in] Handle to the window or control whose text is to be changed.</param>
        /// <param name="lpString">[in] Pointer to a null-terminated string to be used as the new title or control text.</param>
        /// <returns>If the function succeeds, the return value is nonzero.
        /// If the function fails, the return value is zero. To get extended error information, call GetLastError.
        /// </returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        internal static extern bool SetWindowText(IntPtr hWnd, string lpString);

        /// <summary>
        /// The SendMessage function sends the specified message to a window or windows.
        /// It calls the window procedure for the specified window and does not return until the window procedure has processed the message.
        /// To send a message and return immediately, use the SendMessageCallback or SendNotifyMessage function.
        /// To post a message to a thread's message queue and return immediately, use the PostMessage or PostThreadMessage function.
        /// </summary>
        /// <param name="hWnd">[in] Handle to the window whose window procedure will receive the message.
        /// If this parameter is HWND_BROADCAST, the message is sent to all top-level windows in the system,
        /// including disabled or invisible unowned windows, overlapped windows, and pop-up windows; but the message is not sent to child windows.
        /// Microsoft Windows Vista and later: Message sending is subject to User Interface Privilege Isolation (UIPI).
        /// The thread of a process can send messages only to message queues of threads in processes of lesser or equal integrity level.
        /// </param>
        /// <param name="Msg">[in] Specifies the message to be sent.</param>
        /// <param name="wParam">[in] Specifies additional message-specific information.</param>
        /// <param name="lParam">[in] Specifies additional message-specific information.</param>
        /// <returns>The return value specifies the result of the message processing; it depends on the message sent.</returns>
        [DllImport("user32.dll")]
        internal static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        /// <summary>
        /// The SendMessage function sends the specified message to a window or windows.
        /// It calls the window procedure for the specified window and does not return until the window procedure has processed the message.
        /// To send a message and return immediately, use the SendMessageCallback or SendNotifyMessage function.
        /// To post a message to a thread's message queue and return immediately, use the PostMessage or PostThreadMessage function.
        /// </summary>
        /// <param name="hWnd">[in] Handle to the window whose window procedure will receive the message.
        /// If this parameter is HWND_BROADCAST, the message is sent to all top-level windows in the system,
        /// including disabled or invisible unowned windows, overlapped windows, and pop-up windows; but the message is not sent to child windows.
        /// Microsoft Windows Vista and later: Message sending is subject to User Interface Privilege Isolation (UIPI).
        /// The thread of a process can send messages only to message queues of threads in processes of lesser or equal integrity level.
        /// </param>
        /// <param name="Msg">[in] Specifies the message to be sent.</param>
        /// <param name="wParam">Specifies additional message-specific information.</param>
        /// <param name="lParam">Specifies additional message-specific information.</param>
        /// <returns>The return value specifies the result of the message processing; it depends on the message sent.</returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        internal static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, string? lParam);

        /// <summary>
        /// The SendMessage function sends the specified message to a window or windows.
        /// It calls the window procedure for the specified window and does not return until the window procedure has processed the message.
        /// To send a message and return immediately, use the SendMessageCallback or SendNotifyMessage function.
        /// To post a message to a thread's message queue and return immediately, use the PostMessage or PostThreadMessage function.
        /// </summary>
        /// <param name="hWnd">[in] Handle to the window whose window procedure will receive the message.
        /// If this parameter is HWND_BROADCAST, the message is sent to all top-level windows in the system,
        /// including disabled or invisible unowned windows, overlapped windows, and pop-up windows; but the message is not sent to child windows.
        /// Microsoft Windows Vista and later: Message sending is subject to User Interface Privilege Isolation (UIPI).
        /// The thread of a process can send messages only to message queues of threads in processes of lesser or equal integrity level.
        /// </param>
        /// <param name="Msg">[in] Specifies the message to be sent.</param>
        /// <param name="wParam">[in] Specifies additional message-specific information.</param>
        /// <param name="lParam">[in] Specifies additional message-specific information.</param>
        /// <returns>The return value specifies the result of the message processing; it depends on the message sent.</returns>
        [DllImport("user32.dll")]
        internal static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        /// <summary>
        /// The DestroyWindow function destroys the specified window.
        /// The function sends WM_DESTROY and WM_NCDESTROY messages to the window to deactivate it and remove the keyboard focus from it.
        /// The function also destroys the window's menu, flushes the thread message queue, destroys timers, removes clipboard ownership,
        /// and breaks the clipboard viewer chain (if the window is at the top of the viewer chain).
        /// If the specified window is a parent or owner window, DestroyWindow automatically destroys the associated child or
        /// owned windows when it destroys the parent or owner window. The function first destroys child or owned windows,
        /// and then it destroys the parent or owner window. DestroyWindow also destroys modeless dialog boxes created by the CreateDialog function.
        /// </summary>
        /// <param name="hwnd">[in] Handle to the window to be destroyed.</param>
        /// <returns>If the function succeeds, the return value is nonzero.
        /// If the function fails, the return value is zero. To get extended error information, call GetLastError.
        /// </returns>
        [DllImport("user32.dll")]
        internal static extern bool DestroyWindow(IntPtr hwnd);

        /// <summary>
        /// The GetDlgItem function retrieves a handle to a control in the specified dialog box.
        /// </summary>
        /// <param name="hDlg">[in] Handle to the dialog box that contains the control.</param>
        /// <param name="nIDDlgItem">[in] Specifies the identifier of the control to be retrieved.</param>
        /// <returns>If the function succeeds, the return value is the window handle of the specified control.
        /// If the function fails, the return value is NULL, indicating an invalid dialog box handle or a nonexistent control. To get extended error information, call GetLastError.
        /// </returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        internal static extern IntPtr GetDlgItem(IntPtr hDlg, int nIDDlgItem);

        /// <summary>
        /// Creates an overlapped, pop-up, or child window with an extended window style; otherwise, this function is identical to the CreateWindow function.
        /// For more information about creating a window and for full descriptions of the other parameters of CreateWindowEx, see CreateWindow.
        /// </summary>
        /// <param name="dwExStyle">[in] Specifies the extended window style of the window being created. This parameter can be one or more of the following values:
        /// WS_EX_ACCEPTFILES: Specifies that a window created with this style accepts drag-drop files.
        /// WS_EX_APPWINDOW: Forces a top-level window onto the taskbar when the window is visible.
        /// WS_EX_CLIENTEDGE: Specifies that a window has a border with a sunken edge.
        /// WS_EX_COMPOSITED: Windows XP: Paints all descendants of a window in bottom-to-top painting order using double-buffering. This cannot be used if the window has a class style of either CS_OWNDC or CS_CLASSDC.
        /// WS_EX_CONTEXTHELP: Includes a question mark in the title bar of the window. When the user clicks the question mark, the cursor changes to a question mark with a pointer. If the user then clicks a child window, the child receives a WM_HELP message. The child window should pass the message to the parent window procedure, which should call the WinHelp function using the HELP_WM_HELP command. The Help application displays a pop-up window that typically contains help for the child window.
        /// WS_EX_CONTEXTHELP: cannot be used with the WS_MAXIMIZEBOX or WS_MINIMIZEBOX styles.
        /// WS_EX_CONTROLPARENT: The window itself contains child windows that should take part in dialog box navigation. If this style is specified, the dialog manager recurses into children of this window when performing navigation operations such as handling the TAB key, an arrow key, or a keyboard mnemonic.
        /// WS_EX_DLGMODALFRAME: Creates a window that has a double border; the window can, optionally, be created with a title bar by specifying the WS_CAPTION style in the dwStyle parameter.
        /// WS_EX_LAYERED: Windows 2000/XP: Creates a layered window. Note that this cannot be used for child windows. Also, this cannot be used if the window has a class style of either CS_OWNDC or CS_CLASSDC.
        /// WS_EX_LAYOUTRTL: Arabic and Hebrew versions of Windows 98/Me, Windows 2000/XP: Creates a window whose horizontal origin is on the right edge. Increasing horizontal values advance to the left.
        /// WS_EX_LEFT: Creates a window that has generic left-aligned properties. This is the default.
        /// WS_EX_LEFTSCROLLBAR: If the shell language is Hebrew, Arabic, or another language that supports reading order alignment, the vertical scroll bar (if present) is to the left of the client area. For other languages, the style is ignored.
        /// WS_EX_LTRREADING: The window text is displayed using left-to-right reading-order properties. This is the default.
        /// WS_EX_MDICHILD: Creates a multiple-document interface (MDI) child window.
        /// WS_EX_NOACTIVATE: Windows 2000/XP: A top-level window created with this style does not become the foreground window when the user clicks it. The system does not bring this window to the foreground when the user minimizes or closes the foreground window.
        /// To activate the window, use the SetActiveWindow or SetForegroundWindow function.
        /// The window does not appear on the taskbar by default. To force the window to appear on the taskbar, use the WS_EX_APPWINDOW style.
        /// WS_EX_NOINHERITLAYOUT: Windows 2000/XP: A window created with this style does not pass its window layout to its child windows.
        /// WS_EX_NOPARENTNOTIFY: Specifies that a child window created with this style does not send the WM_PARENTNOTIFY message to its parent window when it is created or destroyed.
        /// WS_EX_OVERLAPPEDWINDOW: Combines the WS_EX_CLIENTEDGE and WS_EX_WINDOWEDGE styles.
        /// WS_EX_PALETTEWINDOW: Combines the WS_EX_WINDOWEDGE, WS_EX_TOOLWINDOW, and WS_EX_TOPMOST styles.
        /// WS_EX_RIGHT: The window has generic "right-aligned" properties. This depends on the window class. This style has an effect only if the shell language is Hebrew, Arabic, or another language that supports reading-order alignment; otherwise, the style is ignored.
        /// Using the WS_EX_RIGHT style for static or edit controls has the same effect as using the SS_RIGHT or ES_RIGHT style, respectively. Using this style with button controls has the same effect as using BS_RIGHT and BS_RIGHTBUTTON styles.
        /// WS_EX_RIGHTSCROLLBAR: Vertical scroll bar (if present) is to the right of the client area. This is the default.
        /// WS_EX_RTLREADING: If the shell language is Hebrew, Arabic, or another language that supports reading-order alignment, the window text is displayed using right-to-left reading-order properties. For other languages, the style is ignored.
        /// WS_EX_STATICEDGE: Creates a window with a three-dimensional border style intended to be used for items that do not accept user input.
        /// WS_EX_TOOLWINDOW: Creates a tool window; that is, a window intended to be used as a floating toolbar. A tool window has a title bar that is shorter than a normal title bar, and the window title is drawn using a smaller font. A tool window does not appear in the taskbar or in the dialog that appears when the user presses ALT+TAB. If a tool window has a system menu, its icon is not displayed on the title bar. However, you can display the system menu by right-clicking or by typing ALT+SPACE.
        /// WS_EX_TOPMOST: Specifies that a window created with this style should be placed above all non-topmost windows and should stay above them, even when the window is deactivated. To add or remove this style, use the SetWindowPos function.
        /// WS_EX_TRANSPARENT: Specifies that a window created with this style should not be painted until siblings beneath the window (that were created by the same thread) have been painted. The window appears transparent because the bits of underlying sibling windows have already been painted.
        /// To achieve transparency without these restrictions, use the SetWindowRgn function.
        /// WS_EX_WINDOWEDGE: Specifies that a window has a border with a raised edge.
        /// </param>
        /// <param name="lpClassName">[in] Pointer to a null-terminated string or a class atom created by a previous call to the RegisterClass or RegisterClassEx function. The atom must be in the low-order word of lpClassName; the high-order word must be zero. If lpClassName is a string, it specifies the window class name. The class name can be any name registered with RegisterClass or RegisterClassEx, provided that the module that registers the class is also the module that creates the window. The class name can also be any of the predefined system class names:
        /// BUTTON: Designates a small rectangular child window that represents a button the user can click to turn it on or off. Button controls can be used alone or in groups, and they can either be labeled or appear without text. Button controls typically change appearance when the user clicks them.
        /// COMBOBOX: Designates a control consisting of a list box and a selection field similar to an edit control. When using this style, an application should either display the list box at all times or enable a drop-down list box. If the list box is visible, typing characters into the selection field highlights the first list box entry that matches the characters typed. Conversely, selecting an item in the list box displays the selected text in the selection field.
        /// EDIT: Designates a rectangular child window into which the user can type text from the keyboard. The user selects the control and gives it the keyboard focus by clicking it or moving to it by pressing the TAB key. The user can type text when the edit control displays a flashing caret; use the mouse to move the cursor, select characters to be replaced, or position the cursor for inserting characters; or use the key to delete characters.
        /// LISTBOX: Designates a list of character strings. Specify this control whenever an application must present a list of names, such as filenames, from which the user can choose. The user can select a string by clicking it. A selected string is highlighted, and a notification message is passed to the parent window.
        /// MDICLIENT: Designates an MDI client window. This window receives messages that control the MDI application's child windows. The recommended style bits are WS_CLIPCHILDREN and WS_CHILD. Specify the WS_HSCROLL and WS_VSCROLL styles to create an MDI client window that allows the user to scroll MDI child windows into view.
        /// RichEdit: Designates a Microsoft Rich Edit 1.0 control. This window lets the user view and edit text with character and paragraph formatting, and can include embedded Component Object Model (COM) objects.
        /// RICHEDIT_CLASS: Designates a Rich Edit 2.0 control. This controls let the user view and edit text with character and paragraph formatting, and can include embedded COM objects.
        /// SCROLLBAR: Designates a rectangle that contains a scroll box and has direction arrows at both ends. The scroll bar sends a notification message to its parent window whenever the user clicks the control. The parent window is responsible for updating the position of the scroll box, if necessary.
        /// STATIC: Designates a simple text field, box, or rectangle used to label, box, or separate other controls. Static controls take no input and provide no output.
        /// </param>
        /// <param name="lpWindowName">[in] Pointer to a null-terminated string that specifies the window name. If the window style specifies a title bar, the window title pointed to by lpWindowName is displayed in the title bar. When using CreateWindow to create controls, such as buttons, check boxes, and static controls, use lpWindowName to specify the text of the control. When creating a static control with the SS_ICON style, use lpWindowName to specify the icon name or identifier. To specify an identifier, use the syntax "#num".</param>
        /// <param name="dwStyle">[in] Specifies the style of the window being created. This parameter can be a combination of window styles, plus the control styles.</param>
        /// <param name="x">[in] Specifies the initial horizontal position of the window. For an overlapped or pop-up window, the x parameter is the initial x-coordinate of the window's upper-left corner, in screen coordinates. For a child window, x is the x-coordinate of the upper-left corner of the window relative to the upper-left corner of the parent window's client area. If x is set to CW_USEDEFAULT, the system selects the default position for the window's upper-left corner and ignores the y parameter. CW_USEDEFAULT is valid only for overlapped windows; if it is specified for a pop-up or child window, the x and y parameters are set to zero.</param>
        /// <param name="y">[in] Specifies the initial vertical position of the window. For an overlapped or pop-up window, the y parameter is the initial y-coordinate of the window's upper-left corner, in screen coordinates. For a child window, y is the initial y-coordinate of the upper-left corner of the child window relative to the upper-left corner of the parent window's client area. For a list box y is the initial y-coordinate of the upper-left corner of the list box's client area relative to the upper-left corner of the parent window's client area.
        /// If an overlapped window is created with the WS_VISIBLE style bit set and the x parameter is set to CW_USEDEFAULT, then the y parameter determines how the window is shown. If the y parameter is CW_USEDEFAULT, then the window manager calls ShowWindow with the SW_SHOW flag after the window has been created. If the y parameter is some other value, then the window manager calls ShowWindow with that value as the nCmdShow parameter. </param>
        /// <param name="nWidth">[in] Specifies the width, in device units, of the window. For overlapped windows, nWidth is the window's width, in screen coordinates, or CW_USEDEFAULT. If nWidth is CW_USEDEFAULT, the system selects a default width and height for the window; the default width extends from the initial x-coordinates to the right edge of the screen; the default height extends from the initial y-coordinate to the top of the icon area. CW_USEDEFAULT is valid only for overlapped windows; if CW_USEDEFAULT is specified for a pop-up or child window, the nWidth and nHeight parameter are set to zero.</param>
        /// <param name="nHeight">[in] Specifies the height, in device units, of the window. For overlapped windows, nHeight is the window's height, in screen coordinates. If the nWidth parameter is set to CW_USEDEFAULT, the system ignores nHeight.</param>
        /// <param name="hWndParent">[in] Handle to the parent or owner window of the window being created. To create a child window or an owned window, supply a valid window handle. This parameter is optional for pop-up windows.</param>
        /// <param name="hMenu">[in] Handle to a menu, or specifies a child-window identifier, depending on the window style. For an overlapped or pop-up window, hMenu identifies the menu to be used with the window; it can be NULL if the class menu is to be used. For a child window, hMenu specifies the child-window identifier, an integer value used by a dialog box control to notify its parent about events. The application determines the child-window identifier; it must be unique for all child windows with the same parent window.</param>
        /// <param name="hInstance">[in] Handle to the instance of the module to be associated with the window.</param>
        /// <param name="lpParam">[in]  Pointer to a value to be passed to the window through the CREATESTRUCT structure (lpCreateParams member) pointed to by the lParam param of the WM_CREATE message. This message is sent to the created window by this function before it returns.
        /// If an application calls CreateWindow to create a MDI client window, lpParam should point to a CLIENTCREATESTRUCT structure. If an MDI client window calls CreateWindow to create an MDI child window, lpParam should point to a MDICREATESTRUCT structure. lpParam may be NULL if no additional data is needed.</param>
        /// <returns>If the function succeeds, the return value is a handle to the new window.
        /// If the function fails, the return value is NULL. To get extended error information, call GetLastError.</returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        internal static extern IntPtr CreateWindowEx(int dwExStyle, string lpClassName, string lpWindowName, uint dwStyle, int x, int y, int nWidth, int nHeight, IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);

        /// <summary>
        /// The ScreenToClient function converts the screen coordinates of a specified point on the screen to client-area coordinates.
        /// </summary>
        /// <param name="hWnd">[in] Handle to the window whose client area will be used for the conversion.</param>
        /// <param name="lpPoint">[in] Pointer to a POINT structure that specifies the screen coordinates to be converted.</param>
        /// <returns>If the function succeeds, the return value is nonzero.
        /// If the function fails, the return value is zero.</returns>
        [DllImport("user32.dll")]
        internal static extern bool ScreenToClient(IntPtr hWnd, ref POINT lpPoint);

        /// <summary>
        /// The SetParent function changes the parent window of the specified child window.
        /// </summary>
        /// <param name="hWndChild">[in] Handle to the child window.</param>
        /// <param name="hWndNewParent">[in] Handle to the new parent window. If this parameter is NULL, the desktop window becomes the new parent window. Windows 2000/XP: If this parameter is HWND_MESSAGE, the child window becomes a message-only window.</param>
        /// <returns>If the function succeeds, the return value is a handle to the previous parent window.
        /// If the function fails, the return value is NULL. To get extended error information, call GetLastError.</returns>
        [DllImport("user32.dll")]
        internal static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        /// <summary>
        /// Retrieves the window object whose class name and window name match the specified strings.
        /// </summary>
        /// <param name="hwndParent">Handle to the parent window whose child windows are to be searched.</param>
        /// <param name="hwndChildAfter">Handle to a child window. The search begins with the next child window in the Z order. The child window must be a direct child window of hwndParent, not just a descendant window.</param>
        /// <param name="lpszClass">Pointer to a null-terminated string that specifies the class name or a class atom created by a previous call to the RegisterClass or RegisterClassEx.</param>
        /// <param name="lpszWindow">Pointer to a null-terminated string that specifies the window name (the window's title). If this parameter is NULL, all window names match.</param>
        /// <returns>If the function succeeds, the return value is a pointer to the window object having the specified class and window names. If the function fails, the return value is NULL.</returns>
        [DllImport("user32.dll", SetLastError = true)]
        internal static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        /// <summary>
        /// Loads a string resource from the executable file associated with a specified module, copies the string into a buffer, and appends a terminating null character.
        /// </summary>
        /// <param name="hInstance">A handle to an instance of the module whose executable file contains the string resource. To get the handle to the application itself, call the <see cref="Kernel32.GetModuleHandle"/> function with NULL.</param>
        /// <param name="uID">The identifier of the string to be loaded.</param>
        /// <param name="lpBuffer">The buffer is to receive the string.</param>
        /// <param name="nBufferMax">The size of the buffer, in characters. The string is truncated and null-terminated if it is longer than the number of characters specified. If this parameter is 0, then lpBuffer receives a read-only pointer to the resource itself.</param>
        /// <returns>If the function succeeds, the return value is the number of characters copied into the buffer, not including the terminating null character, or zero if the string resource does not exist.</returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        internal static extern int LoadString(IntPtr hInstance, int uID, out IntPtr lpBuffer, int nBufferMax);

        /// <summary>
        /// Retrieves the system's dialog base units, which are the average width and height of characters in the system font. For dialog boxes that use the system font,
        /// you can use these values to convert between dialog template units, as specified in dialog box templates, and pixels. For dialog boxes that do not use the system font,
        /// the conversion from dialog template units to pixels depends on the font used by the dialog box.
        /// </summary>
        /// <returns>The function returns the dialog base units. The low-order word of the return value contains the horizontal dialog box base unit,
        /// and the high-order word contains the vertical dialog box base unit.</returns>
        [DllImport("user32.dll")]
        internal static extern uint GetDialogBaseUnits();

        /// <summary>
        /// Loads the specified cursor resource from the executable (.EXE) file associated with an application instance.
        /// </summary>
        /// <param name="hInstance">A handle to an instance of the module whose executable file contains the cursor to be loaded.</param>
        /// <param name="lpCursorName">The name of the cursor resource to be loaded. Alternatively, this parameter can consist of the resource identifier in the low-order word and zero in the high-order word.</param>
        [DllImport("user32.dll")]
        internal static extern IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);

        /// <summary>
        /// This function draws a frame control of the specified type and style.
        /// </summary>
        /// <param name="hDC">Handle to the device context of the window in which to draw the control.</param>
        /// <param name="rect">Long pointer to a RECT structure that contains the logical coordinates of the bounding rectangle for frame control.</param>
        /// <param name="type">Specifies the type of frame control to draw.</param>
        /// <param name="state">Specifies the initial state of the frame control.</param>
        /// <returns>Nonzero indicates success. Zero indicates failure.</returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        internal static extern bool DrawFrameControl(HandleRef hDC, ref RECT rect, int type, int state);

        /// <summary>
        /// The current DPI_AWARENESS_CONTEXT for the thread.
        /// </summary>
        /// <returns>This method will return the latest DPI_AWARENESS_CONTEXT sent to SetThreadDpiAwarenessContext. If SetThreadDpiAwarenessContext was never called for this thread, then the return value will equal the default DPI_AWARENESS_CONTEXT for the process.</returns>
        [DllImport("user32.dll")]
        internal static extern IntPtr GetThreadDpiAwarenessContext();

        /// <summary>
        /// Determines whether two DPI_AWARENESS_CONTEXT values are identical.
        /// </summary>
        /// <param name="dpiContextA">The first value to compare.</param>
        /// <param name="dpiContextB">The second value to compare.</param>
        /// <returns>Returns TRUE if the values are equal, otherwise FALSE.</returns>
        /// <remarks>A DPI_AWARENESS_CONTEXT contains multiple pieces of information. For example, it includes both the current and the inherited DPI_AWARENESS values.
        /// AreDpiAwarenessContextsEqual ignores informational flags and determines if the values are equal.
        /// You can't use a direct bitwise comparison because of these informational flags.</remarks>
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool AreDpiAwarenessContextsEqual(IntPtr dpiContextA, IntPtr dpiContextB);

        /// <summary>
        /// The MonitorFromWindow function retrieves a handle to the display monitor that has the largest area of intersection with the bounding rectangle of a specified window.
        /// </summary>
        /// <param name="hwnd">A handle to the window of interest.</param>
        /// <param name="flags">Determines the function's return value if the window does not intersect any display monitor.
        /// This parameter can be one of the following values. MONITOR_DEFAULTTONULL (0)/MONITOR_DEFAULTTOPRIMARY (1)/MONITOR_DEFAULTTONEAREST (2).</param>
        /// <returns>If the point is contained by a display monitor, the return value is an HMONITOR handle to that display monitor.
        /// If the point is not contained by a display monitor, the return value depends on the value of dwFlags.</returns>
        [DllImport("user32.dll")]
        internal static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint flags);

        /// <summary>
        /// The WindowFromDC function returns a handle to the window associated with the specified display device context (DC). Output functions that use the specified device context draw into this window.
        /// </summary>
        /// <param name="hDC">Handle to the device context from which a handle to the associated window is to be retrieved.</param>
        /// <returns>The return value is a handle to the window associated with the specified DC. If no window is associated with the specified DC, the return value is NULL.</returns>
        [DllImport("user32.dll")]
        internal static extern IntPtr WindowFromDC(IntPtr hDC);

        /// <summary>
        /// Returns the dots per inch (dpi) value for the specified window.
        /// </summary>
        /// <param name="hwnd">The window that you want to get information about.</param>
        /// <returns>The DPI for the window, which depends on the DPI_AWARENESS of the window. See the Remarks section for more information. An invalid hwnd value will result in a return value of 0.</returns>
        [DllImport("user32.dll")]
        internal static extern uint GetDpiForWindow(IntPtr hwnd);

        #endregion
    }
}
