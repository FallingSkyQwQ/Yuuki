use thiserror::Error;

/// Failure types surfaced by the core crates.
#[derive(Error, Debug)]
pub enum YuukiError {
    #[error("logging initialization failed: {0}")]
    LoggingInit(String),
    #[error("configuration I/O error: {0}")]
    ConfigIo(#[from] std::io::Error),
    #[error("configuration parse error: {0}")]
    ConfigParse(#[from] serde_yaml::Error),
}

pub type YuukiResult<T> = Result<T, YuukiError>;
