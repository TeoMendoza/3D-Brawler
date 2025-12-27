use crate::*;

pub fn CreateMagician(config: MagicianConfig) -> Magician {
    let player = config.player;
    let game_id = config.game_id;
    let position = config.position;

    let mut magician = Magician {
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
            Timer { name: "Attack".to_string(), current_time: 0.7, reset_time: 0.7 },
            Timer { name: "Reload".to_string(), current_time: 2.2, reset_time: 2.2 },
        ],
        bullets: Vec::new(),
        bullet_capacity: 8,
    };

    let mut bullets: Vec<ThrowingCard> = Vec::with_capacity(magician.bullet_capacity as usize);
    for _i in 0..magician.bullet_capacity {
        let throwing_card: ThrowingCard = CreateThrowingCard();
        bullets.push(throwing_card);
    }
    magician.bullets = bullets;

    magician
}

pub fn CreateThrowingCard() -> ThrowingCard {
    let effects: Vec<Effect> = vec![Effect { effect_type: EffectType::Damage }];
    ThrowingCard { effects }
}
