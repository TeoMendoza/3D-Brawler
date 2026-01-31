#[spacetimedb::table(name = unit_test_mode)]
pub struct UnitTestMode {
    #[primary_key] pub id: u32,
    pub enabled: bool,
}
