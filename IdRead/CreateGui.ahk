MyGui := Gui()
MyGui.Add("Edit", "r10 w500")
MyGui.Show()
ControlSend "This is a line of text in the edit control.{Enter}", "Edit1", MyGui
ControlSendText "Notice that {Enter} is not sent as an Enter keystroke with ControlSendText.", "Edit1", MyGui
