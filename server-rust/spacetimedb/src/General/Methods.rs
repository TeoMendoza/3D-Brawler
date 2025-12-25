use std::time::Duration;
use spacetimedb::{rand::Rng, Identity, SpacetimeType, ReducerContext, ScheduleAt, Table, Timestamp};

use crate::*;
use crate::Collisions::*;
use crate::Map::*;
use crate::Magician::*;


pub fn AddSubscriberUnique(subscribers: &mut Vec<String>, reason: &str) -> ()
{
    if subscribers.iter().any(|existing| existing == reason) {
        return;
    }
    subscribers.push(reason.to_string());
}

pub fn RemoveSubscriber(subscribers: &mut Vec<String>, reason: &str) {
    if let Some(index) = subscribers.iter().rposition(|existing| existing == reason) {
        subscribers.remove(index);
    }
}

pub fn GetPermissionEntry(entries: &mut Vec<PermissionEntry>, key: &str) -> Option<&mut PermissionEntry>
{
    for entry in entries.iter_mut() {
        if entry.key == key {
            return Some(entry);
        }
    }
    
    None
}

