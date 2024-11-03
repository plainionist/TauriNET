
cd src-tauri
cargo build
cd ..

cd src-netcore
dotnet build
cd ..

copy src-netcore\TauriComunication\bin\Debug\net8.0\TauriComunication.dll src-tauri\target\debug
copy src-netcore\TauriComunication\bin\Debug\net8.0\Newtonsoft.Json.dll src-tauri\target\debug
copy src-netcore\TauriLib\bin\Debug\net8.0\TauriIPC.dll src-tauri\target\debug
copy src-netcore\TauriLib\bin\Debug\net8.0\TauriIPC.runtimeconfig.json src-tauri\target\debug

mkdir src-tauri\target\debug\plugins
copy src-netcore\TestApp\bin\Debug\net8.0\TestApp.plugin.dll src-tauri\target\debug\plugins
