./bvm.exe ./data/runtime/interop --exportInterop
./basm.exe -i ./data/runtime/interop -o ./data/runtime/libs --compileInterop
./basm.exe project ./data/compiler/project/CoreProjects/System/project.json
./basm.exe project ./data/compiler/project/CoreProjects/Collections/project.json
./basm.exe project ./data/compiler/project/CoreProjects/Testing/project.json
