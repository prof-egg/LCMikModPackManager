Stuff

# Changelog

## 2.4.0
- Changed the fundamental way the mod manager works, moving/deleting files are now managed by a new system that:
  - Keeps track of individual files that belong to each mod
  - Keeps track of other information like mod name, version, develeoper etc.
  - Manages its own set of dll files
  - Manages a .lcmd save file
- Added a new menu to let you see what mods you have installed
- Added a new menu to let you see what mods are dependencies and how many references each mod has
Note: With this and the new system from 2.3.0, more dynamic utility can be created

## 2.3.1
- Removed bug setting default to 5 minutes (idk why this broke the client it was working fine in the vscode terminal)

## 2.3.0
- Added change log
- Improved download speed by introducing a dependency manager that prevents the client from downloading a single dependency multiple times
- Default wait time for download is now 5 minutes, if a download takes more than 5 minutes the client will throw an exception
- Paths now show up with all "\\" instead of a mixture of "/" and "\\"
- Added Mik directories to lethal company install to handle downloads, future .modd files, and the .dependencies file
- Downloads no longer use the windows downloads folder, they instead use the "Lethal Company/MikModManager/downloads" folder

## 2.2.1
- Client now handles mods with a "patchers" or "config" folder in the root.

## 2.2.0
- Undocumented

## 2.1.0
- Undocumented

## 2.0.0 Beta
- Undocumented

## 1.1.0
- Undocumented

## 1.0.0
- Undocumented

## Test Release
- Undocumented
