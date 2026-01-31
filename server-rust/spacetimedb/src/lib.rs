#![allow(non_snake_case)]
#![allow(ambiguous_glob_reexports)]

pub mod Collisions;
pub mod Factory;
pub mod General;
pub mod Magicians;
pub mod Maps;
pub mod Effects;
pub mod UnitTests;

pub use General::*;
pub use Effects::*;
pub use Factory::*;
pub use Magicians::*;
pub use Maps::*;
pub use Collisions::*;
pub use UnitTests::*;
