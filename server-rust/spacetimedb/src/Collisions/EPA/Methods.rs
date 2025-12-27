use crate::*;

pub fn EpaSolve(gjk: &GjkResult,collider_a: &Vec<ConvexHullCollider>, position_a: DbVector3, yaw_radians_a: f32, collider_b: &Vec<ConvexHullCollider>, position_b: DbVector3, yaw_radians_b: f32, contact_out: &mut Contact) -> bool {
    let max_iterations: i32 = 16;
    let epsilon: f32 = 2e-4;

    *contact_out = Contact { normal: DbVector3 { x: 0.0, y: 1.0, z: 0.0 }, depth: 0.0 };

    if gjk.simplex.len() < 4 { return false; }

    let mut polytope_vertices: Vec<GjkVertex> = gjk.simplex.clone();

    let mut faces: Vec<EpaFace> = Vec::new();
    AddFace(&polytope_vertices, &mut faces, 0, 1, 2);
    AddFace(&polytope_vertices, &mut faces, 0, 3, 1);
    AddFace(&polytope_vertices, &mut faces, 0, 2, 3);
    AddFace(&polytope_vertices, &mut faces, 1, 3, 2);

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

        let closest_face = &faces[closest_face_index as usize];
        let search_direction = closest_face.normal;

        let new_vertex = SupportPairWorld(collider_a, position_a, yaw_radians_a,collider_b, position_b, yaw_radians_b,search_direction,);

        let projection: f32 = Dot(new_vertex.minkowski_point, search_direction);
        let improvement: f32 = projection - closest_face.distance;

        if improvement < epsilon {
            let mut normal = closest_face.normal;
            let normal_length_sq: f32 = Dot(normal, normal);

            if normal_length_sq > 1e-12 {
                normal = Mul(normal, 1.0 / Sqrt(normal_length_sq));
            } 
            
            else {
                normal = NormalizeSmallVector(gjk.last_direction, DbVector3 { x: 0.0, y: 1.0, z: 0.0 });
            }

            let mut depth: f32 = closest_face.distance;
            if depth < 0.0 { depth = 0.0; }

            let relative_b_to_a = Sub(position_a, position_b);
            if Dot(normal, relative_b_to_a) < 0.0 { 
                normal = Negate(normal); 
            }

            *contact_out = Contact { normal, depth };
            return true;
        }

        let new_vertex_index: i32 = polytope_vertices.len() as i32;
        polytope_vertices.push(new_vertex);

        let mut edges: Vec<EpaEdge> = Vec::new();

        for face_index in 0..faces.len() {
            if faces[face_index].obsolete { continue; }

            let face_point = polytope_vertices[faces[face_index].index_a as usize].minkowski_point;
            let to_new_point = Sub(polytope_vertices[new_vertex_index as usize].minkowski_point, face_point);
            let dot_value: f32 = Dot(faces[face_index].normal, to_new_point);

            if dot_value > 0.0 {
                faces[face_index].obsolete = true;

                let index_a = faces[face_index].index_a;
                let index_b = faces[face_index].index_b;
                let index_c = faces[face_index].index_c;

                AddEdge(&mut edges, index_a, index_b);
                AddEdge(&mut edges, index_b, index_c);
                AddEdge(&mut edges, index_c, index_a);
            }
        }

        faces.retain(|face| face.obsolete == false);

        for edge_index in 0..edges.len() {
            let edge = &edges[edge_index];
            if edge.obsolete { continue; }
            AddFace(&polytope_vertices, &mut faces, edge.index_a, edge.index_b, new_vertex_index);
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
        let face = &faces[final_closest_face_index as usize];

        let mut normal = face.normal;
        let normal_length_sq: f32 = Dot(normal, normal);

        if normal_length_sq > 1e-12 {
            normal = Mul(normal, 1.0 / Sqrt(normal_length_sq));
        } 
        
        else {
            normal = NormalizeSmallVector(gjk.last_direction, DbVector3 { x: 0.0, y: 1.0, z: 0.0 });
        }

        let mut depth: f32 = face.distance;
        if depth < 0.0 { depth = 0.0; }

        let relative_b_to_a = Sub(position_a, position_b);
        if Dot(normal, relative_b_to_a) < 0.0 { 
            normal = Negate(normal); 
        }

        *contact_out = Contact { normal, depth };
        return false;
    }

    false
}

pub fn AddFace(vertices: &Vec<GjkVertex>, faces: &mut Vec<EpaFace>, index_a: i32, index_b: i32, index_c: i32) 
{
    let point_a = vertices[index_a as usize].minkowski_point;
    let point_b = vertices[index_b as usize].minkowski_point;
    let point_c = vertices[index_c as usize].minkowski_point;

    let edge_ab = Sub(point_b, point_a);
    let edge_ac = Sub(point_c, point_a);
    let mut normal = Cross(edge_ab, edge_ac);

    let length_squared: f32 = Dot(normal, normal);

    if length_squared > 1e-12 {
        let inverse_length: f32 = 1.0 / Sqrt(length_squared);
        normal = Mul(normal, inverse_length);
    } 
    
    else {
        normal = NormalizeSmallVector(Sub(point_b, point_c), DbVector3 { x: 0.0, y: 1.0, z: 0.0 });
    }

    let mut distance: f32 = Dot(normal, point_a);
    let mut final_index_b = index_b;
    let mut final_index_c = index_c;

    if distance < 0.0 {
        normal = Negate(normal);
        distance = -distance;
        let temp = final_index_b;
        final_index_b = final_index_c;
        final_index_c = temp;
    }

    faces.push(EpaFace { index_a, index_b: final_index_b, index_c: final_index_c, normal, distance, obsolete: false });
}

pub fn AddEdge(edges: &mut Vec<EpaEdge>, index_a: i32, index_b: i32) 
{
    for edge_index in 0..edges.len() {
        let existing_edge = &edges[edge_index];
        if existing_edge.obsolete == false && existing_edge.index_a == index_b && existing_edge.index_b == index_a {
            edges[edge_index].obsolete = true;
            return;
        }
    }

    edges.push(EpaEdge { index_a, index_b, obsolete: false });
}

pub fn ComputeContactNormal(raw_normal: DbVector3, center_a: DbVector3, center_b: DbVector3) -> DbVector3 
{
    let mut normal = raw_normal;
    if Dot(normal, normal) < 1e-6 { 
        return DbVector3 { x: 0.0, y: 1.0, z: 0.0 }; 
    }
    normal = Normalize(normal);

    let center_delta = Sub(center_a, center_b);
    let center_delta_sq: f32 = Dot(center_delta, center_delta);

    if center_delta_sq > 1e-8 {
        if Dot(normal, center_delta) < 0.0 { normal = Negate(normal); }
    }

    let world_up = DbVector3 { x: 0.0, y: 1.0, z: 0.0 };
    let up_dot: f32 = Dot(normal, world_up);

    let floor_snap_dot: f32 = 0.98;
    let ceiling_snap_dot: f32 = -0.98;
    let wall_snap_abs_dot: f32 = 0.05;

    if up_dot >= floor_snap_dot { 
        return world_up; 
    }

    if up_dot <= ceiling_snap_dot { 
        return DbVector3 { x: 0.0, y: -1.0, z: 0.0 }; 
    }

    if up_dot.abs() <= wall_snap_abs_dot {
        normal.y = 0.0;
        return Normalize(normal);
    }

    normal
}
