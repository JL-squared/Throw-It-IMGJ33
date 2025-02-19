
#ifndef INTERSECT
#define INTERSECT

void intersectPlane_float(float3 position, float3 normal, float3 ray_start, float3 ray_dir, out bool hit, out float3 target_position) {
	float3 origin = position - ray_start;
	float dotted = dot(origin, normal) / dot(ray_dir, normal);
	target_position = ray_dir * dotted + ray_start;
	hit = dotted >= 0.0;
}

#endif