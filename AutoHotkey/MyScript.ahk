#NoEnv ; Recommended for performance and compatibility with future AutoHotkey releases.
; #Warn  ; Enable warnings to assist with detecting common errors.
SendMode Input ; Recommended for new scripts due to its superior speed and reliability.
SetWorkingDir %A_ScriptDir% ; Ensures a consistent starting directory.

;f1::
;send ^c
;winactivate, ahk_exe chrome.exe
;Send ^!a

;#HotkeyModifierTimeout 100

; #IfWinExist Untitled - Notepad
^q::
    ; MyString := "Hey"
    ; MyNumber := 7
    ; MyString2 := "how are you?"
    ; ; MsgBox, Yes - Notepad untitled is open
    ; MsgBox % MyString . " viewer, " . MyString2 . " => " . Mynumber * 8

    ; arr := ["Alpha", "Beta", "Charlie", 1024]
    ; arr.Push("Delta")

    ; Loop, read, Data.txt
    ; arr.Push(A_LoopReadLine)

    ; for index, element in arr
    ; MsgBox, % element . " is number " . index

    ; Run, %A_ProgramFiles%\AutoHotkey
    ; Run, msedge.exe https://abv.bg
    ; Run, chrome.exe https://abv.bg

    ; InputBox, name, Input name dialogue, Please enter your name, hide, ,200, 200, 0, 0, locale, 5, joey
    ; Switch ErrorLevel
    ; {
    ; case 0:
    ;     MsgBox, Your name is %name%
    ; case 1:
    ;     MsgBox, You clicked cancel
    ; case 2:
    ;     MsgBox, Timeout
    ; }

    ; ;
    ; url := "https://the-internet.herokuapp.com/login"
    ; userName := "tomsmith"
    ; password := "SuperSecretPassword!"
    ; web_browser := ComObjCreate("InternetExplorer.Application")
    ; web_browser.Visible := True
    ; web_browser.Navigate(url)

    ; while web_browser.busy
    ; {
    ;     sleep 500
    ; }
    ; sleep 1000

    ; username_input := web_browser.document.getElementbyID("username")
    ; username_input.value := userName

    ; password_input := web_browser.document.getElementbyID("password")
    ; password_input.value := password

    ; web_browser.document.getElementByID("login").submit()

    ; ;
    ; path := "New WorkSheet.xlsx"
    ex := ComObjCreate("Excel.Application")
    ex.visible := True
    ex.Workbooks.Add
    ; ex.Workbooks.Open(path)
    
return
