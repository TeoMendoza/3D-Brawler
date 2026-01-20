use crate::*;

pub fn is_permission_unblocked(entries: &[PermissionEntry], key: &str) -> bool
{
    let entry: &PermissionEntry = get_permission_entry(entries, key).expect("Permission entry not found");
    return entry.subscribers.is_empty()
}

pub fn get_permission_entry<'a>(entries: &'a [PermissionEntry], key: &str) -> Option<&'a PermissionEntry>
{
    for entry in entries.iter() {
        if entry.key == key {
            return Some(entry);
        }
    }
    
    None
}

pub fn add_subscriber_to_permission(entries: &mut [PermissionEntry], key: &str, subscriber: &str) 
{
    for entry in entries.iter_mut() {
        if entry.key == key {
            add_subscriber(&mut entry.subscribers, subscriber);
            return;
        }
    }
    panic!("Permission entry not found: {}", key);
}

pub fn remove_subscriber_from_permission(entries: &mut [PermissionEntry], key: &str, subscriber: &str) 
{
    for entry in entries.iter_mut() {
        if entry.key == key {
            remove_subscriber(&mut entry.subscribers, subscriber);
            return;
        }
    }
    panic!("Permission entry not found: {}", key);
}

pub fn add_subscriber(subscribers: &mut Vec<String>, reason: &str)
{
    subscribers.push(reason.to_string());
}

pub fn remove_subscriber(subscribers: &mut Vec<String>, reason: &str) 
{
    if let Some(index) = subscribers.iter().position(|existing| existing == reason) {
        subscribers.swap_remove(index);
    }
}








