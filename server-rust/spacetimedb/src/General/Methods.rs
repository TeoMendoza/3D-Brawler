use spacetimedb::{ReducerContext};
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

pub fn cleanup_on_disconnect_or_death(ctx: &ReducerContext, magician: &mut Magician)
{
    let collision_entry = CollisionEntry { entry_type: CollisionEntryType::Magician, id: magician.id };
    for mut other in ctx.db.magician().game_id().filter(magician.game_id) {
        if let Some(index) = other.collision_entries.iter().position(|entry| *entry == collision_entry) {
            other.collision_entries.swap_remove(index);
            ctx.db.magician().id().update(other);
        }
    }

    for player_effect in ctx.db.player_effects().target_id().filter(magician.id) {
        match player_effect.effect_type {
            EffectType::Hypnosis => undo_hypnosis_effect_magician(ctx, magician, &player_effect.hypnosis_informaton),
            _ => { }
        }
        ctx.db.player_effects().id().delete(player_effect.id);
    }
}

pub fn cleanup_on_match_end(ctx: &ReducerContext, match_id: u64)
{
    // Delete All Players, Effects, Etc
}







