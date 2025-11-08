use directories::ProjectDirs;
use once_cell::sync::Lazy;
use rand::Rng;
use serde::{Deserialize, Serialize};
use serde_yaml;
use std::collections::HashMap;
use std::fs;
use std::path::{Path, PathBuf};
use std::sync::Mutex;
use uuid::Uuid;

use crate::errors::YuukiResult;
use tracing::warn;

const ACCOUNT_STORE_NAME: &str = "accounts.yaml";

static AUTH_MANAGER: Lazy<Mutex<AuthManager>> = Lazy::new(|| Mutex::new(AuthManager::new()));

/// Supported account families for Yuuki.
#[derive(Serialize, Deserialize, Debug, Clone)]
pub enum AccountType {
    Microsoft,
    Offline,
    LittleSkin,
}

/// Represents a user account stored by the launcher.
#[derive(Serialize, Deserialize, Debug, Clone)]
pub struct YuukiAccount {
    pub id: String,
    pub username: String,
    pub account_type: AccountType,
    pub provider: String,
}

/// Response returned when initiating the device code flow.
#[derive(Serialize, Deserialize, Debug, Clone)]
pub struct YuukiDeviceCode {
    pub user_code: String,
    pub verification_uri: String,
    pub device_code: String,
    pub expires_in: u32,
    pub message: String,
    pub poll_interval: u32,
}

/// Token information tied to each account.
#[derive(Serialize, Deserialize, Debug, Clone)]
pub struct YuukiTokenResponse {
    pub access_token: String,
    pub refresh_token: String,
    pub expires_in: u32,
}

struct AuthManager {
    accounts: Vec<YuukiAccount>,
    tokens: HashMap<String, YuukiTokenResponse>,
    storage: PathBuf,
}

impl AuthManager {
    fn new() -> Self {
        let storage = Self::resolve_storage_path();
        let mut manager = AuthManager {
            accounts: Vec::new(),
            tokens: HashMap::new(),
            storage,
        };
        if let Err(err) = manager.load_from_disk() {
            warn!("failed to load accounts: {err}");
        }
        manager.ensure_default_account();
        if let Err(err) = manager.persist() {
            warn!("failed to persist default account: {err}");
        }
        manager
    }

    fn resolve_storage_path() -> PathBuf {
        if let Some(dirs) = ProjectDirs::from("com", "yuuki", "YuukiLauncher") {
            dirs.config_dir().join(ACCOUNT_STORE_NAME)
        } else {
            PathBuf::from(ACCOUNT_STORE_NAME)
        }
    }

    fn ensure_default_account(&mut self) {
        if self.accounts.is_empty() {
            let account = YuukiAccount {
                id: "local.offline".into(),
                username: "Offline Player".into(),
                account_type: AccountType::Offline,
                provider: "offline".into(),
            };
            self.tokens.insert(
                account.id.clone(),
                YuukiTokenResponse {
                    access_token: "offline-token".into(),
                    refresh_token: "offline-refresh".into(),
                    expires_in: 0,
                },
            );
            self.accounts.push(account);
        }
    }

    fn list_accounts(&mut self) -> Vec<YuukiAccount> {
        self.ensure_default_account();
        self.accounts.clone()
    }

    fn start_device_login(&mut self, provider: &str) -> YuukiDeviceCode {
        let user_code = format!("{}-{}", provider.to_uppercase(), Self::random_code());
        let device_code = Uuid::new_v4().to_string();
        let account = YuukiAccount {
            id: format!("{provider}:{device_code}"),
            username: "New Device".into(),
            account_type: AccountType::Microsoft,
            provider: provider.into(),
        };
        self.accounts.push(account.clone());
        self.tokens.insert(
            account.id.clone(),
            YuukiTokenResponse {
                access_token: format!("tok-{}", device_code),
                refresh_token: format!("ref-{}", device_code),
                expires_in: 600,
            },
        );
        let _ = self.persist();
        YuukiDeviceCode {
            user_code: user_code.clone(),
            verification_uri: "https://microsoft.com/devicelogin".into(),
            device_code,
            expires_in: 600,
            message: format!(
                "Enter {user_code} at https://microsoft.com/devicelogin to link your account."
            ),
            poll_interval: 5,
        }
    }

    fn add_offline_account(&mut self, username: &str) -> YuukiResult<YuukiAccount> {
        let id = format!("offline-{}", Uuid::new_v4());
        let account = YuukiAccount {
            id: id.clone(),
            username: username.to_string(),
            account_type: AccountType::Offline,
            provider: "offline".into(),
        };
        self.accounts.push(account.clone());
        self.tokens.insert(
            id.clone(),
            YuukiTokenResponse {
                access_token: format!("tok-{id}"),
                refresh_token: format!("ref-{id}"),
                expires_in: 0,
            },
        );
        self.persist()?;
        Ok(account)
    }

    fn refresh_token(&mut self, account_id: &str) -> Option<YuukiTokenResponse> {
        if let Some(token) = self.tokens.get_mut(account_id) {
            token.access_token = format!("tok-{}", Self::random_code());
            token.refresh_token = format!("ref-{}", Self::random_code());
            token.expires_in = 900;
            return Some(token.clone());
        }
        None
    }

    fn random_code() -> String {
        let mut rng = rand::thread_rng();
        (0..6).map(|_| rng.gen_range(0..10).to_string()).collect()
    }

    fn persist(&self) -> YuukiResult<()> {
        if let Some(parent) = self.storage.parent() {
            fs::create_dir_all(parent)?;
        }
        let serialized = serde_yaml::to_string(&self.accounts)?;
        fs::write(&self.storage, serialized)?;
        Ok(())
    }

    fn load_from_disk(&mut self) -> YuukiResult<()> {
        if !self.storage.exists() {
            return Ok(());
        }
        let contents = fs::read_to_string(&self.storage)?;
        let accounts: Vec<YuukiAccount> = serde_yaml::from_str(&contents)?;
        self.accounts = accounts;
        Ok(())
    }
}

pub fn list_accounts() -> YuukiResult<Vec<YuukiAccount>> {
    let mut manager = AUTH_MANAGER.lock().expect("auth manager mutex poisoned");
    Ok(manager.list_accounts())
}

pub fn start_device_login(provider: &str) -> YuukiResult<YuukiDeviceCode> {
    let mut manager = AUTH_MANAGER.lock().expect("auth manager mutex poisoned");
    Ok(manager.start_device_login(provider))
}

pub fn refresh_token(account_id: &str) -> YuukiResult<Option<YuukiTokenResponse>> {
    let mut manager = AUTH_MANAGER.lock().expect("auth manager mutex poisoned");
    Ok(manager.refresh_token(account_id))
}

pub fn add_offline_account(username: &str) -> YuukiResult<YuukiAccount> {
    let mut manager = AUTH_MANAGER.lock().expect("auth manager mutex poisoned");
    manager.add_offline_account(username)
}
