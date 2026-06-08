namespace GenesisEmu.Frontend.Windows
{
    public static class GenesisInputKeyNames
    {
        public static readonly string[] PadButtons =
        {
            "A", "B", "C", "Start", "Up", "Down", "Left", "Right", "X", "Y", "Z", "Mode",
        };

        private static readonly string[] s_keyNames =
        {
            "","Escape","D1","D2", "D3","D4","D5","D6",
            "D7","D8","D9","D0", "Minus","Equals","Back","Tab",
            "Q","W","E","R", "T","Y","U","I",
            "O","P","LeftBracket","RightBracket", "Return","LeftControl","A","S",
            "D","F","G","H", "J","K","L","Semicolon",
            "Apostrophe","Grave","LeftShift","Backslash", "Z","X","C","V",
            "B","N","M","Comma", "Period","Slash","RightShift","Multiply",
            "LeftAlt","Space","Capital","F1", "F2","F3","F4","F5",
            "F6","F7","F8","F9", "F10","NumberLock","ScrollLock","NumberPad7",
            "NumberPad8","NumberPad9","Subtract","NumberPad4", "NumberPad5","NumberPad6","Add","NumberPad1",
            "NumberPad2","NumberPad3","NumberPad0","Decimal", "","","Oem102","F11",
            "F12","","","", "","","","",
            "","","","", "F13","F14","F15","",
            "","","","", "","","","",
            "Kana","","","AbntC1", "","","","",
            "","Convert","","NoConvert", "","Yen","AbntC2","",
            "","","","", "","","","",
            "","","","", "","NumberPadEquals","","",
            "PreviousTrack","AT","Colon","Underline", "Kanji","Stop","AX","Unlabeled",
            "","NextTrack","","", "NumberPadEnter","RightControl","","",
            "Mute","Calculator","PlayPause","", "MediaStop","","","",
            "","","","", "","","VolumeDown","",
            "VolumeUp","","WebHome","NumberPadComma", "","Divide","","PrintScreen",
            "RightAlt","","","", "","","","",
            "","","","", "","Pause","","Home",
            "Up","PageUp","","Left", "","Right","","End",
            "Down","PageDown","Insert","Delete", "","","","",
            "","","","LeftWindowsKey", "RightWindowsKey","Applications","Power","Sleep",
            "","","","Wake", "","WebSearch","WebFavorites","WebRefresh",
            "WebStop","WebForward","WebBack","MyComputer", "Mail","MediaSelect","","",
            "","","","", "","","","",
            "","","","", "","","",""
        };

        private static readonly string[] s_joystickNames =
        {
            "Button 0", "Button 1", "Button 2", "Button 3",
            "Button 4", "Button 5", "Button 6", "Button 7",
            "Button 8", "Button 9", "Button 10", "Button 11",
            "Button 12", "Button 13", "Button 14", "Button 15",
            "Button 16", "Button 17", "Button 18", "Button 19",
            "Button 20", "Button 21", "Button 22", "Button 23",
            "Button 24", "Button 25", "Button 26", "Button 27",
            "Button 28", "Button 29", "Button 30", "Button 31",
            "Point 0 Up", "Point 0 Down", "Point 0 Left", "Point 0 Right",
            "Point 1 Up", "Point 1 Down", "Point 1 Left", "Point 1 Right",
            "Rot 0", "Rot 1", "Rot 2", "Rot 3",
            "XYZ 0", "XYZ 1", "XYZ 2", "XYZ 3",
            "XYZ 4", "XYZ 5",
        };

        public static string GetKeyName(int in_keyNo)
        {
            if (in_keyNo < 0 || in_keyNo >= s_keyNames.Length) return "";
            return s_keyNames[in_keyNo];
        }

        public static string GetJoystickName(int in_buttonNo)
        {
            if (in_buttonNo < 0 || in_buttonNo >= s_joystickNames.Length) return "";
            return s_joystickNames[in_buttonNo];
        }
    }
}
