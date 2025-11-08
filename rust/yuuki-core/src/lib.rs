pub mod api;
pub mod auth;
pub mod config;
pub mod errors;
pub mod logging;

pub use auth::{
    list_accounts, refresh_token, start_device_login, YuukiAccount, YuukiDeviceCode,
    YuukiTokenResponse,
};
pub use config::*;
pub use errors::{YuukiError, YuukiResult};
pub use logging::init_tracing;

/// Initializes the core runtime (logging, diagnostics, etc.).
pub fn init_core() -> YuukiResult<()> {
    init_tracing()
}
