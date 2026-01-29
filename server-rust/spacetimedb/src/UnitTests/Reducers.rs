use spacetimedb::{reducer, ReducerContext, ScheduleAt, TimeDuration, Table};
use std::time::Duration;
use crate::*;

#[reducer]
pub fn test_join_and_start_game_single_player(ctx: &ReducerContext) {
    log::info!("Test Join And Start Game Called");
    let player = ctx
        .db
        .logged_in_players()
        .identity()
        .find(ctx.sender)
        .expect("Player must be logged in");

    let mut game: Game = match ctx.db.game().in_progress().filter(false).next() {
        Some(existing_game) => existing_game,
        None => {
            let created_game = ctx.db.game().insert(Game {
                id: 0,
                max_players: 12,
                current_players: 0,
                in_progress: false,
            });

            let tick_millis: u64 = 1000 / 60;
            let tick_rate: f32 = 1.0 / 60.0;

            ctx.db.move_all_magicians().insert(MoveAllMagiciansTimer {
                scheduled_id: 0,
                scheduled_at: ScheduleAt::Interval(Duration::from_millis(tick_millis).into()),
                tick_rate,
                game_id: created_game.id,
            });

            ctx.db.handle_magician_timers_timer().insert(HandleMagicianTimersTimer {
                scheduled_id: 0,
                scheduled_at: ScheduleAt::Interval(Duration::from_millis(tick_millis).into()),
                tick_rate,
                game_id: created_game.id,
            });

            ctx.db.handle_magician_stateless_timers_timer().insert(HandleMagicianStatelessTimersTimer {
                scheduled_id: 0,
                scheduled_at: ScheduleAt::Interval(Duration::from_millis(tick_millis).into()),
                tick_rate,
                game_id: created_game.id,
            });

            ctx.db.gravity_magician().insert(GravityTimerMagician {
                scheduled_id: 0,
                scheduled_at: ScheduleAt::Interval(Duration::from_millis(tick_millis).into()),
                tick_rate,
                gravity: 20.0,
                game_id: created_game.id,
            });

            ctx.db.player_effects_table_timer().insert(PlayerEffectsTableTimer {
                scheduled_id: 0,
                scheduled_at: ScheduleAt::Interval(Duration::from_millis(tick_millis).into()),
                tick_rate,
                game_id: created_game.id,
            });

            created_game
        }
    };

    game.current_players += 1;

    if game.current_players >= 1 && !game.in_progress {
        game.in_progress = true;

        let end_time = ctx
            .timestamp
            .checked_add(TimeDuration::from_micros(600_000_000))
            .expect("Match End Time Timestamp Overflow");

        ctx.db.game_timers().insert(GameTimersTimer {
            scheduled_id: 0,
            scheduled_at: ScheduleAt::Time(end_time),
            game_id: game.id,
        });
    }

    let game_id = game.id;
    ctx.db.game().id().update(game);

    let magician_config = MagicianConfig {
        player,
        game_id,
        position: DbVector3 { x: 0.0, y: 0.0, z: 0.0 },
    };

    let magician = create_magician(magician_config);
    let inserted_magician = ctx.db.magician().insert(magician);

    let invincible_effect = create_invincible_effect(5.0);
    add_effects_to_table(ctx, vec![invincible_effect], inserted_magician.id, inserted_magician.id, game_id);
}

#[reducer]
pub fn test_leave_game_and_cleanup_match_if_empty(ctx: &ReducerContext) {
    let magician_option = ctx.db.magician().identity().find(ctx.sender);
    if magician_option.is_none() {
        return;
    }

    let mut magician = magician_option.unwrap();
    let game_id = magician.game_id;

    cleanup_on_disconnect_or_death(ctx, &mut magician);
    ctx.db.magician().identity().delete(ctx.sender);

    let mut game = ctx.db.game().id().find(game_id).expect("Game not found");
    if game.current_players > 0 {
        game.current_players -= 1;
    }

    if game.current_players == 0 {
        cleanup_on_game_end(ctx, game_id);
        ctx.db.game_timers().game_id().delete(game_id);
        ctx.db.game().id().delete(game_id);
        return;
    }

    ctx.db.game().id().update(game);
}


#[reducer]
pub fn enable_unit_test_mode(ctx: &ReducerContext) {
    if ctx.db.unit_test_mode().id().find(0).is_none() {
        ctx.db.unit_test_mode().insert(UnitTestMode { id: 0, enabled: true });
    } else {
        let mut row = ctx.db.unit_test_mode().id().find(0).unwrap();
        row.enabled = true;
        ctx.db.unit_test_mode().id().update(row);
    }
}

#[reducer]
pub fn disable_unit_test_mode(ctx: &ReducerContext) {
    if let Some(mut row) = ctx.db.unit_test_mode().id().find(0) {
        row.enabled = false;
        ctx.db.unit_test_mode().id().update(row);
    }
}

pub fn IsUnitTestModeEnabled(ctx: &ReducerContext) -> bool 
{
    ctx.db.unit_test_mode().id().find(0).map(|x| x.enabled).unwrap_or(false)
}
