# Summary
 
MD Tracer allows you to enjoy SEGA MegaDrive (Genesis) games just like a typical emulator, but its main purpose is as a tracer that tracks the internal processing of the MegaDrive hardware and software.
 
All processing (MC68000, Z80, VDP, YM2612, PSG) is coded in VisualStadio's c# language and .net, making it easy to understand the internal processing of the hardware.
 
The software also displays MC68000 programs in disassembled form, allowing you to stop them at any point and check the values ​​of memory, registers, etc. while stepping through the program.
 
Enjoy the amazing technology and wisdom of the engineers of the time from both the hardware and software perspectives.
 
Note: This program is not intended for playing games illegally. Its purpose is to help users understand and appreciate the ingenuity of the engineers who created this remarkable technology.
 
![demo1](https://www.jppass.jp/mdtracer/wp-content/uploads/2025/01/demo1.png)

 
## Features
 
* This program provides the following features
 * Draws a virtual screen and allows you to check the contents
 * Displays the program in assembler and allows you to stop it at any point
 * Displays the values ​​of the MC68000 and VDP registers
 * Displays a function relationship diagram and allows you to know where it stopped
 * Searches for the places where the MC68000 accesses the VDP or Z80
 * Can stop at the point where specified memory is referenced/updated

* This program is entirely written in C# and .NET.
* It can be compiled using Visual Studio 2022.
![demo2](https://www.jppass.jp/mdtracer/wp-content/uploads/2025/01/demo2.png)
![demo3](https://www.jppass.jp/mdtracer/wp-content/uploads/2025/01/demo3.png)
![demo4](https://www.jppass.jp/mdtracer/wp-content/uploads/2025/01/demo4.png)
![demo5](https://www.jppass.jp/mdtracer/wp-content/uploads/2025/01/demo5.png)
![demo6](https://www.jppass.jp/mdtracer/wp-content/uploads/2025/01/demo6.png)
![demo7](https://www.jppass.jp/mdtracer/wp-content/uploads/2025/01/demo7.png)
![demo8](https://www.jppass.jp/mdtracer/wp-content/uploads/2025/01/demo8.png)
![demo9](https://www.jppass.jp/mdtracer/wp-content/uploads/2025/01/demo9.png)
![demo10](https://www.jppass.jp/mdtracer/wp-content/uploads/2025/01/demo10.png)
![demo11](https://www.jppass.jp/mdtracer/wp-content/uploads/2025/01/demo11.png)

## page
  https://www.jppass.jp/mdtracer
  
## binary
  https://www.jppass.jp/mdtracer/download/
  
## source
  GitHub: https://github.com/sasayaki-japan/MDTracer/tree/main
  
## Compatibility
This tracer has been confirmed to work only with the following games that I legally own.
  
Verified Titles:
  * JAPAN Version
     * Space Harrier II
     * Super Thunder Blade
     * Altered Beast
     * Phantasy Star
     * Thunder Force II
     * Ghouls'n Ghosts
     * Super Hang-On
     * Super Shinobi
     * Tatsujin
     * Vermilion
     * Golden Axe III
     * Sorcerian
     * After Burner II
     * Phantasy Star III
     * Thunder Force III
     * Phelios
     * Super Monaco GP
     * Hellfire
     * Strider Hiryuu
     * FZ Senki Axis
     * Burning Force
     * Granada
     * DARIUS(Sagaia)
     * Musha Aleste
     * Sonic The Hedgehog
     * Bare Knuckle
     * Kuuga
     * Thunder Force IV
     * Castlevania - Bloodlines
     * Super Fantasy Zone
     * Galaxy Force II
     * Panorama Cotton
     * Ex-Ranza
     * Dynamite Headdy
     * Battle Mania Daiginjou
     * OutRun
     * Gunstar Heroes

  * USA version
     * Vectorman
  
## unimplemented features
  
   * mmc(address 0xa13000-)
   * interlace
   * 32x
   * cd
   * pal
   * sram
  
## References
  
  * Sega Genesis Manual: Genesis Technical Overview v1.00 (1991)(Sega)(US)
  * YM2608 OPNA application manual
  * sn76489 user manual, Texas Instruments
  * MC68000 16-Bit Microprocessor User's Manual, MOTOROLA, 1981
  * Z80 CPU User Manual - Zilog
  
## Thanks
  
I created it with reference to the regular manual, but I referred to the following materials for the parts that are not written in the manual
  
  * Gens
  * BlastEm
  * MDSound
  * Sega Genesis VDP documentation(by Charles MacDonald)
  
## License
  
This project is licensed under the MIT License.
 
Copyright (c) 2019 Stephane Dallongeville

## How to Use MDTracer
## How to Use MDTracer
１．Extract the Executable File Extract the downloaded executable file.  
２．Run MDTracer.exe Run MDTracer.exe from the extracted folder.  
３．Select a Game File  
　　Press the “F” key during execution to open the file explorer.  
　　Select the game file you want to run, and the game will start.  
　　For previously executed game files, press keys “1” to “9” to re-execute them.  
４．Default Keyboard Configuration The default keyboard mapping is as follows:  
　　Start: Space  
　　A Button: N  
　　B Button: M  
　　C Button: ,  
　　Up: W  
　　Down: S  
　　Left: A  
　　Right: D  
　　X Button: H  
　　Y Button: J  
　　Z Button: K  
　　Mode: Return  
５．Input Configuration To configure the input settings, follow these steps:
　1.Select “Control” → “Setting” → “Setting” from the menu to open the Setting View.
  2.Check “I/O” in the “Window” tab to open the I/O View.
  3.Press the key you want to change to open Button Select.
        To configure a joypad, press “Rescan” and select the device from the “Joystick” list.
  4.Press the button on the desired device to complete the key mapping.

６．Menu
■Control
　■Setting　　　　 Display the settings screen
　■Exit　　　　　　Exit
■Capture
　■Screenshot　　　Save a screen capture
　■Video Recording Start video recording. Press again while recording to stop
　■Open Screenshot Folder Open the folder where screen captures are stored
　■Open Video Folder　　　Open the folder where videos are stored
■Emulation
　■Pause / Resume　Pause the game. Press again while paused to resume
　■Reset　　　　　 Reset the game
　■Frame Advance　 Advance the game one frame at a time. Resume with ESC
　■State Capture
　　■Save　　　　　Save the current game state
　　■Load　　　　　Restore the most recently saved game state
　　■List　　　　　Display a list of saved game states
　■Input Capture
　　■Record　　　　Start recording input operations. Press again while recording to stop
　　■Replay　　　　Replay the most recently saved input operations
　　■List　　　　　Display a list of saved input operations

７．Shortcut Key List
　Esc Pause / Resume
　F1 Save State Capture
　F2 Load State Capture
　F3 Display State Capture List
　F4 Open Setting
　F5 Start / Stop Input Capture Recording
　F6 Start / Stop Input Capture Replay
　F7 Display Input Capture List
　F9 Frame Advance
　F10 Save Screenshot
　F11 Start / Stop Video Recording
　F12 Reset

８．Setting View
■Window
　Turn the check ON to open the specified window
■View
　■View　　　　　　Turn the specified display of the game screen ON / OFF
　■Priority High 　Color the dots whose Priority is High in the specified display
　■Priority Low　　Color the dots whose Priority is Low in the specified display

９．VDP Memory View
■Grid 　　　　　　Display an 8×8-dot grid
■Screenshot 　　　When ON, this works together with Screenshot (F10) and saves a capture of this window
■Video Recording　When ON, this works together with Video Recording (F11) and starts recording video of this window

