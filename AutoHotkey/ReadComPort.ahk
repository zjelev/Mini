data := readCOM()

readCOM(){
	COM_Port     = COM3
	COM_Baud     = 9600
	COM_Parity   = N
	COM_Data     = 8
	COM_Stop     = 1
	
	COM_Settings = %COM_Port%:baud=%COM_Baud% parity=%COM_Parity% data=%COM_Data% stop=%COM_Stop% dtr=Off
	COM_Handle := Serial_Initialize(COM_Settings)
	Read_Data := Serial_Read(COM_Handle, "0xFF")
	
	;TimeOut :=0
	loop { ;loop until data is read
		Sleep,500
		Read_Data := Serial_Read(COM_Handle, "0xFF")
		if (Read_Data){
			Read_Data:=Hex2ASCII(Read_Data)
			Read_Data:= RegExReplace(Read_Data, "[^0-9]") ;leve only numbers
			
		}
	} until (Read_Data) 
	
	Serial_Close(COM_Handle)
	;MsgBox, % Read_Data
	Return, Read_Data
}

;########################################################################
;######    Hex2ASCII              #######################################
;########################################################################

Hex2ASCII(fHexString)
{	Loop Parse, fHexString
	NewHexString .= A_LoopField (Mod(A_Index,2) ? "" : ",")
	Loop Parse, NewHexString, `,
		ConvString .= Chr("0x" A_LoopField)
	Return ConvString
}	

;########################################################################
;###### Convert HEX               #######################################
;########################################################################

hex2str(string)
{
	if !(DllCall("crypt32\CryptStringToBinary", "ptr", &string, "uint", 0, "uint", 0x4, "ptr", 0, "uint*", size, "ptr", 0, "ptr", 0))
		throw Exception("CryptStringToBinary failed", -1)
	VarSetCapacity(buf, size, 0)
	if !(DllCall("crypt32\CryptStringToBinary", "ptr", &string, "uint", 0, "uint", 0x4, "ptr", &buf, "uint*", size, "ptr", 0, "ptr", 0))
		throw Exception("CryptStringToBinary failed", -1)
	return StrGet(&buf, size, "UTF-8")
}

;########################################################################
;###### Initialize COM Subroutine #######################################
;########################################################################
Serial_Initialize(COM_Settings)
{
  ;Global COM_FileHandle      ;uncomment this if there is a problem

  ;###### Build COM DCB ######
  ;Creates the structure that contains the COM Port number, baud rate,...
  VarSetCapacity(DCB, 28)
  BCD_Result := DllCall("BuildCommDCB"
       ,"str" , COM_Settings ;lpDef
       ,"UInt", &DCB)        ;lpDCB
  If (BCD_Result <> 1)
  {
    error := DllCall("GetLastError")
    MsgBox, There is a problem with Serial Port communication. `nFailed Dll BuildCommDCB, BCD_Result=%BCD_Result% `nLasterror=%error%`nThe Script Will Now Exit.
    ExitApp
  }

  ;###### Extract/Format the COM Port Number ######
  StringSplit, COM_Port_Temp, COM_Settings, `:
  COM_Port_Temp1_Len := StrLen(COM_Port_Temp1)  ;For COM Ports > 9 \\.\ needs to prepended to the COM Port name.
  If (COM_Port_Temp1_Len > 4)                   ;So the valid names are
    COM_Port = \\.\%COM_Port_Temp1%             ; ... COM8  COM9   \\.\COM10  \\.\COM11  \\.\COM12 and so on...
  Else                                          ;
    COM_Port = %COM_Port_Temp1%
  ;MsgBox, COM_Port=%COM_Port%

  ;###### Create COM File ######
  ;Creates the COM Port File Handle
  ;StringLeft, COM_Port, COM_Settings, 4  ; 7/23/08 This line is replaced by the "Extract/Format the COM Port Number" section above.
  COM_FileHandle := DllCall("CreateFile"
       ,"Str" , COM_Port     ;File Name
       ,"UInt", 0xC0000000   ;Desired Access
       ,"UInt", 3            ;Safe Mode
       ,"UInt", 0            ;Security Attributes
       ,"UInt", 3            ;Creation Disposition
       ,"UInt", 0            ;Flags And Attributes
       ,"UInt", 0            ;Template File
       ,"Cdecl Int")
  If (COM_FileHandle < 1)
  {
    error := DllCall("GetLastError")
    MsgBox, There is a problem with Serial Port communication. `nFailed Dll CreateFile, COM_FileHandle=%COM_FileHandle% `nLasterror=%error%`nThe Script Will Now Exit.
    ExitApp
  }

  ;###### Set COM State ######
  ;Sets the COM Port number, baud rate,...
  SCS_Result := DllCall("SetCommState"
       ,"UInt", COM_FileHandle ;File Handle
       ,"UInt", &DCB)          ;Pointer to DCB structure
  If (SCS_Result <> 1)
  {
    error := DllCall("GetLastError")
    MsgBox, There is a problem with Serial Port communication. `nFailed Dll SetCommState, SCS_Result=%SCS_Result% `nLasterror=%error%`nThe Script Will Now Exit.
    Serial_Close(COM_FileHandle)
    ExitApp
  }

  ;###### Create the SetCommTimeouts Structure ######
  ReadIntervalTimeout        = 0xffffffff
  ReadTotalTimeoutMultiplier = 0x00000000
  ReadTotalTimeoutConstant   = 0x00000000
  WriteTotalTimeoutMultiplier= 0x00000000
  WriteTotalTimeoutConstant  = 0x00000000

  VarSetCapacity(Data, 20, 0) ; 5 * sizeof(DWORD)
  NumPut(ReadIntervalTimeout,         Data,  0, "UInt")
  NumPut(ReadTotalTimeoutMultiplier,  Data,  4, "UInt")
  NumPut(ReadTotalTimeoutConstant,    Data,  8, "UInt")
  NumPut(WriteTotalTimeoutMultiplier, Data, 12, "UInt")
  NumPut(WriteTotalTimeoutConstant,   Data, 16, "UInt")

  ;###### Set the COM Timeouts ######
  SCT_result := DllCall("SetCommTimeouts"
     ,"UInt", COM_FileHandle ;File Handle
     ,"UInt", &Data)         ;Pointer to the data structure
  If (SCT_result <> 1)
  {
    error := DllCall("GetLastError")
    MsgBox, There is a problem with Serial Port communication. `nFailed Dll SetCommState, SCT_result=%SCT_result% `nLasterror=%error%`nThe Script Will Now Exit.
    Serial_Close(COM_FileHandle)
    ExitApp
  }

  Return COM_FileHandle
}

;########################################################################
;###### Close COM Subroutine ############################################
;########################################################################
Serial_Close(COM_FileHandle)
{
  ;###### Close the COM File ######
  CH_result := DllCall("CloseHandle", "UInt", COM_FileHandle)
  If (CH_result <> 1)
    MsgBox, Failed Dll CloseHandle CH_result=%CH_result%

  Return
}

;########################################################################
;###### Write to COM Subroutines ########################################
;########################################################################
Serial_Write(COM_FileHandle, Message)
{
  ;Global COM_FileHandle

  SetFormat, Integer, DEC

  ;Parse the Message. Byte0 is the number of bytes in the array.
  StringSplit, Byte, Message, `,
  Data_Length := Byte0
  ;msgbox, Data_Length=%Data_Length% b1=%Byte1% b2=%Byte2% b3=%Byte3% b4=%Byte4%

  ;Set the Data buffer size, prefill with 0xFF.
  VarSetCapacity(Data, Byte0, 0xFF)

  ;Write the Message into the Data buffer
  i=1
  Loop %Byte0%
  {
    NumPut(Byte%i%, Data, (i-1) , "UChar")
    ;msgbox, %i%
    i++
  }
  ;msgbox, Data string=%Data%

  ;###### Write the data to the COM Port ######
  WF_Result := DllCall("WriteFile"
       ,"UInt" , COM_FileHandle ;File Handle
       ,"UInt" , &Data          ;Pointer to string to send
       ,"UInt" , Data_Length    ;Data Length
       ,"UInt*", Bytes_Sent     ;Returns pointer to num bytes sent
       ,"Int"  , "NULL")
  If (WF_Result <> 1 or Bytes_Sent <> Data_Length)
    MsgBox, Failed Dll WriteFile to COM Port, result=%WF_Result% `nData Length=%Data_Length% `nBytes_Sent=%Bytes_Sent%
    
    Return Bytes_Sent
}

;########################################################################
;###### Read from COM Subroutines #######################################
;########################################################################
Serial_Read(COM_FileHandle, Num_Bytes, byref Bytes_Received = "")
{
  ;Global COM_FileHandle
  ;Global COM_Port
  ;Global Bytes_Received
	SetFormat, Integer, HEX
	
  ;Set the Data buffer size, prefill with 0x55 = ASCII character "U"
  ;VarSetCapacity won't assign anything less than 3 bytes. Meaning: If you
  ;  tell it you want 1 or 2 byte size variable it will give you 3.
	Data_Length  := VarSetCapacity(Data, Num_Bytes, 0x55)
  ;msgbox, Data_Length=%Data_Length%
	
	
  ;###### Read the data from the COM Port ######
  ;msgbox, COM_FileHandle=%COM_FileHandle% `nNum_Bytes=%Num_Bytes%
	Read_Result := DllCall("ReadFile"
       ,"UInt" , COM_FileHandle   ; hFile
       ,"Str"  , Data             ; lpBuffer
       ,"Int"  , Num_Bytes        ; nNumberOfBytesToRead
       ,"UInt*", Bytes_Received   ; lpNumberOfBytesReceived
       ,"Int"  , 0)               ; lpOverlapped
  ;MsgBox, Read_Result=%Read_Result% `nBR=%Bytes_Received% ,`nData=%Data%
	If (Read_Result <> 1)
	{
		MsgBox, There is a problem with Serial Port communication. `nFailed Dll ReadFile on COM Port, result=%Read_Result% - The Script Will Now Exit.
		Serial_Close(COM_FileHandle)
		Exit
	}
	
  ;###### Format the received data ######
  ;This loop is necessary because AHK doesn't handle NULL (0x00) characters very nicely.
  ;Quote from AHK documentation under DllCall:
  ;     "Any binary zero stored in a variable by a function will hide all data to the right
  ;     of the zero; that is, such data cannot be accessed or changed by most commands and
  ;     functions. However, such data can be manipulated by the address and dereference operators
  ;     (& and *), as well as DllCall itself."
	i = 0
	Data_HEX =
	Loop %Bytes_Received%
	{
    ;First byte into the Rx FIFO ends up at position 0
		
		Data_HEX_Temp := NumGet(Data, i, "UChar") ;Convert to HEX byte-by-byte
		StringTrimLeft, Data_HEX_Temp, Data_HEX_Temp, 2 ;Remove the 0x (added by the above line) from the front
		
    ;If there is only 1 character then add the leading "0'
		Length := StrLen(Data_HEX_Temp)
		If (Length =1)
			Data_HEX_Temp = 0%Data_HEX_Temp%
		
		i++
		
    ;Put it all together
		Data_HEX .= Data_HEX_Temp
	}
  ;MsgBox, Read_Result=%Read_Result% `nBR=%Bytes_Received% ,`nData_HEX=%Data_HEX%
	
	SetFormat, Integer, DEC
	Data := Data_HEX
	
	Return Data
}