use core::slice;
use std::env;
use std::ffi::{c_char, CString};

use lazy_static::lazy_static;
use netcorehost::{hostfxr::AssemblyDelegateLoader, nethost, pdcstr};

lazy_static! {
    static ref ASM:AssemblyDelegateLoader = {
        let hostfxr = nethost::load_hostfxr().unwrap();

        let exe_path = env::current_exe().expect("Failed to get the executable path");
        let exe_dir = exe_path
            .parent()
            .expect("Failed to get the executable directory");

        env::set_current_dir(&exe_dir).expect("Failed to set current directory");

        let context = hostfxr
            .initialize_for_runtime_config(pdcstr!("TauriIPC.runtimeconfig.json"))
            .expect("Invalid runtime configuration");

        context
            .get_delegate_loader_for_assembly(pdcstr!("TauriIPC.dll"))
            .expect("Failed to load DLL")
    };
}

pub fn exec_function(method_name: &str) -> String {
    let instance = &ASM;

    let process_request = instance
        .get_function_with_unmanaged_callers_only::<fn(text_ptr: *const u8, text_length: i32) -> *mut c_char>(
            pdcstr!("TauriIPC.Bridge, TauriIPC"),
            pdcstr!("ProcessRequest"),
        )
        .unwrap();

    let ptr_string = process_request(method_name.as_ptr(), method_name.len() as i32);

    let data = unsafe { CString::from_raw(ptr_string) };

    format!("{}", data.to_string_lossy())
}