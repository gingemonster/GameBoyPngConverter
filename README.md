
# GameBoyPngConverter  
A utility, written in .net core, for converting 4 color png images to C files for use in the GameBoy Developer Kit (GBDK) when displaying full screen images, like a splash screen.

# Creating suitable images
Images should be pre-processed and saved as .png images with the following specifications:

 1. Image width and height should be a multiple of 8 as each sprite on the gameboy is 8x8 pixels
 2. Images should be converted into black and white and contain only 4 colors
 3. Typically images should be 160x144 pixels which is the size of the GameBoy screen

The following can be achieved in Photoshop or Gimp by resizing/cropping, de-saturating then converting to index colors with 4 colors in the palette.

# Generating C files from your image
The simplest way to get started is to download the latest release for your operating system from [https://github.com/gingemonster/GameBoyPngConverter/releases](https://github.com/gingemonster/GameBoyPngConverter/releases). Once you have all the files downloaded and unzipped follow these instructions:

## Windows
You can either run the utility from the command prompt like this:

> GameBoyPngConverter.exe "path to my png file"

Or just drag and drop a png image onto the GameBoyPngConverter.exe

## Linux
You need to download and unzip the files then open a terminal in the unzipped directory, then:

 1. Change the permissions on the GameBoyPngConverter file to make them executable **chmod 777 ./GameBoyPngConverter**
 2. Run the utility like this **./GameBoyPngConverter "path to my png file** where you replace **path to my png file** with the full path to the file you want to convert.

## Mac / OSX
You need to download and unzip the files then open a terminal in the unzipped directory, then:

 1. Change the permissions on the GameBoyPngConverter file to make them executable 
**sudo chmod +x GameBoyPngConverter** and hit enter.
 2. Type in the password of your admin account and hit enter to grant the executable permission to run
 3. Now type in open **GameBoyPngConverter "path to my png file"** where you replace **path to my png file** with the full path to the file you want to convert, and hit enter.

# Using the generated files
Two files should be created in the same directory as your png named **mypngname_data.c** and **mypngname_map.c**. Copy both of these files to the project directory for your GameBoy game then use code similar to below to include in your game and display:

    #include <gb/gb.h>
    #include "mario_data.c"
    #include "mario_map.c"
    void main(){
	    set_bkg_data(0, 114, mario_data);
	    set_bkg_tiles(0, 0, 20, 18, mario_map);
	    SHOW_BKG;
	    DISPLAY_ON;
    }

Replacing **mario** with the name of your file and changing **114** to be the number of tiles generated for your image as shown in the comment at the top of your **myimage_data.c** file. IF your image is smaller than the full screen gameboy size **160x144** then you may also need to alter **20** and **18** in the **set_bkg_tiles** line to match your images width and height in 8x8 sprites.