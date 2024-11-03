use lazy_static::lazy_static;
use netcorehost::{hostfxr::AssemblyDelegateLoader, nethost, pdcstr};
use std::env;
use std::ffi::{c_char, CString};

lazy_static! {
    static ref ASM: AssemblyDelegateLoader = {
        let hostfxr = nethost::load_hostfxr().unwrap();

        let exe_path = env::current_exe().expect("Failed to get the executable path");
        let exe_dir = exe_path
            .parent()
            .expect("Failed to get the executable directory");

        env::set_current_dir(&exe_dir).expect("Failed to set current directory");

        let context = hostfxr
            .initialize_for_runtime_config(pdcstr!("TauriDotNetBridge.runtimeconfig.json"))
            .expect("Invalid runtime configuration");

        context
            .get_delegate_loader_for_assembly(pdcstr!("TauriDotNetBridge.dll"))
            .expect("Failed to load DLL")
    };
}

pub fn process_request(request: &str) -> String {
    let instance = &ASM;

    let process_request = instance
        .get_function_with_unmanaged_callers_only::<fn(text_ptr: *const u8, text_length: i32) -> *mut c_char>(
            pdcstr!("TauriDotNetBridge.Bridge, TauriDotNetBridge"),
            pdcstr!("ProcessRequest"),
        )
        .unwrap();

    let response_ptr = process_request(request.as_ptr(), request.len() as i32);

    let response = unsafe { CString::from_raw(response_ptr) };

    format!("{}", response.to_string_lossy())
}
