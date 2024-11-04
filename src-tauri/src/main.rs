// Prevents additional console window on Windows in release, DO NOT REMOVE!!
#![cfg_attr(not(debug_assertions), windows_subsystem = "windows")]

use tauri_dotnet_bridge_host;

#[tauri::command]
fn dotnet_request(request: &str) -> String {
    tauri_dotnet_bridge_host::process_request(request)
}

fn main() {
    tauri::Builder::default()
        .invoke_handler(tauri::generate_handler![dotnet_request])
        .run(tauri::generate_context!())
        .expect("error while running tauri application");
}
