#NoEnv  ; Recommended for performance and compatibility with future AutoHotkey releases.
; #Warn  ; Enable warnings to assist with detecting common errors.
SendMode Input  ; Recommended for new scripts due to its superior speed and reliability.
SetWorkingDir %A_ScriptDir%  ; Ensures a consistent starting directory.

; #IfWinExist Untitled - Notepad
^q::
MyString := "Hey"
MyNumber := 7
MyString2 := "how are you?"
; MsgBox, Yes - Notepad untitled is open
MsgBox % MyString . " viewer, " . MyString2 . " => " . Mynumber * 8
return
