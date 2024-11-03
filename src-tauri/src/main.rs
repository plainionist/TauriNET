// Prevents additional console window on Windows in release, DO NOT REMOVE!!
#![cfg_attr(not(debug_assertions), windows_subsystem = "windows")]

mod dotnet_host;

#[tauri::command]
fn plugin_request(data_str: &str) -> String {
    dotnet_host::exec_function(data_str)
}

fn main() {
    tauri::Builder::default()
        .invoke_handler(tauri::generate_handler![plugin_request])
        .run(tauri::generate_context!())
        .expect("error while running tauri application");
}
