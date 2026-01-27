use spacetimedb::{ReducerContext, Table, TimeDuration, ScheduleAt};
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

pub fn handle_magician_death(ctx: &ReducerContext, magician: &mut Magician) 
{
    let player_option = ctx.db.logged_in_players().identity().find(magician.identity);
    cleanup_on_disconnect_or_death(ctx, magician);
    
    if let Some(player) = player_option {
        let respawn_time = ctx.timestamp.checked_add(TimeDuration::from_micros(5_000_000)).expect("Respawn Timestamp Overflow");
        let respawn_timer = RespawnTimersTimer { scheduled_id: 0, scheduled_at: ScheduleAt::Time(respawn_time), game_id: magician.game_id, player: player, identity: magician.identity};
        ctx.db.respawn_timers().insert(respawn_timer);
    }

    ctx.db.magician().id().delete(magician.id);
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

pub fn cleanup_on_game_end(ctx: &ReducerContext, match_id: u32)
{
    let game_players_iterator = ctx.db.magician().game_id().filter(match_id);
    let effects_iterator = ctx.db.player_effects().game_id().filter(match_id);
    let respawns_iterator = ctx.db.respawn_timers().game_id().filter(match_id);

    for magician in game_players_iterator {
        ctx.db.magician().id().delete(magician.id);
    }

    for effect in effects_iterator {
        ctx.db.player_effects().id().delete(effect.id);
    }

    for respawn in respawns_iterator {
        ctx.db.respawn_timers().scheduled_id().delete(respawn.scheduled_id);
    }

    ctx.db.move_all_magicians().game_id().delete(match_id);
    ctx.db.gravity_magician().game_id().delete(match_id);
    ctx.db.handle_magician_timers_timer().game_id().delete(match_id);
    ctx.db.handle_magician_stateless_timers_timer().game_id().delete(match_id);
    ctx.db.player_effects_table_timer().game_id().delete(match_id);
}







