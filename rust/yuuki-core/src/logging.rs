use std::sync::OnceLock;

use crate::errors::{YuukiError, YuukiResult};
use tracing_subscriber::{fmt, EnvFilter};

static LOGGER_INITIALIZED: OnceLock<()> = OnceLock::new();

/// Sets up tracing-based logging with a default info-level filter.
pub fn init_tracing() -> YuukiResult<()> {
    LOGGER_INITIALIZED.get_or_try_init(|| {
        let env_filter =
            EnvFilter::try_from_default_env().unwrap_or_else(|_| EnvFilter::new("info"));
        fmt::fmt()
            .with_env_filter(env_filter)
            .try_init()
            .map_err(|err| YuukiError::LoggingInit(err.to_string()))
    })?;
    Ok(())
}
