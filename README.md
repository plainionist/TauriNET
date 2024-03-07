# Tauri + Typescript + C#

This template should help get you started developing with Tauri, .NET and TypeScript in Vite. 

Latest working example: https://github.com/RubenPX/TauriNET/releases/download/0.1.2/build.zip

## TODOS
- [X] Call C# code from Tauri
- [X] Call C# code from tauri with parameters and return string
- [ ] Allow comunication betwen JSON data
- [ ] Call Tauri code from C#
- [ ] Hide exposed dlls to avoid deobfuscation ([Embed files](https://tauri.app/v1/guides/building/resources/))

## How it works

You can replace frontend as you want

To invoke NetHost code, you need install tauri API and run this code

```javascript
import { invoke } from "@tauri-apps/api/tauri"

let name = "";
let greetMsg = ""

async function greet(){
  // Learn more about Tauri commands at https://tauri.app/v1/guides/features/command
  greetMsg = await invoke("greet", { name })
}
```

## References

- [Tauri](https://tauri.app)
- [netcorehost](https://github.com/OpenByteDev/netcorehost)
