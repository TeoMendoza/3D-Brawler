use spacetimedb::{table, ScheduleAt};
use crate::*;

#[table(name = player_effects, index(name = sender_and_type, btree(columns = [sender_id, effect_type])), index(name = target_and_type, btree(columns = [target_id, effect_type])), index(name = target_sender_and_type, btree(columns = [target_id, sender_id, effect_type])))]
#[derive(Clone)]
pub struct PlayerEffect
{
    #[primary_key] #[unique] #[auto_inc] pub id: u64,
    #[index(btree)] pub target_id: u64,
    #[index(btree)] pub sender_id: u64,
    #[index(btree)] pub game_id: u32,

    // Exactly The Same As Effect Class, But Restore For Organizational Purposes
    pub effect_type: EffectType,
    pub application_information: ApplicationInformation,
    pub damage_information: Option<DamageEffectInformation>,
    pub cloak_information: Option<CloakEffectInformation>,
    pub dust_information: Option<DustEffectInformation>,
    pub speed_information: Option<SpeedEffectInformation>,
    pub hypnosis_informaton: Option<HypnosisEffectInformation>,
    pub stunned_information: Option<StunnedEffectInformation>,
    pub tarot_information: Option<TarotEffectInformation>
}

#[table(name = player_effects_table_timer, scheduled(handle_player_effects_table))]
pub struct PlayerEffectsTableTimer {
    #[primary_key] #[auto_inc] pub scheduled_id: u64,
    pub scheduled_at: ScheduleAt,
    pub tick_rate: f32,
    pub game_id: u32,
}