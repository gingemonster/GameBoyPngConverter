
# GameBoyPngConverter  
A utility, written in .NET 6, for converting 4 color png images to C files for use in the GameBoy Developer Kit (GBDK) when displaying full screen images, like a splash screen.

If you are starting to learn how to develop you own GameBoy games check out our youtube tutorial series https://www.youtube.com/playlist?list=PLeEj4c2zF7PaFv5MPYhNAkBGrkx4iPGJo

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

 1. You may need to install two dependancies **sudo apt install libc6-dev** and **sudo apt install libgdiplus**
 3. Change the permissions on the GameBoyPngConverter file to make them executable **chmod 777 ./GameBoyPngConverter**
 3. Run the utility like this **./GameBoyPngConverter "path to my png file** where you replace **path to my png file** with the full path to the file you want to convert.
 


## Mac / OSX
You need to download and unzip the files then open a terminal in the unzipped directory, then:

 1. Install a platform specific package for OSX using [https://brew.sh/](https://brew.sh/) by running **brew install mono-libgdiplus**
 2. Change the permissions on the GameBoyPngConverter file to make them executable 
**sudo chmod +x GameBoyPngConverter** and hit enter.
 3. Type in the password of your admin account and hit enter to grant the executable permission to run
 4. Now type in open **./GameBoyPngConverter "path to my png file"** where you replace **path to my png file** with the full path to the file you want to convert, and hit enter.


 # Additional parameters
 The simples way to use this application is to run it with only one parameter containing path to a .png file. For Windows it looks like this:

 > GameBoyPngConverter.exe "path to my png file"

You can also use this addition options:

    -a, --automated        Automated generation process. [default: False]
    -o, --offset <offset>  Set offset of first tile to generate map file. [default:  0]
    -bw, --blackwhite      Generate palette of only two color: transparent and the darkest. [default: False]

And use it as:

 > GameBoyPngConverter.exe "path to my png file" -o 32 -bw

 ## Offset

Offset can be usefull in case when you have loaded into RAM some other tiles for other map and you want to load data from this image after this tiles (ex. to show it on window layer), data for the new map have to be shifted according to number of tiles loaded before.

## Black and white mode

When you generate data and map for file with only 2 or 3 colors, generated tiles with use only the darkest colors. When you set this flag, image with only 2 or 3 colors will use colors from the darkest and as the last one, it will use transparent color.


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
