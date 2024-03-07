// Prevents additional console window on Windows in release, DO NOT REMOVE!!
// #![cfg_attr(not(debug_assertions), windows_subsystem = "windows")]

mod host_loader;

// Learn more about Tauri commands at https://tauri.app/v1/guides/features/command
#[tauri::command]
fn greet(name: &str) -> String {
    let returned_string_utf8 = host_loader::run_method_utf8(name);
    format!("{}", returned_string_utf8)
}

fn main() {
    // Precarga el DLL de NetHost
    host_loader::get_instance();

    // Run tauri
    tauri::Builder::default()
        .invoke_handler(tauri::generate_handler![greet])
        .run(tauri::generate_context!())
        .expect("error while running tauri application");
}
