use std::time::Duration;
use spacetimedb::{reducer, ReducerContext, ScheduleAt, Table, TimeDuration};
use crate::*;

#[reducer]
pub fn test_smoke(_ctx: &ReducerContext) { }