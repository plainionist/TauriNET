// use std::env;
// use std::process::Command;

fn main() {
    // let profile = env::var("PROFILE").unwrap_or_default();
    // if profile == "release" {
    //     Command::new("dotnet")
    //         .arg("build")
    //         .arg("-c")
    //         .arg("Release")
    //         .arg("-p:OutputPath=../../src-tauri/target/release/dotnet")
    //         .current_dir("../src-dotnet")
    //         .status()
    //         .expect("Failed to run dotnet build");
    // } else {
    //     Command::new("dotnet")
    //         .arg("build")
    //         .current_dir("../src-dotnet")
    //         .status()
    //         .expect("Failed to run dotnet build");
    // }

    tauri_build::build()
}
