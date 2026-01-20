use crate::*;

pub fn create_magician(config: MagicianConfig) -> Magician 
{
    let player = config.player;
    let game_id = config.game_id;
    let position = config.position;

    let bullet_capacity: i32 = 8;

    let mut bullets: Vec<ThrowingCard> = Vec::with_capacity(bullet_capacity as usize);
    for _i in 0..bullet_capacity {
        bullets.push(create_throwing_card());
    }

    let magician = Magician {
        identity: player.identity,
        id: player.id,
        name: player.name,
        game_id,
        position,
        rotation: DbRotation2 { yaw: 0.0, pitch: 0.0 },
        velocity: DbVector3 { x: 0.0, y: 0.0, z: 0.0 },
        corrected_velocity: DbVector3 { x: 0.0, y: 0.0, z: 0.0 },
        collider: MagicianIdleCollider(),
        collision_entries: vec![CollisionEntry { entry_type: CollisionEntryType::Map, id: 1 }],
        is_colliding: false,
        state: MagicianState::Default,
        kinematic_information: KinematicInformation { jump: false, falling: false, crouched: false, grounded: false, sprinting: false },
        combat_information: CombatInformation { health: 200.0, max_health: 200.0, speed_multiplier: 1.0, game_score: 0},
        permissions: vec![
            PermissionEntry { key: "CanWalk".to_string(), subscribers: Vec::new() },
            PermissionEntry { key: "CanRun".to_string(), subscribers: Vec::new() },
            PermissionEntry { key: "CanJump".to_string(), subscribers: Vec::new() },
            PermissionEntry { key: "CanCrouch".to_string(), subscribers: Vec::new() },
            PermissionEntry { key: "CanAttack".to_string(), subscribers: Vec::new() },
            PermissionEntry { key: "CanReload".to_string(), subscribers: Vec::new() },
            PermissionEntry { key: "CanDust".to_string(), subscribers: Vec::new() },
            PermissionEntry { key: "CanCloak".to_string(), subscribers: Vec::new() },
            PermissionEntry { key: "CanHypnosis".to_string(), subscribers: Vec::new() },
            PermissionEntry { key: "CanTarot".to_string(), subscribers: Vec::new() },
            PermissionEntry { key: "Stunned".to_string(), subscribers: Vec::new() },
            PermissionEntry { key: "Dusted".to_string(), subscribers: Vec::new() },
            PermissionEntry { key: "Taroted".to_string(), subscribers: Vec::new() },
            PermissionEntry { key: "Cloaked".to_string(), subscribers: Vec::new() },
            PermissionEntry { key: "Hypnosised".to_string(), subscribers: Vec::new() },
        ],    
        timers: vec![
            Timer { name: "Attack".to_string(), state: TimerState::Inactive, cooldown_time: 0.7, use_finished_time: 0.7, current_time: 0.0 },
            Timer { name: "Reload".to_string(), state: TimerState::Inactive, cooldown_time: 2.2, use_finished_time: 2.2, current_time: 0.0 },
            Timer { name: "Dust".to_string(), state: TimerState::Inactive, cooldown_time: 10.0, use_finished_time: 2.4, current_time: 0.0 },
            Timer { name: "Cloak".to_string(), state: TimerState::Inactive, cooldown_time: 20.0, use_finished_time: 1.5, current_time: 0.0 },
            Timer { name: "Hypnosis".to_string(), state: TimerState::Inactive, cooldown_time: 20.0, use_finished_time: 2.0, current_time: 0.0 },

        ],
        stateless_timers: vec![
            StatelessTimer { name: "Tarot".to_string(), state: StatelessTimerState::Inactive, cooldown_time: 20.0, application_time: 0.0, current_time: 0.0},
        ],
        bullets: bullets,
        bullet_capacity: bullet_capacity,
        effects: Vec::new()
    };

    magician
}

pub fn create_throwing_card() -> ThrowingCard 
{
    let damage_effect = create_damage_effect(25.0, 1.0);
    let effects: Vec<Effect> = vec![damage_effect];
    ThrowingCard { effects: effects }
}

pub fn create_damage_effect(base_damage: f32, multiplier: f32) -> Effect
{
    let application_information = ApplicationInformation { application_type: ApplicationType::Single, current_time: None, end_time: None, reapply_time: None, current_reapply_time: None, applied: None};
    let damage_information = DamageEffectInformation { base_damage: base_damage, damage_multiplier: multiplier };
    let damage = Effect { effect_type: EffectType::Damage, application_information: application_information, damage_information: Some(damage_information), cloak_information: None, dust_information: None, speed_information: None, hypnosis_information: None, stunned_information: None, tarot_information: None};

    damage
}

pub fn create_cloak_effect(duration: f32) -> Effect 
{
    let application_information = ApplicationInformation {application_type: ApplicationType::Duration, current_time: Some(0.0), end_time: Some(duration), reapply_time: None, current_reapply_time: None, applied: None};
    let cloak_information = CloakEffectInformation { };
    let cloak = Effect { effect_type: EffectType::Cloak, application_information: application_information, damage_information: None, cloak_information: Some(cloak_information), dust_information: None, speed_information: None, hypnosis_information: None, stunned_information: None, tarot_information: None};
    
    cloak
}

pub fn create_speed_multiplier_effect(multiplier: f32, duration: f32) -> Effect 
{
    let application_information = ApplicationInformation { application_type: ApplicationType::Duration, current_time: Some(0.0), end_time: Some(duration), reapply_time: None, current_reapply_time: None, applied: None};
    let speed_information = SpeedEffectInformation { speed_multiplier: multiplier };
    let speed = Effect { effect_type: EffectType::Speed, application_information: application_information, damage_information: None, cloak_information: None, dust_information: None, speed_information: Some(speed_information), hypnosis_information: None, stunned_information: None, tarot_information: None};
    
    speed
}

pub fn create_dust_effect(duration: f32) -> Effect 
{
    let application_information = ApplicationInformation { application_type: ApplicationType::Duration, current_time: Some(0.0), end_time: Some(duration), reapply_time: None, current_reapply_time: None, applied: None};
    let dust_information = DustEffectInformation {};
    let dust = Effect { effect_type: EffectType::Dust, application_information: application_information, damage_information: None, cloak_information: None, dust_information: Some(dust_information), speed_information: None, hypnosis_information: None, stunned_information: None, tarot_information: None};
    
    dust
}

pub fn create_hypnosis_effect(duration: f32) -> Effect 
{
    let application_information = ApplicationInformation { application_type: ApplicationType::Duration, current_time: Some(0.0), end_time: Some(duration), reapply_time: None, current_reapply_time: None, applied: None};
    let hypnonsis_information = HypnosisEffectInformation { last_target_id: None };
    let hypnosis = Effect { effect_type: EffectType::Hypnosis, application_information: application_information, damage_information: None, cloak_information: None, dust_information: None, speed_information: None, hypnosis_information: Some(hypnonsis_information), stunned_information: None, tarot_information: None};
    
    hypnosis
}

pub fn create_stunned_effect() -> Effect 
{
    let application_information = ApplicationInformation { application_type: ApplicationType::Indefinite, current_time: None, end_time: None, reapply_time: None, current_reapply_time: None, applied: Some(false)};
    let stunned_information = StunnedEffectInformation { };
    let stunned = Effect { effect_type: EffectType::Stunned, application_information: application_information, damage_information: None, cloak_information: None, dust_information: None, speed_information: None, hypnosis_information: None, stunned_information: Some(stunned_information), tarot_information: None };
    
    stunned
}

pub fn create_tarot_effect(duration: f32) -> Effect 
{
    let application_information = ApplicationInformation { application_type: ApplicationType::Duration, current_time: Some(0.0), end_time: Some(duration), reapply_time: None, current_reapply_time: None, applied: None};
    let tarot_information = TarotEffectInformation { };
    let tarot = Effect { effect_type: EffectType::Tarot, application_information: application_information, damage_information: None, cloak_information: None, dust_information: None, speed_information: None, hypnosis_information: None, stunned_information: None, tarot_information: Some(tarot_information) };
    
    tarot
}



