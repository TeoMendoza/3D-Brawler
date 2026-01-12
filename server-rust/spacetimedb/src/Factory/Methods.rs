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
        player_permission_config: vec![
            PermissionEntry { key: "CanWalk".to_string(), subscribers: Vec::new() },
            PermissionEntry { key: "CanRun".to_string(), subscribers: Vec::new() },
            PermissionEntry { key: "CanJump".to_string(), subscribers: Vec::new() },
            PermissionEntry { key: "CanCrouch".to_string(), subscribers: Vec::new() },
            PermissionEntry { key: "CanAttack".to_string(), subscribers: Vec::new() },
            PermissionEntry { key: "CanReload".to_string(), subscribers: Vec::new() },
        ],
        timers: vec![
            Timer { name: "Attack".to_string(), state: TimerState::Inactive, cooldown_time: 0.7, use_finished_time: 0.7, current_time: 0.0 },
            Timer { name: "Reload".to_string(), state: TimerState::Inactive, cooldown_time: 2.2, use_finished_time: 2.2, current_time: 0.0 },
        ],
        bullets: bullets,
        bullet_capacity: bullet_capacity,
        effects: Vec::new()
    };

    magician
}

pub fn create_throwing_card() -> ThrowingCard 
{
    let application_information = ApplicationInformation { application_type: ApplicationType::Single, current_time: None, end_time: None, reapply_time: None, current_reapply_time: None };
    let damage_information = DamageInformation { base_damage: 25.0, damage_multiplier: 1.0 };
    let effects: Vec<Effect> = vec![Effect { effect_type: EffectType::Damage, application_information: application_information, damage_information: Some(damage_information)}];
    ThrowingCard { effects: effects }
}
