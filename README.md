# PathManager
A universal application to patch in memory or in a file

This application arises from the need, when doing the crack of an application, not to have to program a crack (a permanent change on a file) or a path (a change in memory, after running the program).
Once the crack is done, you need to write a script.

Contrary to the patches of the crack that can be found on the net, Patch

The script, containing the pseudo instructions for the applications, patch_list.txt is in the executable folder

The application will read the instructions and will correct the patch in memory or on file.
Virtually it could only be used as a Loader, in fact if every item (program) puts only the pseudo "exec" instruction the application will launch the selected program.

Patch Manager has 2 tabs, the first for the user who trivially selects the program, works it and uses it, the second is referred to the crack developers, here you will find a naked script generator, which will capture the icon to show in the first tab (the one for the user), plus another wizard will generate the script for the verification (path, length and sha256) of the file before it is modified.
There are nice features like the paths converted with the environment variables.
This set of instructions is weak, but little by little, we can expand it according to your needs.

I am looking for people of good will who like the project to expand it and who knows ... make it a standard in Patch and Crack.

This is the instruction set

Title / EndTitle
For init a new title you will include between Title and EndTitle the patching information.

Title = Application name e version (combobox caption)
EndTitle
es:
Title = "ENTER HERE THE NAME OF APPLICATION AND THE VERSION"
...
EndTitle

Icon
Bitmap of executable (better if you make generate from wizard)
es:
Icon = "89504E470D0A1A0A0000000D494844520000000F000000100806000000C9562504000000017352474200AECE1CE90000000467414D410000B18F0BFC6105000000097048597300000EC300000EC301C76FA8640000030349444154384F8D935D6893671886DFD50D4FA61D382CD38922431C53B603873F5B1144B4E2F040714376E240F4C0A382B24EC16CCAD6DA1A6B758DF9696D7E4C7F5C4D8C355F4CD3358D49DB2FF9DAD89A665B935A5434A92236287175AEB9FC1202733BDA0DCFC9CB7B3DF7FDBC3CAFC80B7863B0FFC637B21C2A9765797EE1F0FFCAE572951D3F6167F3176D1C3ADA45E7153F4391FED991A89C1E1D09878714D93634249F5594817D8A226F0987C32B8AA810D79DCE255FEE69462C7420DE1F50EB0E2BB7BE24791F82A3E009BFA0C3FF94ABC12994D8146EF7B5871A8DA6A40077BBDDAB3FFA448778B703B1388058FE07252B33DC4D43E21E4CA4E0EE23484FC348324B7B7BEB9D0298578FD773F0B3CD66C47C1B62910FB16C54759FA22F92633006E1DF4149C0CD4935C9C863CCE696BE222A44AFD777F9F34D16967D682DBAFB114B7EA3DAF8272E7F8EAE20B807410A437FF43ECDCDCD8D455408EF35CFC4C7EB4C7CBAB115517A419DDD8978AF8FED7B6FD368CDA0B33F47DFF1178DF61CC1C1099A9A9A761451217E6977663F58A563F51A1325A5F9D9ED88B2AB2C5F1FE1D08F0F385CFD88AABA692A7F7A4620A860341ACB8BA8104D8636CA969E61C1622D6FBDA355DDF56A8336DE5EEA66D7FE187B0EC639A23151597998B30D0DE8F5FAB54554086B4B2BDF56395850568710DF23E6D420E6E513B4B26DB75B7D200B333333CCCECE92C964B0D92EE6DDB716E0133F688E35D4D7A6DCAE4B0C04AFD3E3EB416FECA5BCE20AD5D575E472B942E5C1AF2C1B08DD8C70EA94F66501FEAF2A2A2A366A6B6B9F9C3B7D92EEEE6EA69F4DD37B2BC8399F969F2503EA862149929AF235A95BF3A6D96CB67BBD5E3A3B3BD1E9CE333C3C4C36FB9C9DE6ED5CFAD5C10153150EAF84FA07FE0D7B3C9EEF7C3E1FA15088B1B131A2D128175A5A48A7D3EC377D8DA1CBC4F8F8388944028B55DD89D765B15856E43BE6DD92C924F1781CAB7A29168B73DED9568052A91493939368B55A7F11FB470683A13410083862B1D80B15CE298AF2777D7D7DD66AB3118944B868B7535353234B9234F715A5B5166CEB04FB510000000049454E44AE426082"

Verify
Verify i the file in Path, have same size and same SHA256 (Build this command in the Deply Tab)
Verify = "Application path", "Length", "SHA256"
es:
Verify = "%programfiles(x86)%\Mazaika\mz004.exe", "2342344", "98C638F9F8709AC49A379DC755672E12F575F33448C6D2009713F302B5B4EA80"

OpenFile
Open the verifyed file and choice to make backup
OpenFile = "noBackup"|"Backup"
es:
OpenFile = "Backup"

Write
Write byte(s) to file Opened
Write = Offset (hex), Original Byte (hex), Patched Byte (hex)
es:
Write = 820D2b, "7F 0C", "90 90"

CloseFile
Close a opened file
es:
CloseFile

GoAdmin
Verify and ask, if the pocess run in user mode to go in admin mode (for continue the script to run in admin mode)
es:
GoAdmin
*Explore
Open file verified location
es:
Explore

MessageBox
Message Popup with information (intallation guide or serial, etc.)
Messagebox = "Text", "TextSelectable"
es:
MessageBox = "Click File->Enter serial.<NEWLINE> and insert:","1234567890"
 
Note: write <NEWLINE> where you want insert a new line

Author
Write into log the Author name
es:
Author = "Ciccio Pasticcio"

Pause
Wait a key press to continue
Pause = "message"
es:
Pause = "Continue... (press any key)"
 
Note if message filed is empty the default message is: "Press any key to contunue"

RegDeleteValue
Delete a value into registry
RegDeleteValue = "CU" | "LM", RegistryKey, Value
CU = Current User
LM = Local Machine (if you use this value, run the application in administrator mode)
es: 
RegDeleteValue="CU","SOFTWARE\ASProtect\SpecData","B05A6C3D29FFAF52"

Exec
Execute the verify application, with the eventually arguments
Exec "Arguments"
es:
Exec = ""
Exec = "/a /s"

Log
Write in the report info from the pathing operation
Log = "Verbose" | "OnError" ! "None"
Verbose = write all info
OnError = write only if failure command
None = write only Print command

Print
Print a string for the user
Print = "string"
es:
Print = "Helo world"

Wait
Wait a time before contunue procedure
Wait = in millisecond
es:
Wait = "1000"

Patch
Patch Memory
Patch = Offset (hex), Original Byte (hex), Patched Byte (hex)
es:
Patch = 820D2b, "7F 0C", "90 90"

Exit
Exit PatchManager
es:
exit
