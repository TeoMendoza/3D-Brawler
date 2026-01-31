use spacetimedb::{Identity, ScheduleAt, table};
use crate::*;

#[table(name = logged_in_players, public)] // Public for lobby purposes
#[table(name = logged_out_players)]
pub struct Player {
    #[primary_key] pub identity: Identity,
    #[unique] #[auto_inc] pub id: u64,
    pub name: String // In game name
}

#[table(name = game, public)]
pub struct Game {
    #[unique] #[primary_key] #[auto_inc] pub id: u32,
    #[index(btree)] pub in_progress: bool,
    pub max_players: u32,
    pub current_players: u32,
}

#[table(name = game_timers, scheduled(handle_game_end))]
pub struct GameTimersTimer {
    #[primary_key] #[auto_inc] pub scheduled_id: u64,
    #[unique] pub game_id: u32,
    pub scheduled_at: ScheduleAt,
}

#[table(name = respawn_timers, scheduled(handle_respawn))]
pub struct RespawnTimersTimer {
    #[primary_key] #[auto_inc] pub scheduled_id: u64,
    #[index(btree)] pub game_id: u32,
    #[unique] pub identity: Identity, // Used to find target in logged in players (confirms existence)
    pub scheduled_at: ScheduleAt,
}