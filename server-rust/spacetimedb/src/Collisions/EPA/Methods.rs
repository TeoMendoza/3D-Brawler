use crate::*;

pub fn epa_solve(gjk: &GjkResult, collider_a: &Vec<ConvexHullCollider>, position_a: DbVector3, yaw_radians_a: f32, collider_b: &Vec<ConvexHullCollider>, position_b: DbVector3, yaw_radians_b: f32, contact_out: &mut Contact) -> bool { // Returns accurate collision normal and depth for two colliding objects
    let max_iterations: i32 = 16;
    let epsilon: f32 = 2e-3;
    let max_polytope_vertices: usize = 64;

    *contact_out = Contact { normal: DbVector3 { x: 0.0, y: 1.0, z: 0.0 }, depth: 0.0 };

    if gjk.simplex.len() < 4 { return false; }

    let mut polytope_vertices: Vec<GjkVertex> = gjk.simplex.clone();

    let mut faces: Vec<EpaFace> = Vec::new();
    faces.reserve(32);

    add_face(&polytope_vertices, &mut faces, 0, 1, 2);
    add_face(&polytope_vertices, &mut faces, 0, 3, 1);
    add_face(&polytope_vertices, &mut faces, 0, 2, 3);
    add_face(&polytope_vertices, &mut faces, 1, 3, 2);

    let mut edges: Vec<(i32, i32)> = Vec::new();
    edges.reserve(64);

    for _iteration in 0..max_iterations {
        let mut closest_face_index: i32 = -1;
        let mut closest_distance: f32 = f32::MAX;

        for face_index in 0..faces.len() {
            let face = &faces[face_index];
            if face.obsolete { continue; }
            if face.distance < closest_distance {
                closest_distance = face.distance;
                closest_face_index = face_index as i32;
            }
        }

        if closest_face_index < 0 { return false; }

        let search_direction = faces[closest_face_index as usize].normal;

        let new_vertex = support_pair_world(collider_a, position_a, yaw_radians_a, collider_b, position_b, yaw_radians_b, search_direction);
        let projection: f32 = dot(new_vertex.minkowski_point, search_direction);
        let improvement: f32 = projection - closest_distance;

        if improvement < epsilon {
            let mut normal = search_direction;
            let relative_b_to_a = sub(position_a, position_b);
            if dot(normal, relative_b_to_a) < 0.0 { normal = negate(normal); }

            let mut depth: f32 = closest_distance;
            if depth < 0.0 { depth = 0.0; }

            *contact_out = Contact { normal, depth };
            return true;
        }

        if polytope_vertices.len() >= max_polytope_vertices {
            let mut normal = search_direction;
            let relative_b_to_a = sub(position_a, position_b);
            if dot(normal, relative_b_to_a) < 0.0 { normal = negate(normal); }

            let mut depth: f32 = closest_distance;
            if depth < 0.0 { depth = 0.0; }

            *contact_out = Contact { normal, depth };
            return false;
        }

        let new_vertex_index: i32 = polytope_vertices.len() as i32;
        polytope_vertices.push(new_vertex);

        edges.clear();

        for face_index in 0..faces.len() {
            if faces[face_index].obsolete { continue; }

            let index_a = faces[face_index].index_a;
            let face_point = polytope_vertices[index_a as usize].minkowski_point;
            let to_new_point = sub(polytope_vertices[new_vertex_index as usize].minkowski_point, face_point);

            if dot(faces[face_index].normal, to_new_point) > 0.0 {
                faces[face_index].obsolete = true;

                let index_b = faces[face_index].index_b;
                let index_c = faces[face_index].index_c;

                add_edge_pair(&mut edges, index_a, index_b);
                add_edge_pair(&mut edges, index_b, index_c);
                add_edge_pair(&mut edges, index_c, index_a);
            }
        }

        let mut face_index: usize = 0;
        while face_index < faces.len() {
            if faces[face_index].obsolete {
                faces.swap_remove(face_index);
            } else {
                face_index += 1;
            }
        }

        for edge_index in 0..edges.len() {
            let (index_a, index_b) = edges[edge_index];
            add_face(&polytope_vertices, &mut faces, index_a, index_b, new_vertex_index);
        }
    }

    let mut final_closest_face_index: i32 = -1;
    let mut final_closest_distance: f32 = f32::MAX;

    for face_index in 0..faces.len() {
        let face = &faces[face_index];
        if face.obsolete { continue; }
        if face.distance < final_closest_distance {
            final_closest_distance = face.distance;
            final_closest_face_index = face_index as i32;
        }
    }

    if final_closest_face_index >= 0 {
        let mut normal = faces[final_closest_face_index as usize].normal;

        let relative_b_to_a = sub(position_a, position_b);
        if dot(normal, relative_b_to_a) < 0.0 { normal = negate(normal); }

        let mut depth: f32 = final_closest_distance;
        if depth < 0.0 { depth = 0.0; }

        *contact_out = Contact { normal, depth };
        return false;
    }

    false
}

pub fn add_face(vertices: &Vec<GjkVertex>, faces: &mut Vec<EpaFace>, index_a: i32, index_b: i32, index_c: i32) {
    let point_a = vertices[index_a as usize].minkowski_point;
    let point_b = vertices[index_b as usize].minkowski_point;
    let point_c = vertices[index_c as usize].minkowski_point;

    let edge_ab = sub(point_b, point_a);
    let edge_ac = sub(point_c, point_a);
    let mut normal = cross(edge_ab, edge_ac);

    let length_squared: f32 = dot(normal, normal);

    if length_squared > 1e-12 {
        normal = mul(normal, 1.0 / length_squared.sqrt());
    } else {
        normal = normalize_small_vector(sub(point_b, point_c), DbVector3 { x: 0.0, y: 1.0, z: 0.0 });
    }

    let mut distance: f32 = dot(normal, point_a);
    let mut final_index_b = index_b;
    let mut final_index_c = index_c;

    if distance < 0.0 {
        normal = negate(normal);
        distance = -distance;
        let temp = final_index_b;
        final_index_b = final_index_c;
        final_index_c = temp;
    }

    faces.push(EpaFace { index_a, index_b: final_index_b, index_c: final_index_c, normal, distance, obsolete: false });
}

pub fn add_edge_pair(edges: &mut Vec<(i32, i32)>, index_a: i32, index_b: i32) {
    let mut edge_index: usize = 0;
    while edge_index < edges.len() {
        let (existing_a, existing_b) = edges[edge_index];
        if existing_a == index_b && existing_b == index_a {
            edges.swap_remove(edge_index);
            return;
        }
        edge_index += 1;
    }

    edges.push((index_a, index_b));
}


