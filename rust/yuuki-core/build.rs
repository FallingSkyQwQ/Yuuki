fn main() {
    if std::env::var_os("FRB_DART_OUTPUT_DIR").is_none() {
        println!(
            "cargo:warning=FRB_DART_OUTPUT_DIR not set; skipping flutter_rust_bridge codegen."
        );
        return;
    }

    let dart_dir = std::env::var("FRB_DART_OUTPUT_DIR").expect("checked above");

    flutter_rust_bridge_codegen::CodegenBuilder::new()
        .input("src/api.rs")
        .dart_output_dir(dart_dir)
        .rust_output("src/api.rs")
        .build()
        .expect("flutter_rust_bridge_codegen failed");
}
