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

pub fn initialize() -> &'static AssemblyDelegateLoader {
    &ASM
}

unsafe extern "system" fn copy_to_c_string(ptr: *const u16, length: i32) -> *mut c_char {
    let wide_chars = unsafe { slice::from_raw_parts(ptr, length as usize) };
    let string = String::from_utf16_lossy(wide_chars);
    let c_string = match CString::new(string) {
        Ok(c_string) => c_string,
        Err(_) => return std::ptr::null_mut(),
    };
    c_string.into_raw()
}

pub fn exec_function(method_name: &str) -> String {
    let instance = initialize();

    let set_copy_to_c_string = instance
        .get_function_with_unmanaged_callers_only::<fn(f: unsafe extern "system" fn(*const u16, i32) -> *mut c_char)>(
            pdcstr!("TauriIPC.Bridge, TauriIPC"),
            pdcstr!("SetCopyToCStringFunctionPtr"),
        ).unwrap();

    set_copy_to_c_string(copy_to_c_string);

    let handler_utf8 = instance
        .get_function_with_unmanaged_callers_only::<fn(text_ptr: *const u8, text_length: i32) -> *mut c_char>(
            pdcstr!("TauriIPC.Bridge, TauriIPC"),
            pdcstr!("process_request"),
        )
        .unwrap();

    let ptr_string = handler_utf8(method_name.as_ptr(), method_name.len() as i32);
    let data = unsafe { CString::from_raw(ptr_string) };

    format!("{}", data.to_string_lossy())
}
