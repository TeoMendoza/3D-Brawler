use crate::*;

pub fn IsPermissionUnblocked(entries: &[PermissionEntry], key: &str) -> bool
{
    let entry: &PermissionEntry = GetPermissionEntry(entries, key).expect("Permission entry not found");
    return entry.subscribers.len() == 0;
}

pub fn GetPermissionEntry<'a>(entries: &'a [PermissionEntry], key: &str) -> Option<&'a PermissionEntry>
{
    for entry in entries.iter() {
        if entry.key == key {
            return Some(entry);
        }
    }
    
    None
}

pub fn AddSubscriberToPermission(entries: &mut [PermissionEntry], key: &str, subscriber: &str) {
    for entry in entries.iter_mut() {
        if entry.key == key {
            AddSubscriberUnique(&mut entry.subscribers, subscriber);
            return;
        }
    }
    panic!("Permission entry not found: {}", key);
}

pub fn RemoveSubscriberFromPermission(entries: &mut [PermissionEntry], key: &str, subscriber: &str) {
    for entry in entries.iter_mut() {
        if entry.key == key {
            RemoveSubscriber(&mut entry.subscribers, subscriber);
            return;
        }
    }
    panic!("Permission entry not found: {}", key);
}

pub fn AddSubscriberUnique(subscribers: &mut Vec<String>, reason: &str) -> ()
{
    if subscribers.iter().any(|existing: &String| existing == reason) {
        return;
    }
    subscribers.push(reason.to_string());
}

pub fn RemoveSubscriber(subscribers: &mut Vec<String>, reason: &str) {
    if let Some(index) = subscribers.iter().rposition(|existing| existing == reason) {
        subscribers.remove(index);
    }
}







