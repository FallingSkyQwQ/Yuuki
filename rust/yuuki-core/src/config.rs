use serde::{Deserialize, Serialize};
use std::fs;
use std::path::Path;

use crate::errors::{YuukiError, YuukiResult};

/// Represents the loader family for a profile.
#[derive(Debug, Serialize, Deserialize, Clone)]
pub enum LoaderType {
    Vanilla,
    Forge,
    Fabric,
    Quilt,
    NeoForge,
}

/// Java runtime configuration attached to each profile.
#[derive(Debug, Serialize, Deserialize, Clone)]
pub struct JavaConfig {
    pub path: String,
    pub minimum_version: String,
}

/// Settings that users can tune per-profile.
#[derive(Debug, Serialize, Deserialize, Clone, Default)]
pub struct Settings {
    pub ram_mb: u32,
    pub java_flags: Vec<String>,
}

/// Profile metadata persisted to disk.
#[derive(Debug, Serialize, Deserialize, Clone)]
pub struct ProfileConfig {
    pub id: String,
    pub name: String,
    pub loader: LoaderType,
    pub java: JavaConfig,
    pub settings: Settings,
}

/// Returns a baseline profile useful for onboarding.
pub fn default_profile() -> ProfileConfig {
    ProfileConfig {
        id: "default".into(),
        name: "Default Profile".into(),
        loader: LoaderType::Vanilla,
        java: JavaConfig {
            path: String::from("/usr/lib/jvm/temurin/bin/java"),
            minimum_version: "17".into(),
        },
        settings: Settings {
            ram_mb: 4096,
            java_flags: vec!["-XX:+UseG1GC".into()],
        },
    }
}

/// Loads a profile from a YAML file.
pub fn load_profile(path: impl AsRef<Path>) -> YuukiResult<ProfileConfig> {
    let path = path.as_ref();
    let contents = fs::read_to_string(path)?;
    let profile: ProfileConfig = serde_yaml::from_str(&contents)?;
    Ok(profile)
}

/// Saves the profile configuration back to YAML.
pub fn save_profile(path: impl AsRef<Path>, profile: &ProfileConfig) -> YuukiResult<()> {
    let serialized = serde_yaml::to_string(profile)?;
    fs::write(path, serialized)?;
    Ok(())
}
