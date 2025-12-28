use spacetimedb::{reducer, table, rand::Rng, Identity, SpacetimeType, ReducerContext, ScheduleAt, Table, Timestamp};
use crate::*;

#[table(name = ping_status, public)]
pub struct PingStatus {
    #[primary_key]
    pub identity: Identity,
    pub last_sequence: u32,
}

#[reducer]
pub fn ping(ctx: &ReducerContext, sequence: u32) {
    let existing = ctx.db.ping_status().identity().find(ctx.sender);

    if existing.is_some() {
        let mut row = existing.unwrap();
        row.last_sequence = sequence;
        ctx.db.ping_status().identity().update(row);
        return;
    }

    ctx.db.ping_status().insert(PingStatus { identity: ctx.sender, last_sequence: sequence });
}
