use flutter_rust_bridge::frb;

use crate::{
    auth::{
        list_accounts, refresh_token, start_device_login, YuukiAccount, YuukiDeviceCode,
        YuukiTokenResponse,
    },
    config::{default_profile, load_profile},
    init_core,
};

#[frb]
pub fn yuuki_status() -> String {
    "Yuuki core is alive".to_string()
}

#[frb]
pub fn yuuki_initialize() -> bool {
    init_core().is_ok()
}

#[frb]
pub fn yuuki_profile_preview(path: &str) -> String {
    match load_profile(path) {
        Ok(profile) => format!("Profile: {} (loader {})", profile.name, profile.loader),
        Err(err) => format!("Failed to load profile: {err}"),
    }
}

#[frb]
pub fn yuuki_default_profile() -> String {
    format!("{}", default_profile().name)
}

#[frb]
pub fn yuuki_list_accounts() -> Vec<YuukiAccount> {
    list_accounts().unwrap_or_default()
}

#[frb]
pub fn yuuki_start_device_login(provider: &str) -> YuukiDeviceCode {
    start_device_login(provider).unwrap_or_else(|_| YuukiDeviceCode {
        user_code: "000000".into(),
        verification_uri: "https://example.com".into(),
        device_code: "stub-device".into(),
        expires_in: 0,
        message: "Device login temporarily unavailable".into(),
        poll_interval: 5,
    })
}

#[frb]
pub fn yuuki_refresh_token(account_id: &str) -> Option<YuukiTokenResponse> {
    refresh_token(account_id).unwrap_or(None)
}

#[frb]
pub fn yuuki_add_offline_account(username: &str) -> YuukiAccount {
    add_offline_account(username).unwrap_or_else(|_| YuukiAccount {
        id: "local.offline".into(),
        username: username.into(),
        account_type: AccountType::Offline,
        provider: "offline".into(),
    })
}
