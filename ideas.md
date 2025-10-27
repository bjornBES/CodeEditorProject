# Ideas

this is just an idea dump

## Start up

when the Editor is started it

1. Loads extensions (finds them and loads there .json into memory)
2. Builds the extensions (runs dotnet build or something like that)
3. make the process (with dotnet run --no-build)
4. writes something to the stdin once done with init
   1. note this will run check for any missing dependent and find them (on the extension side)
5. show the window
