use spacetimedb::{table, Identity};

#[table(name = logged_in_players, public)]
#[table(name = logged_out_players)]
pub struct Player {
    #[primary_key]
    pub identity: Identity,
    #[unique] #[auto_inc]
    pub id: u64,
    pub name: String

}

#[table(name = game, public)]
pub struct Game {
    #[unique] #[primary_key] #[auto_inc]
    pub id: u32,
    pub max_players: u32,
    pub current_players: u32,
    #[index(btree)]
    pub in_progress: bool
}
