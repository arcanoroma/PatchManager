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
